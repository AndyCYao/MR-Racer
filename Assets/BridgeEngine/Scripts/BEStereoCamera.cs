/*
 * This file is part of the Structure SDK.
 * Copyright © 2018 Occipital, Inc. All rights reserved.
 * http://structure.io
 *
 * Provides the stereo camera rendering infrastructure at runtime.
 * Intercepts the mainCamera rendering, so we can manually drive the left/right
 * stereo pair and lens distortion camera pair rendering.
 */
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.iOS;

using BEDummyGoogleVR;
namespace BEDummyGoogleVR {}

static class iPhoneXConstants {
	public static float kiPhoneXViewportWidthModifier = 0.781f;
	public static float kiPhoneXViewportHeightModifier = 0.771f;
	public static float kiPhoneXLensSpace = 17.5f;
}


public class BEStereoCamera : MonoBehaviour {
    private Camera mainCamera;

    public Camera leftCamera;
    public Camera rightCamera;

	private LensDistortionMesh leftEyeMesh;
	private LensDistortionMesh rightEyeMesh;

    private int cullingMask;
    private List<Canvas> mainScreenCanvas;


	private bool firstShot = true;

    void Awake() {
        mainCamera = GetComponent<Camera>();
    }

    /// Prepare the stereo cameras as a replacemetn for the main camera.
    void Start() {
#if UNITY_5_6_OR_NEWER
        // Prevent AllowMSAA which results in black rendering.
        Camera.main.allowMSAA = false;
#endif
        
        leftCamera = CreateEyeCamera("Left Eye");
		leftEyeMesh = CreateLensMesh("Left Lens Distortion", BEEyeSide.Left, leftCamera);

        rightCamera = CreateEyeCamera("Right Eye");
		rightEyeMesh = CreateLensMesh("Right Lens Distortion", BEEyeSide.Right, rightCamera);
    }

	/// Main Camera Callbacks
	void OnPreCull() {
		cullingMask = mainCamera.cullingMask;
		mainCamera.cullingMask = 0;
	}

	void OnPostRender() {
		mainCamera.cullingMask = cullingMask;

		RenderCamera(leftCamera, leftEyeMesh);
		RenderCamera(rightCamera, rightEyeMesh);
	}

	/// Update the tracking and projection of the cameras
    internal void UpdateStereoSetup( BEStereoSetup stereoSetup ) {
        // Get the Camera FOV setting and print it.
        if( firstShot ) {
            firstShot = false;
            float fov = calculateFovFromMatrix(stereoSetup.leftProjection.ToMatrix());
            Debug.Log("Camera FOV: " + fov);
        }

#if UNITY_5_6_OR_NEWER
        Vector3 leftPosition = stereoSetup.leftPosePosition.ToVector3();
        leftCamera.transform.SetPositionAndRotation(leftPosition, stereoSetup.leftPoseRotation.ToQuaternion());
        leftCamera.projectionMatrix = stereoSetup.leftProjection.ToMatrix();

        Vector3 rightPosition = stereoSetup.rightPosePosition.ToVector3();
        rightCamera.transform.SetPositionAndRotation(rightPosition, stereoSetup.rightPoseRotation.ToQuaternion());
        rightCamera.projectionMatrix = stereoSetup.rightProjection.ToMatrix();

        // NOTE: mainCamera in Mono mode by default is positioned exactly where the
        // color camera is physically which provides best color coverage in MR rendering.
        //  This means that main camera is typically up and to the left a little from the eye cameras.
        // 
        // Reset mainCamera position to mid-point between the eyes, so we get correct placement
        // of things like LostTracking billboard and other stuff developers want to put in front
        // of your eyes.
        mainCamera.transform.position = (leftPosition + rightPosition) * 0.5f;
#endif
    }


    //  ------------------ Private Methods --------------

    private float calculateFovFromMatrix( Matrix4x4 mat ){
        float b = mat[5];        
        float RAD2DEG = 180.0f / 3.14159265358979323846f;
        float fov = RAD2DEG * (2.0f * (float)Mathf.Atan(1.0f / b));
        return fov;
    }

    /// Convenience to create an Eye Camera for rendering into.
    private Camera CreateEyeCamera( string name ) {
        var cameraGO = new GameObject();
        var camera = cameraGO.AddComponent<Camera>();

        camera.CopyFrom(mainCamera);
        camera.name = name;
        camera.depth = -1;
        camera.enabled = false;

        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;

		camera.targetTexture = new RenderTexture(mainCamera.pixelWidth/2, mainCamera.pixelHeight, 16, RenderTextureFormat.Default);
        return camera;
    }

	// This mesh is used to render a texture with barrel distortion
	private LensDistortionMesh CreateLensMesh(string gameObjectName, BEEyeSide side, Camera eyeCamera) {
		var lensCameraGO = new GameObject(gameObjectName);
		var lensDistortionMesh = lensCameraGO.AddComponent<LensDistortionMesh>();
		lensDistortionMesh.sourceCamera = eyeCamera;
		lensDistortionMesh.side = side;
		lensDistortionMesh.CreateMesh();

		float lensSpace = 1.0f / (mainCamera.pixelWidth * 2.0f);
		float lensWidth = (Screen.width / 2 - lensSpace);
		float lensHeight = Screen.height;
		float lensXOffset = 0;
		float lensYOffset = 0;

		bool isIPhoneX = false;
#if UNITY_2017_3_OR_NEWER || UNITY_2017_2_1
		isIPhoneX = (Device.generation == DeviceGeneration.iPhoneX);
#else
		isIPhoneX = (Screen.width == 2436 && Screen.height == 1125);
#endif

		if  (isIPhoneX) {
			lensSpace = iPhoneXConstants.kiPhoneXLensSpace;
			float screenWidthX = (Screen.width / 2) * iPhoneXConstants.kiPhoneXViewportWidthModifier;
			lensXOffset = (Screen.width / 2) - screenWidthX;
			float adjustedHeight = lensHeight * iPhoneXConstants.kiPhoneXViewportHeightModifier;
			lensYOffset = (lensHeight - adjustedHeight) / 2f;
			lensWidth = lensWidth * iPhoneXConstants.kiPhoneXViewportWidthModifier;
			lensHeight = adjustedHeight;
		}

		if( side == BEEyeSide.Left ) {
			lensDistortionMesh.viewport = new Rect(lensXOffset, lensYOffset, lensWidth, lensHeight);
		} else {
			lensDistortionMesh.viewport = new Rect((Screen.width / 2 + lensSpace), lensYOffset, lensWidth, lensHeight);
		}


		return lensDistortionMesh;
	}

	private void RenderCamera( Camera eyeCamera, LensDistortionMesh mesh/*Camera lensCamera*/ ) {
		eyeCamera.cullingMask = cullingMask;
        eyeCamera.Render();

		if (mesh != null) {
			mesh.RenderLens ();
		}
    }
}
