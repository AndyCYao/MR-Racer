/*
 * This file is part of the Structure SDK.
 * Copyright © 2018 Occipital, Inc. All rights reserved.
 * http://structure.io
 * 
 * Automatically detect if GoogleVRForUnity is missing from the project.
 * Offer to download & install it.
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// This dummy namespace is used for getting past compile-time dependencies
// by filling them in with temporary class stubs in GoogleVRDummy.cs
//
// Upon successful detection, then GoogleVRDummy.cs is deleted and scripts are reloaded.
//
// It's necessary to keep this namespace around for BEEye.cs and BridgeEngineUnity.cs
// They both have "using BEDummyGoogleVR;"
using BEDummyGoogleVR;
namespace BEDummyGoogleVR {}

#if UNITY_EDITOR

[UnityEditor.InitializeOnLoad]
public class BEGoogleVRDownloader
{
	private static readonly string GOOGLE_VR_VERSION = "1.110.0";  // GoogleVR 1.110.0 is the version on 1.120.0 release. That's correct, Google posted an incorrect version.
	private static readonly string GOOGLE_VR_INSTALL_URL = "https://bridge.occipital.com/files/GoogleVRForUnity_1.120.0.unitypackage";

	private static readonly string INSTALL_IS_IN_PROGRESS_PATH = "BridgeEngineGVRInstallInProgress.check";
	private static readonly string IGNORE_GOOGLE_VR_CHECK_PATH = "BridgeEngineIgnoreGVR.check";
	private static readonly string REENABLE_COMPATIBILITY_CHECK_MESSAGE =
		"GoogleVR check can be re-enabled by deleting " + IGNORE_GOOGLE_VR_CHECK_PATH;

	static WWW downloader = null;

    static BEGoogleVRDownloader()
    {
		if (UnityEditorInternal.InternalEditorUtility.inBatchMode || UnityEditorInternal.InternalEditorUtility.isHumanControllingUs == false)
			return;

		// Defer the install check and prompts until after Unity EditorApplication
		// completes loading and hits the first update loop.
		EditorApplication.update += RunOnceInstallCheck;
	}

	static void RunOnceInstallCheck() {
		EditorApplication.update -= RunOnceInstallCheck;

		if (IsIgnoreGoogleVRCheck() == false)
		{
			bool isGoogleVRDetected = DetectGoogleVR();
			if (isGoogleVRDetected)
			{
				DeleteGoogleVRDummy();
				DeleteGoogleVRUnityPackagePath();
				DeleteInstallInProgress();
			}
			else if (IsInstallInProgress() == false)
			{
				int option;

#pragma warning disable CS0162 // One of these code paths is valid, depending on what version of GoogleVR is installed.  But the C# compiler doesn't know that.

				if( GvrUnitySdkVersion.GVR_SDK_VERSION == "Not Installed" ) {
					option = UnityEditor.EditorUtility.DisplayDialogComplex("GoogleVR is missing",
						"To use Bridge Engine with Unity you need the GoogleVR library. Do you want to download it?",
						"Yes, Download",
						"No and never ask",
						"No");
				} else {
					string message = string.Format(
						"To use Bridge Engine with Unity you need GoogleVR {0}, you have {1}.  "
						+"Do you want to download and replace with the correct version?", GOOGLE_VR_VERSION, GvrUnitySdkVersion.GVR_SDK_VERSION );
					option = UnityEditor.EditorUtility.DisplayDialogComplex("Different GoogleVR Version",
						message,
						"Download and Replace GoogleVR",
						"No and never ask",
						"No");
				}

#pragma warning restore CS0162



				switch (option) {
				case 0:
					DownloadAndImportGoogleVRAsset();
					return;

				case 1: // Do not check, and do not check again.
					System.IO.File.Create(IGNORE_GOOGLE_VR_CHECK_PATH);
					Debug.Log("<b>Bridge Engine</b>: " + IGNORE_GOOGLE_VR_CHECK_PATH + " created. Delete it to re-enable check");
					UnityEditor.EditorUtility.DisplayDialog("Skipping GoogleVR check", REENABLE_COMPATIBILITY_CHECK_MESSAGE, "Ok");
					return;

				case 2: // Do not check
					// Fall through.
				default:
					return;
				}
			} else {
				// Resume installation of GoogleVR
				DownloadAndImportGoogleVRAsset();
			}
		}
    }

	public static bool IsInstallInProgress()
	{
		// Check if InstallInProgress matches current process ID.
		if( System.IO.File.Exists(INSTALL_IS_IN_PROGRESS_PATH) ) {
			using( TextReader reader = File.OpenText(INSTALL_IS_IN_PROGRESS_PATH) ) {
				var installerPIDString = reader.ReadLine();
				int installerPID;
				if( int.TryParse(installerPIDString, out installerPID) ) {
					int currentPID = System.Diagnostics.Process.GetCurrentProcess().Id;
					if( installerPID == currentPID ) {
						return true;
					}
				}
			}
		}
		return false;
	}
	
	static void DeleteInstallInProgress()
	{
		if (System.IO.File.Exists(INSTALL_IS_IN_PROGRESS_PATH))
			System.IO.File.Delete(INSTALL_IS_IN_PROGRESS_PATH);
	}

	static void CreateInstallInProgressCheckFile()
	{
		using( TextWriter writer = File.CreateText(INSTALL_IS_IN_PROGRESS_PATH) ) {
			int processID = System.Diagnostics.Process.GetCurrentProcess().Id;
			writer.WriteLine( processID.ToString() );
		}
			// System.IO.File.Create(INSTALL_IS_IN_PROGRESS_PATH);
	}

	public static bool IsIgnoreGoogleVRCheck()
	{
		return System.IO.File.Exists(IGNORE_GOOGLE_VR_CHECK_PATH);
	}

	static void DeleteGoogleVRDummy()
	{
		string[] paths = UnityEditor.AssetDatabase.FindAssets("GoogleVRDummy");
		bool detectedDummy = paths.Length > 0;

		if (detectedDummy)
		{
			string path = UnityEditor.AssetDatabase.GUIDToAssetPath(paths[0]);

			UnityEditor.AssetDatabase.DeleteAsset(path); // disable script
			UnityEditor.AssetDatabase.DeleteAsset(path + ".meta"); // disable script
			UnityEditor.AssetDatabase.Refresh();
		}
	}

	static bool DetectGoogleVR()
	{
		if( Directory.Exists("Assets/GoogleVR") ) {
			if( GvrUnitySdkVersion.GVR_SDK_VERSION == GOOGLE_VR_VERSION ) {
				return true;
			}
		}
		return false;
	}

	static string GoogleVRUnityPackagePath()
	{
		return "Library/googleVR.unitypackage";
	}

	static void DeleteGoogleVRUnityPackagePath()
	{
		if (System.IO.File.Exists(GoogleVRUnityPackagePath()))
			System.IO.File.Delete(GoogleVRUnityPackagePath());
	}

	static void ShowProgressBar(float progress)
	{
		UnityEditor.EditorUtility.DisplayProgressBar("Downloading..", "Downloading GoogleVRForUnity.unitypackage", progress);
	}

	static void ImportGoogleVRPackage()
	{
		// Delete existing GoogleVR package
		if( Directory.Exists("Assets/GoogleVR") ) {
			Directory.Delete("Assets/GoogleVR", true);
		}

		string path = GoogleVRUnityPackagePath();
		UnityEditor.AssetDatabase.ImportPackage(path, false);
		DeleteGoogleVRUnityPackagePath();
	}

	static void DownloadAndImportGoogleVRAsset()
	{
		CreateInstallInProgressCheckFile();
		bool alreadyDownloaded = System.IO.File.Exists(GoogleVRUnityPackagePath());

		if (alreadyDownloaded)
		{
			ImportGoogleVRPackage();
		}
		else
		{
			Debug.Log("<b>Bridge Engine</b>: Cannot find GoogleVR package, trying to download it");
			downloader = new WWW(GOOGLE_VR_INSTALL_URL);
			UnityEditor.EditorApplication.update += Update;
		}
	}

	static void Update()
	{
		if (downloader != null)
		{
			if (downloader.isDone)
			{
				if (string.IsNullOrEmpty(downloader.error))
					OnDownloadFinished(downloader);
				else
					OnDownloadFinishedWithError(downloader);

				UnityEditor.EditorUtility.ClearProgressBar();
				downloader = null;
			}
			else
				ShowProgressBar(downloader.progress);
		}

		if (downloader == null)
			UnityEditor.EditorApplication.update -= Update;
	}

	static void OnDownloadFinished(WWW downloader)
	{
		var googleVRPackagePath = GoogleVRUnityPackagePath();
		System.IO.File.WriteAllBytes(googleVRPackagePath, downloader.bytes);
		
		ImportGoogleVRPackage();
	}

	static void OnDownloadFinishedWithError(WWW downloader)
	{
		Debug.LogError("<b>Bridge Engine</b>: GoogleVRForUnity.unitypackage download failed: " + downloader.error);
		
	}
}
#endif
