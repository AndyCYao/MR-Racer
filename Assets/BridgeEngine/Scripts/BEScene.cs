/*
 * This file is part of the Structure SDK.
 * Copyright © 2018 Occipital, Inc. All rights reserved.
 * http://structure.io
 *
 * Provide scene materials and rendering support for the loaded BridgeEngineScene.
 */
using UnityEngine;
using UnityEngine.Events;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

using BEDummyGoogleVR;
namespace BEDummyGoogleVR {}

public class BEScene : MonoBehaviour
{
    /// Top level BridgeEngineScene GameObject in scene
    public static string gameObjectName = "@BridgeEngineScene";

    /// Check if @BridgeEngineScene is present in the current Scene
    public static bool IsInScene()
    {
        return FindBEScene() != null;
    }

    public static BEScene FindBEScene()
    {
        return GameObject.FindObjectOfType<BEScene>();
    }



    public delegate void EnvironmentMeshCreatedDelegate();
    public static event EnvironmentMeshCreatedDelegate OnEnvironmentMeshCreated;

    /**
     * Choose the material rendering style.
     */
    public Material renderMaterial;

    /**
     * Track the current sceneMaterial, so we save on overhead inspecting materials.
     * Only apply changes when renderMaterial changes.
     */
    Material sceneMaterial = null;

    void Start() {
        gameObject.layer = LayerMask.NameToLayer ("Platform"); // 8 is Platform Layer

        ApplyCorrectMaterialAndLayerSetting();
        OnEnvironmentMeshCreated();
    }

    void Update() {
        if (sceneMaterial != renderMaterial){
            ApplyCorrectMaterialAndLayerSetting();
        }
    }

    /**
     * Apply the appropriate render material to the scanned mesh
     */
    public void ApplyCorrectMaterialAndLayerSetting()
    {
		sceneMaterial = renderMaterial;

        var beSceneLayer = gameObject.layer;
        var renderers = GetComponentsInChildren<MeshRenderer>();

        Material updateToMaterial = sceneMaterial;

// Substitute LiveCamProj texture with a standard VertexLit Diffuse one.
#if UNITY_EDITOR
        if( updateToMaterial && updateToMaterial.name.StartsWith("LiveCamProj")) {
            updateToMaterial = new Material(Shader.Find("Diffuse"));
            Debug.Log("Substituting " + sceneMaterial.name + " with " + updateToMaterial.name +" for running in Unity Simulator");
        }
#endif 

        foreach (var r in renderers)
        {
            r.gameObject.layer = beSceneLayer; // Apply Materials and Layer setting.
            r.sharedMaterial = updateToMaterial;
        }
    }

#if UNITY_EDITOR

    #region Paths

    public static string ImportFolderName()
    {
        return "ImportedBridgeEngineScene";
    }

    public static string EditorResourcesFolderPath()
    {
        return "Assets/Editor/Resources";
    }

    public static string ImportFolderPath()
    {
        return Path.Combine(EditorResourcesFolderPath(), ImportFolderName());
    }

    public static bool ImportFolderExists()
    {
        return Directory.Exists(ImportFolderPath());
    }	
	#endregion

    /// Construct a new top level @BridgeEngineScene object and set it up.
	public static void CreateNewBridgeEngineScene()
    {
        BEScene beScene = GameObject.FindObjectOfType<BEScene>();

        // Remove the old beScene GameObject.
        if (beScene != null)
        {
            Debug.Log("Replacing " + BEScene.gameObjectName +" in Hierarchy");
            GameObject.DestroyImmediate(beScene.gameObject);
        }

        Debug.Log("Creating " + BEScene.gameObjectName +" in Hierarchy");
        beScene = (new GameObject()).AddComponent<BEScene>();
        beScene.name = BEScene.gameObjectName;

        // Default the material to LiveColorProjShadow
        var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/BridgeEngine/Materials/LiveCamProjShadow.mat");
        beScene.renderMaterial = material;

        // Load mesh object from beSceneFolder.
		string beSceneFolderName = ImportFolderName();
        GameObject meshBaseObject = Resources.Load<GameObject>(Path.Combine(beSceneFolderName, "colorizedMesh"));
        if (meshBaseObject == null)
            meshBaseObject = Resources.Load<GameObject>(Path.Combine(beSceneFolderName, "coarseMesh"));

        if (meshBaseObject == null)
        {
            Debug.LogWarning("Could not load a mesh from either colourizedMeshor or coarseMesh");
            return;
        }

        var obj = GameObject.Instantiate<GameObject>(meshBaseObject);
        obj.tag = "EditorOnly";
        obj.AddComponent<EditorOnlyScript>();
        obj.transform.parent = beScene.transform;
        obj.transform.localScale = new Vector3(-1,-1,1);

		// Make all sub-meshes collidable, editorOnly, and removed at runtime
        MeshFilter[] meshes = obj.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter meshFilter in meshes)
        {
            MeshCollider collider = meshFilter.gameObject.AddComponent<MeshCollider>();
			collider.sharedMesh = meshFilter.sharedMesh;
            meshFilter.tag = "EditorOnly";
            meshFilter.gameObject.AddComponent<EditorOnlyScript>();
        }

        beScene.ApplyCorrectMaterialAndLayerSetting();

        Debug.Log("`BridgeEngineScene` folder imported");
    }
#endif
}

