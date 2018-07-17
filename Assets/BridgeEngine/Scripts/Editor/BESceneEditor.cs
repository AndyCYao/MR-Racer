using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (BEScene))]
public class BESceneEditor : Editor {

	public override void OnInspectorGUI() {
		BEScene beScene = (BEScene)target;

		if (DrawDefaultInspector ()) {
			beScene.ApplyCorrectMaterialAndLayerSetting ();
		}	
	}
}
