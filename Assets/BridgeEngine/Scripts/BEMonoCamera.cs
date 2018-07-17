using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BEMonoCamera : MonoBehaviour {
	public Texture2D renderTexture;

	public Matrix4x4 colorCameraProjectionMatrix;
	public Matrix4x4 unityProjectionMatrix;

	private BEScene beScene = null;

	private Camera mainCamera;
	private Mesh mesh;
	private Material material;

	void Start() {
		mainCamera = this.GetComponent<Camera> ();
		mainCamera.clearFlags = CameraClearFlags.Nothing;
		beScene = BEScene.FindBEScene ();

		mesh = new Mesh();
		material = new Material(Shader.Find("Hidden/Occipital/LensDistortionMesh"));
	}

	void OnPreRender() {
		GL.Clear (true, true, Color.black);
		if (renderTexture != null &&
			beScene != null && 
			IsCameraShader(beScene.renderMaterial.shader.name)) {
				RenderCameraFeedToScreen (renderTexture);
		}
	}

	void RenderCameraFeedToScreen(Texture2D feed) {
		Matrix4x4 colorProjInverse = colorCameraProjectionMatrix.inverse;

		Vector4 minClip = new Vector4(-1, -1, -1, 1);
		Vector4 maxClip = new Vector4( 1,  1, -1, 1);

		Vector4 minFromCamera = colorProjInverse * minClip;
		Vector4 maxFromCamera = colorProjInverse * maxClip;

		Vector4 maxForRender = unityProjectionMatrix * maxFromCamera;
		Vector4 minForRender = unityProjectionMatrix * minFromCamera;

		// Create a square mesh in screenspace to render from
		Vector3[] vertices = new Vector3[4];
		Vector2[] uv = new Vector2[4];
		int[] indicies = new int[6];

		vertices [0] = new Vector3(minForRender.x, maxForRender.y, 0);
		vertices [1] = new Vector3(minForRender.x, minForRender.y, 0);
		vertices [2] = new Vector3(maxForRender.x, maxForRender.y, 0);
		vertices [3] = new Vector3(maxForRender.x, minForRender.y, 0);

		uv [0] = new Vector2 (0, 1);
		uv [1] = new Vector2 (0, 0);
		uv [2] = new Vector2 (1, 1);
		uv [3] = new Vector2 (1, 0);

		indicies [0] = 0;
		indicies [1] = 3;
		indicies [2] = 2;

		indicies [3] = 0;
		indicies [4] = 1;
		indicies [5] = 3;

		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = indicies;

		material.SetTexture("_MainTex", renderTexture);
		material.SetPass(0);
		Graphics.DrawMeshNow(mesh, Vector3.zero, Quaternion.identity);
	}

	bool IsCameraShader(string shaderName) {
		return shaderName.Contains ("LiveCam");
	}
}