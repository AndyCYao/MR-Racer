/*
 * This file is part of the Structure SDK.
 * Copyright © 2018 Occipital, Inc. All rights reserved.
 * http://structure.io
 * 
 * Provides the runtime interop and camera control from BridgeEngine.
 * 
 * USAGE:
 *   Drag & Drop the BridgeEngineMain prefab onto your scene.
 * 
 * OPERATION:
 *    When a structure sensor is attached and there's a mapped scene loaded, the sensor applies
 *    the real world 6DOF movement tracking to the main camera Transform.
 *    If no structure sensor is attached, or the mapped scene is not loaded, then BridgeEngineUnity
 *    falls back to standard 3DOF tracking.
 */

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.VR;
using UnityEngine.Rendering;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using BEDummyGoogleVR;
namespace BEDummyGoogleVR {}

[BEScriptOrder(-10)]
public class BridgeEngineUnity : MonoBehaviour {
    /// Check if BridgeEngineUnity is present in the current Scene
    public static bool IsInScene() {
        BridgeEngineUnity beunity = GameObject.FindObjectOfType<BridgeEngineUnity>();
        return beunity != null;
    }

    #if UNITY_EDITOR
    BESimulatorPoseController simPoseController;
    #endif

    // Shared static instance for all the interop callbacks.
    static BridgeEngineUnity instance = null;
	static public BridgeEngineUnity main {
		get{
            if(instance == null) {
                var beUnityInstances = GameObject.FindObjectsOfType<BridgeEngineUnity>();
        		Assert.IsTrue(beUnityInstances.Count() == 1, "We don't have a single BridgeEngineUnity instance");
                instance = beUnityInstances[0];
            }
            return instance;
        }
	}

	public bool stereoInEditor = false;

    #region Public Properties, Events, Accessors

    /**
     *  Return the last known pose accuracy.
     */
    public BETrackerPoseAccuracy TrackerPoseAccuracy() {
        return lastTrackerUpdate.trackerPoseAccuracy;
    }

    /**
     * Public event for updating when pose accuracy has changed.
     * This is also called immediately on first pose update.
     */
    [System.Serializable]
    public class TrackerPoseAccuracyEvent : UnityEvent<BETrackerPoseAccuracy>{}

    /**
     *  Public event for BEController's motion event.
     */
    [System.Serializable]
    public class ControllerMotionEvent : UnityEvent<Vector3, Quaternion>{}

    /**
     *  Public event for BEController's button event.
     */
    [System.Serializable]
    public class ControllerButtonEvent : UnityEvent<BEControllerButtons, BEControllerButtons, BEControllerButtons>{}

    /**
     *  Public event for BEController's touch event.
     */
    [System.Serializable]
    public class ControllerTouchEvent : UnityEvent<Vector2, BEControllerTouchStatus>{}


    [Header("Simulator Controls")]
    [Comment("Movement\n\nHold Left Shift to look around with the mouse,\nand move with keyboard.\n\nWS - Forward Backward\nAD - Left Right\nQE - Down Up\n\nTracking\n\nTo toggle between tracking modes press one\nof the following number keys:\n\n1 - Not Available\n2 - Low\n3 - High", 235)]

    [RangeAttribute(0.0f, 10f)]
    [TooltipAttribute("Default 3.0")]
    public float mouseSensitivity = 3;
    [RangeAttribute(0.0f, 10f)]
    [TooltipAttribute("Default 1.5")]
    public float moveSpeed = 1.5f;

    /// Determine if we're using the Metal API for rendering.
    private bool UsingMetal {
        get { return SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal; }
    }

    /// Adjust the Color Camera projection matrix, so it adapts to
    /// the the scale of the retina resolution screen.
    private Matrix4x4 _scaleBiasMatrixColor_ = new Matrix4x4();

    [Header("Tracking Status")]

    /// Called every update of the camera pose.
    public UnityEvent onPoseChanged;

    /// Called whenever the pose accuracy changes.
    public TrackerPoseAccuracyEvent onPoseAccuracyChanged;

    /// Detemine if we started up in Stereo or Mono mode.
    public bool isStereoModeActive {get; private set;}

    private Camera mainCamera;

	[HideInInspector]
	public Texture2D cameraTexture = null;

    [Header("Controller Events")]
    /**
     * Remote connected to BridgeEngine
     */
    public UnityEvent onControllerDidConnect;

    /**
     * Remote disconnected from BridgeEngine
     */
    public UnityEvent onControllerDidDisconnect;

    /**
     * Controller motion event. 
     * @parameter position World position
     * @parameter rotationQuaternion World rotation in quaternions.
    */
    public ControllerMotionEvent onControllerMotionEvent;

    /**
     * Remote button event. 
     * @parameter current Current state of all buttons (1 - held down, 0 - released)
     * @parameter down Buttons that have changed to down state (1 - click down, 0 - no change)
     * @parameter up Buttons that have changed to released state (1 - released, 0 - no change)
    */
    public ControllerButtonEvent onControllerButtonEvent;

    /**
     * Remote touch event. 
     *  Position - relative position across the touch pad.
     *  Status - active tracking touch status
     */
    public ControllerTouchEvent onControllerTouchEvent;

    #endregion

	#region Private Variables
	private BEStereoCamera stereoCamera;
    private BEMonoCamera monoCamera;
	#endregion


    /**
	 * Get the main camera, and annouce if something is misconfigured.
	 */
	Camera GetMainCamera()
	{
		if (mainCamera != null)
            return mainCamera;
        
		mainCamera = Camera.main;
        
		if (mainCamera == null) {
			Debug.LogError ("Cannot find main camera, make sure you have marked your camera with the MainCamera tag");
		}

		return mainCamera;
	}

    #region Unity Methods

	void Awake() {
        _scaleBiasMatrixColor_.SetRow(0, new Vector4(/*c1*/ 0.5f, 0.0f, 0.0f, 0.5f)); 
        _scaleBiasMatrixColor_.SetRow(1, new Vector4(/*c2*/ 0.0f, 0.5f, 0.0f, 0.5f)); 
        _scaleBiasMatrixColor_.SetRow(2, new Vector4(/*c3*/ 0.0f, 0.0f, 0.5f, 0.5f)); 
        _scaleBiasMatrixColor_.SetRow(3, new Vector4(/*c4*/ 0.0f, 0.0f, 0.0f, 1.0f)); 

        UnityEngine.XR.XRSettings.enabled = false; // Force VR mode off, because BridgeEngine will drive it with our own camera rig.
        Screen.sleepTimeout = SleepTimeout.NeverSleep; // Never sleep for MR/VR. We use controller input.

		#if UNITY_EDITOR
			isStereoModeActive = stereoInEditor;
		#else
			isStereoModeActive = BridgeEngineUnityInterop.be_isStereoMode();
		#endif

		// Relocate the LostTrackingCanvas onto the MainCamera.
		mainCamera = GetMainCamera();
		Assert.IsNotNull(mainCamera, "BridgeEngineUnity is missing main camera!");

		var lostTrackingBillboard = gameObject.GetComponentInChildren<BELostTrackingBillboard>(includeInactive:true);
		Assert.IsNotNull(lostTrackingBillboard, "BridgeEngineUnity is missing a child LostTrackingBillboard for announcing tracking loss");

		if (mainCamera && lostTrackingBillboard) {
            // We leave the lostTrackingCanvas disabled by default, so it doesn't interfere with the user's work.
            lostTrackingBillboard.gameObject.SetActive(true);

			// Inherit the camera's local Z for placing the canvas plane, but no closer than 0.3 so it's a good distance.
			float visibleNearZ = mainCamera.nearClipPlane + 0.01f;
			var canvasLocalPosition = lostTrackingBillboard.transform.transform.localPosition;
			canvasLocalPosition.z = Math.Max( visibleNearZ, 0.30f );
			lostTrackingBillboard.transform.transform.localPosition = canvasLocalPosition;
			lostTrackingBillboard.transform.SetParent (mainCamera.transform, false); // Reparent, keeping local transform.
		}
	}

	/**
	 * Detect if there's a duplicate instance of BridgeEngineUnity main script loaded,
	 * as this could seriousely muck-up interop to BE.
	 * Set up the camera near and far clipping planes, and register the interop callbacks.
	 */
	IEnumerator Start () {
#if !UNITY_EDITOR
        // Bridge Engine is guaranteed to be ready on start.
		// Immediately load the mesh, only if we have a BEScene to load them into.
		if (BEScene.IsInScene())
		{
			BridgeEngineUnityInterop.be_loadMeshes(ScanMeshCallback);
		} else {
            Debug.Log("No @BridgeEngineScene present, skipping loading world meshes");
        }
#endif

		if (isStereoModeActive) {
			stereoCamera = mainCamera.gameObject.AddComponent<BEStereoCamera>();
		} else {
			monoCamera = mainCamera.gameObject.AddComponent<BEMonoCamera>();
		}

        Application.targetFrameRate = 60;
        lastTrackerUpdate.trackerPoseAccuracy = BETrackerPoseAccuracy.Uninitialized;

		// Configure the Tracking UIs BridgeEngineUnity instance.
		var lostTrackingBillboard = gameObject.GetComponentInChildren<BELostTrackingBillboard>(includeInactive:true);
		if (lostTrackingBillboard) {
			lostTrackingBillboard.beUnity = this;
		}

        // Register for callbacks.
        RegisterTrackerCallback();
        ControllerRegisterAndStart();

		return HideOverlayAfterSplash ();
	}

	IEnumerator HideOverlayAfterSplash()
	{
		while (!SplashScreen.isFinished)
		{
			SplashScreen.Draw();
			yield return null;
		}

		BridgeEngineUnityInterop.be_hideOverlay ();
	}


#if UNITY_EDITOR
    void OnApplicationQuit() {
        // Only shutdown the controller when we're in Simulator Play mode.
        ControllerShutdown();
    }
#endif

    void Update () {
#if UNITY_EDITOR
        // When running in editor / simulator, allow user to move around with keybaord and mouse.
        // The movement input is then injected back into Unity as though it came from the BridgeEngine.

        simPoseController.speed = moveSpeed;
        simPoseController.sensitivity = mouseSensitivity;
        simPoseController.UpdateTracker();

#else // UNITY_EDITOR
        // When running on device, allow touches to mimic pulling the Primary button (aka Trigger on the Bridge Controller).
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
            // Press Primary
            ControllerButtonEventCallback( BEControllerButtons.ButtonPrimary, BEControllerButtons.ButtonPrimary, 0);
        }

        if (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled)) {
            // Release Primary
            ControllerButtonEventCallback( 0, 0, BEControllerButtons.ButtonPrimary);
        }
#endif // UNITY_EDITOR
    }

    #endregion

    #region Interop Methods
    BETrackerUpdateInterop lastTrackerUpdate;

    //---- BridgeEngine Tracker Interop ---
    private void RegisterTrackerCallback() {
        BridgeEngineUnityInterop.be_registerTrackerEventCallback(BridgeEngineUnity.TrackerUpdateCallback);
    }

    //this is the assigned method here that will get called from the plugin
    [BridgeEngineUnityInterop.MonoPInvokeCallback (typeof(BridgeEngineUnityInterop.trackerUpdateInterop))]
    private static void TrackerUpdateCallback(BETrackerUpdateInterop _trackerUpdateInterop)
    {
        if (BridgeEngineUnity.main != null) {
            BridgeEngineUnity.main.TrackerUpdate(ref _trackerUpdateInterop);
        }
    }

    /// Update the LiveColor texturemap that is projected onto the BEScene world geometry.
    void CameraTextureUpdate(ref BETextureInfo cameraTextureInfo)
    {
        IntPtr cameraTextureId = cameraTextureInfo.texturePtr;
        if (cameraTextureId == IntPtr.Zero)
        {
            cameraTexture = Texture2D.whiteTexture;
            Shader.SetGlobalTexture("_u_CameraTex", cameraTexture);
        }
        else
        {
            var projection = cameraTextureInfo.texturePerspectiveProj.ToMatrix();
			Matrix4x4 worldToLocal = Matrix4x4.TRS(cameraTextureInfo.pos.ToVector3(), 
												   cameraTextureInfo.rot.ToQuaternion(),
												   cameraTextureInfo.scale.ToVector3()).inverse;

			Shader.SetGlobalMatrix("_colorCameraViewProjMatrix", _scaleBiasMatrixColor_ *  projection * worldToLocal);

            bool needToRecreate = cameraTexture == null ||
                                  cameraTexture.width != cameraTextureInfo.width ||
                                  cameraTexture.height != cameraTextureInfo.height;
            if (needToRecreate)
            {
                cameraTexture = Texture2D.CreateExternalTexture(cameraTextureInfo.width, 
                                                                cameraTextureInfo.height, 
                                                                TextureFormat.RGBA32, false, false, 
                                                                cameraTextureId);

                Shader.SetGlobalTexture("_u_CameraTex", cameraTexture);
            }
            else if (cameraTexture.GetNativeTexturePtr() != cameraTextureId)
            {
                cameraTexture.UpdateExternalTexture(cameraTextureId);
            }
        }

        #if UNITY_EDITOR    // TODO: move to BESimulatorPoseController
        if( stereoCamera != null )
        {
            var projection = mainCamera.projectionMatrix;
            var middlePoint = mainCamera.transform.position;
            var worldToLocal = Matrix4x4.TRS(middlePoint, mainCamera.transform.rotation, Vector3.one).inverse;
            
            //Shader.SetGlobalMatrix("_cameraProjMatrix", _scaleBiasMatrixColor_ * projection * worldToLocal);
		}
        #endif

        // Refresh the monoCamera background texture
        #if !UNITY_EDITOR
        if (monoCamera != null) {
            if( monoCamera.renderTexture != cameraTexture ) {
    			monoCamera.renderTexture = cameraTexture;
            }

			monoCamera.colorCameraProjectionMatrix = lastTrackerUpdate.cameraTextureInfo.texturePerspectiveProj.ToMatrix();
			monoCamera.unityProjectionMatrix = lastTrackerUpdate.cameraProj.ToMatrix ();
		}
        #endif
    }

    /**
     * Receive the 6DOF tracker updates from BridgeEngine.
     */
	void TrackerUpdate(ref BETrackerUpdateInterop interop) {
        lastTrackerUpdate = interop;

        if (mainCamera != null)
        {
#if !UNITY_EDITOR
			mainCamera.projectionMatrix = lastTrackerUpdate.cameraProj.ToMatrix();  // uses the intrinsics from Bridge Engine to get a correct projection matrix
#endif
			mainCamera.transform.position = lastTrackerUpdate.pos.ToVector3 ();
            mainCamera.transform.rotation = lastTrackerUpdate.rot.ToQuaternion ();
			mainCamera.transform.localScale = lastTrackerUpdate.scale.ToVector3 ();
        }

		if (isStereoModeActive && stereoCamera != null) {
			stereoCamera.UpdateStereoSetup (lastTrackerUpdate.stereoSetup);
		}
		
        // Update the BEController's cameraTransform, from our latest mainCamera tracking.
        Matrix4x4 unityCameraWorldMatrix = mainCamera.transform.localToWorldMatrix;
        beMatrix4 beCameraWorldMatrix = new beMatrix4();
        beCameraWorldMatrix.Set(unityCameraWorldMatrix);
        BridgeEngineUnityInterop.beControllerUpdateCamera(beCameraWorldMatrix);

        // Update the listeners if pose accuracy changes.
        bool poseAccuracyChanged = lastTrackerUpdate.trackerPoseAccuracy != interop.trackerPoseAccuracy;
        if (poseAccuracyChanged && onPoseAccuracyChanged != null)
        {
            onPoseAccuracyChanged.Invoke(interop.trackerPoseAccuracy);
        }

        if( onPoseChanged != null ) {
            onPoseChanged.Invoke();
        }

		// Refresh the camera texture last, so we have an opportunity to update the color samplers.
        CameraTextureUpdate(ref lastTrackerUpdate.cameraTextureInfo);
    }

    //---- Controller Interop ---
    private void ControllerRegisterAndStart() {
#if UNITY_EDITOR
        simPoseController = new BESimulatorPoseController();
        simPoseController.trackerUpdateCallback = BridgeEngineUnity.TrackerUpdateCallback;
        simPoseController.controllerMotionEventCallback = BridgeEngineUnity.ControllerMotionEventCallback;
        simPoseController.controllerButtonEventCallback = BridgeEngineUnity.ControllerButtonEventCallback;
        simPoseController.controllerTouchEventCallback = BridgeEngineUnity.ControllerTouchEventCallback;

        simPoseController.StartWithMainCameraTransform( mainCamera.transform );
#endif
        // Register for controller changes
#if UNITY_EDITOR
        // When in Unity Simulator, don't use the shared controller so it releases after play.
        BridgeEngineUnityInterop.beControllerInit( false );
#else
        // When on Device, use the shared controller because BE got it first and we need to use the same.
        BridgeEngineUnityInterop.beControllerInit( true );
#endif

        BridgeEngineUnityInterop.be_registerControllerEventDidConnectCallback(BridgeEngineUnity.ControllerDidConnectCallback);
        BridgeEngineUnityInterop.be_registerControllerEventDidDisconnectCallback(BridgeEngineUnity.ControllerDidDisconnectCallback);
        BridgeEngineUnityInterop.be_registerControllerMotionEventCallback(BridgeEngineUnity.ControllerMotionEventCallback);
        BridgeEngineUnityInterop.be_registerControllerButtonEventCallback(BridgeEngineUnity.ControllerButtonEventCallback);
        BridgeEngineUnityInterop.be_registerControllerTouchEventCallback(BridgeEngineUnity.ControllerTouchEventCallback);
    }

    void ControllerShutdown() {
        BridgeEngineUnityInterop.beControllerShutdown();
    }

    public bool isBridgeControllerConnected() {
        return BridgeEngineUnityInterop.beControllerIsBridgeControllerConnected();
    }

    // Controllers callbacks
    [BridgeEngineUnityInterop.MonoPInvokeCallback (typeof(BridgeEngineUnityInterop.voidInterop))]
    private static void ControllerDidConnectCallback()
    {
        if (BridgeEngineUnity.main != null) {
            BridgeEngineUnity.main.ControllerDidConnect();
        }
    }

    /**
     * Called when the controller is paired and ready to send events.
     */
    private void ControllerDidConnect()
    {
        if (onControllerDidConnect != null)
            onControllerDidConnect.Invoke();
    }


    [BridgeEngineUnityInterop.MonoPInvokeCallback (typeof(BridgeEngineUnityInterop.voidInterop))]
    private static void ControllerDidDisconnectCallback()
    {
        if (BridgeEngineUnity.main != null) {
            BridgeEngineUnity.main.ControllerDidDisconnect();
        }
    }

    /**
     * Called when the controller got disconnected.
     */
    private void ControllerDidDisconnect()
    {
        if (onControllerDidDisconnect != null)
            onControllerDidDisconnect.Invoke();
    }

    /**
     * Called on controller movement
     */
    [BridgeEngineUnityInterop.MonoPInvokeCallback (typeof(BridgeEngineUnityInterop.controllerMotionInterop))]
    private static void ControllerMotionEventCallback(beVector3 position, beVector4 rotationQuaternion)
    {
        if (BridgeEngineUnity.main != null && BridgeEngineUnity.main.onControllerMotionEvent != null) {
            BridgeEngineUnity.main.onControllerMotionEvent.Invoke(position.ToVector3(), rotationQuaternion.ToQuaternion());
        }
    }

    /**
     * Called when the controller button is pressed
     */
    [BridgeEngineUnityInterop.MonoPInvokeCallback (typeof(BridgeEngineUnityInterop.controllerButtonsInterop))]
    private static void ControllerButtonEventCallback(BEControllerButtons current, BEControllerButtons down, BEControllerButtons up)
    {
        if (BridgeEngineUnity.main != null && BridgeEngineUnity.main.onControllerButtonEvent != null) {
            BridgeEngineUnity.main.onControllerButtonEvent.Invoke(current, down, up);
        }
    }

    [BridgeEngineUnityInterop.MonoPInvokeCallback (typeof(BridgeEngineUnityInterop.controllerTouchInterop))]
    private static void ControllerTouchEventCallback(beVector2 position, BEControllerTouchStatus status)
    {
        if (BridgeEngineUnity.main != null) {
            BridgeEngineUnity.main.ControllerOnTouchEvent(position.ToVector2(), status);
        }
    }

    /**
     * Called when the controller button is pressed
     */
    void ControllerOnTouchEvent(Vector2 position, BEControllerTouchStatus status)
    {
        if (onControllerTouchEvent != null)
            onControllerTouchEvent.Invoke(position, status);
    }
    
    [BridgeEngineUnityInterop.MonoPInvokeCallback (typeof(BridgeEngineUnityInterop.meshInterop))]
    private static void ScanMeshCallback(int meshIndex, int meshCount, int verticesCount, IntPtr positions, IntPtr normals, IntPtr colors, IntPtr uvs, int indiciesCount, IntPtr indicies16)
    {
        if (BridgeEngineUnity.main != null) {
            BridgeEngineUnity.main.ScannedMeshTransfer(meshIndex, meshCount, verticesCount, positions, normals, colors, uvs, indiciesCount, indicies16);
        }
    }

    /**
     * Called when mesh i / n is scanned, so we can re-create it in Unity
     */
    void ScannedMeshTransfer(int meshIndex, int meshCount, int verticesCount, IntPtr positions, IntPtr normals, IntPtr colors, IntPtr uvs, int indiciesCount, IntPtr indicies16)
    {
        string desc = string.Format("Scanned Object {0}/{1}", meshIndex + 1, meshCount);
        // Debug.LogFormat("ScannedMeshTransfer. {0} / {1}. verticesCount = {2}. indiciesCount = {3}", meshIndex, meshCount, verticesCount, indiciesCount);

        if (meshCount <= 0) return;
        if (meshIndex >= meshCount) return;

        // Make sure we have a valid BEScene object to attach the meshes to.
        var beSceneObject = BEScene.FindBEScene();

        // We expect a valid scene, or something's wrong.
        if( beSceneObject == null ) {
            Debug.LogFormat("BridgeEngineScene not found, couldn't load mesh \"{0}\"", desc);
            return;
        }

        var meshInTransfer = new Mesh();
        
        // strings allocate
        string meshDesc = "[Mesh]" + desc;

        meshInTransfer.name = meshDesc;
        meshInTransfer.subMeshCount = 1;
        meshInTransfer.vertices = BridgeEngineUnityInterop.GetNativeArray<Vector3>(positions, verticesCount);
        int[] indxArray = BridgeEngineUnityInterop.GetNativeIndxArray(indicies16, indiciesCount);
        
        bool useForRendering = true;
        if (useForRendering)
        {
            meshInTransfer.normals = BridgeEngineUnityInterop.GetNativeArray<Vector3>(normals, verticesCount);
            meshInTransfer.colors = BridgeEngineUnityInterop.GetNativeArray<Color>(colors, verticesCount);
            meshInTransfer.uv = BridgeEngineUnityInterop.GetNativeArray<Vector2>(uvs, verticesCount);
            
            if (meshInTransfer.normals == null) {
                meshInTransfer.RecalculateNormals();
            }
        }

        meshInTransfer.SetIndices(indxArray, MeshTopology.Triangles, 0, true);
        
        var meshObject = new GameObject(desc);
        meshObject.transform.SetParent( beSceneObject.transform, false ); // Don't do any magic with the BEScene transform.

        if (useForRendering)
        {
            meshObject.AddComponent<MeshFilter>().sharedMesh = meshInTransfer;
            meshObject.AddComponent<MeshRenderer>();
        }

        // Make sure the mesh has physics
        MeshCollider collider = meshObject.GetComponent<MeshCollider>() ?? meshObject.AddComponent<MeshCollider>();
        collider.sharedMesh = meshInTransfer;

        // transfer complete
        if (meshIndex == meshCount - 1)
        {
            beSceneObject.ApplyCorrectMaterialAndLayerSetting();
            Debug.Log("ScannedMeshTransfer. Complete, applying materials");
        }
    }

    #endregion
}


#region DataStructure_for_interop

// Enums for BridgeEngineUnity
#region Enums

/**
 * Touch status from BEEngine.h BEControllerTouchStatus
 */
public enum BEControllerTouchStatus : int {
    TouchIdle = 0,
    TouchFirstContact = 1,
    TouchReleaseContact = 2,
    TouchMove = 3
}

/**
 * Bridge controller button bit values from BEEngine.h BEControllerTouchStatus
 */
[Flags]
public enum BEControllerButtons : int {
    ButtonPrimary   = 1<<0, // Trigger pulled or CODAWheel clicker
    ButtonSecondary = 1<<1, // App button with (•••)
    ButtonHomePower = 1<<2, // Home/Power button with (o)
    ButtonTouchpad     = 1<<3, // Touch pad clicker pressed
    ButtonTouchContact = 1<<4, // Touch pad contact with finger
}

/**
 * Side that the camera is on.
 */
public enum BEEyeSide : int {
    Mono              = 0,
    Left              = 1,
    Right             = 2,
};

/**
 * Tracking state from BEEngine.h BETrackerPoseAccuracy
 */
public enum BETrackerPoseAccuracy : int {
    Uninitialized = -1, // Before first pose is delivered, this is the default.
    NotAvailable = 0,
    High = 1,
    Low = 2,
}

#endregion

/// <summary>
/// ST tracker update struct for interop with the SDK
/// this struct keeps all of the chunks of information
/// coming in from the sensor's tracking movements
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct BETrackerHintsInterop
{
    public int isOrientationOnly; // 0 or 1
    public float modelVisibilityPercentage;
    public int scannedAreaNotVisible; // 0 or 1
}

/// <summary>
/// Structure that describes stereo setup for left / right cameras
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct BEStereoSetup
{
    public beVector3 leftPosePosition;
    public beVector4 leftPoseRotation;
    public beMatrix4 leftProjection;
    
    public beVector3 rightPosePosition;
    public beVector4 rightPoseRotation;
    public beMatrix4 rightProjection;
}

/// <summary>
/// Structure that describes a texture from native plugin
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct BETextureInfo
{
    public IntPtr texturePtr;
    public int width;
    public int height;
    public beMatrix4 texturePerspectiveProj;
    public beMatrix4 textureViewpoint;
	public beVector3 pos;
	public beVector3 scale;
	public beVector4 rot;
}

[StructLayout(LayoutKind.Sequential)]
internal struct BETrackerUpdateInterop
{
    public double timestamp;
    public beVector3 pos;
    public beVector3 scale;
    public beVector4 rot;
    public BETrackerPoseAccuracy trackerPoseAccuracy;
    public BETrackerHintsInterop trackerHints;
    public BEStereoSetup stereoSetup;
    public BETextureInfo cameraTextureInfo;
	public beMatrix4 cameraProj;
}

#endregion

/// <summary>
/// This is a utility class used to keep all of the plugin specific methods in one place
/// you shouldn't need to do too many modifications directly in here
/// </summary>
internal partial class BridgeEngineUnityInterop
{
    //defines the signature for the mono function
    public delegate void trackerUpdateInterop(BETrackerUpdateInterop _trackerUpdateInterop);
    public delegate void voidInterop();
    public delegate void meshInterop(int meshIndex, int meshCount, int verticesCount, IntPtr positions, IntPtr normals, IntPtr colors, IntPtr uvs, 
                                     int indiciesCount, IntPtr indicies16);
    public delegate void controllerMotionInterop(beVector3 position, beVector4 rotationQuaternion);
    public delegate void controllerButtonsInterop(BEControllerButtons current, BEControllerButtons down, BEControllerButtons up);
    public delegate void controllerTouchInterop(beVector2 position, BEControllerTouchStatus status);

    public static T[] GetNativeArray<T>(IntPtr array, int length)
    {
        if (array == IntPtr.Zero)
            return null;

        T[] result = new T[length];
        int size = Marshal.SizeOf (typeof(T));

        if (IntPtr.Size == 4) {
            // 32-bit system
            for (int i = 0; i < result.Length; i++) {
                result [i] = (T)Marshal.PtrToStructure (array, typeof(T));
                array = new IntPtr (array.ToInt32 () + size);
            }
        } else {
            // probably 64-bit system
            for (int i = 0; i < result.Length; i++) {
                result [i] = (T)Marshal.PtrToStructure (array, typeof(T));
                array = new IntPtr (array.ToInt64 () + size);
            }
        }
        return result;
    }
    
    public static int[] GetNativeIndxArray(IntPtr arrayInt16, int length)
    {
         int[] result = new int[length];
         int size = sizeof(Int16);
 
         if (IntPtr.Size == 4) {
             // 32-bit system
             for (int i = 0; i < result.Length; i++) {
                 result [i] = (int)Marshal.ReadInt16(arrayInt16);
                 arrayInt16 = new IntPtr (arrayInt16.ToInt32 () + size);
             }
         } else {
             // probably 64-bit system
             for (int i = 0; i < result.Length; i++) {
                 result [i] = (int)Marshal.ReadInt16(arrayInt16);
                 arrayInt16 = new IntPtr (arrayInt16.ToInt64 () + size);
             }
         }
         return result;
    }

    #if UNITY_EDITOR
    const string AUTO_IMPORT_PATH="BEUnityEditorPlugin";
    #else
    const string AUTO_IMPORT_PATH="__Internal";
    #endif

    // get tracked movement update events
    #if UNITY_EDITOR
    public static void be_registerTrackerEventCallback(trackerUpdateInterop callback) {}
    #else
    [DllImport ("__Internal")]
    public static extern void be_registerTrackerEventCallback(trackerUpdateInterop callback);
    #endif
    
    // get scanned mesh data
    #if UNITY_EDITOR
    // Future: callback interface for loading a mesh into simulator.  For now it's auto-loaded an OBJ from the BridgeEngineScene folder.
    // [DllImport (AUTO_IMPORT_PATH)]
    // public static extern void be_loadMeshes(meshInterop callback);

    /// Load all the meshes (BE is guaranteed to be ready on Awake)
    public static void be_loadMeshes(meshInterop callback) {}
    #else
    [DllImport ("__Internal")]
    public static extern void be_loadMeshes(meshInterop callback);
    #endif

    [DllImport (AUTO_IMPORT_PATH)]
    public static extern void beControllerInit( bool useShared );
    [DllImport (AUTO_IMPORT_PATH)]
    public static extern void beControllerShutdown();
    [DllImport (AUTO_IMPORT_PATH)]
    public static extern void beControllerUpdateCamera( beMatrix4 cameraTransform );
    [DllImport (AUTO_IMPORT_PATH)]
    public static extern bool beControllerIsBridgeControllerConnected();
    [DllImport (AUTO_IMPORT_PATH)]
    public static extern void be_registerControllerEventDidConnectCallback(voidInterop callback);
    [DllImport (AUTO_IMPORT_PATH)]
    public static extern void be_registerControllerEventDidDisconnectCallback(voidInterop callback);
    [DllImport (AUTO_IMPORT_PATH)]
    public static extern void be_registerControllerMotionEventCallback(controllerMotionInterop callback);
    [DllImport (AUTO_IMPORT_PATH)]
    public static extern void be_registerControllerButtonEventCallback(controllerButtonsInterop callback);
    [DllImport (AUTO_IMPORT_PATH)]
    public static extern void be_registerControllerTouchEventCallback(controllerTouchInterop callback);

    // Render the material
    #if UNITY_EDITOR
	public static bool be_isStereoMode() { return true; }
    #else
    [DllImport ("__Internal")]
    public static extern bool be_isStereoMode();
    #endif

	// Hide the overlay
	#if UNITY_EDITOR
	public static void be_hideOverlay() {}
	#else
	[DllImport ("__Internal")]
	public static extern void be_hideOverlay();
	#endif


    // this creates the type def for the plugin to communicate
    // with the mono pointers
    [AttributeUsage (AttributeTargets.Method)]
    public sealed class MonoPInvokeCallbackAttribute : Attribute
    {
        public MonoPInvokeCallbackAttribute (Type t) {}
    }
}
