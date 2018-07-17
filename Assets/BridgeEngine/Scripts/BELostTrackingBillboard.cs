/*
 * This file is part of the Structure SDK.
 * Copyright © 2018 Occipital, Inc. All rights reserved.
 * http://structure.io
 *
 * Loss of Tracking Feedback. Fades in a notice to look back at scene.
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using BEDummyGoogleVR;
namespace BEDummyGoogleVR {}

[RequireComponent(typeof(SpriteRenderer))]
public class BELostTrackingBillboard : MonoBehaviour
{
	/// Disables the tracking lost screen from appearing.
	public bool DisableWarning;

	/**
	 * Make the tracking state be driven off BridgeEngineUnity dependency injection,
	 * and register our TrackingStateChanged callback.
	 */
	[SerializeField] BridgeEngineUnity _beUnity;
	public BridgeEngineUnity beUnity {
		set {
			_beUnity = value;

			if (_beUnity) {
				// Jump directly to current state's alpha setting.
				this.alpha = targetAlpha;
			}
		}
		get {
			return _beUnity;
		}
	}

	/**
	 * Track to the target alpha, incrementally adjusting the canvas transparency up or down.
	 */
    float targetAlpha = 0;

	/**
	 * Our "LostTrackingBillboard" sprite renderer.
	 */
	SpriteRenderer spriteRenderer;

	/**
	 * Cache of scene's main reticlePointer
	 */
	GvrReticlePointer reticlePointer;

	/**
	 *  Shader's Alpha property for setting alpha transparency.
	 */
	int alphaPropertyId;

	/// LostTracking Material
	Material lostTrackingMaterial = null;

	/// Alpha transparency of the billboard
	float alpha {
		get{
			return lostTrackingMaterial.GetFloat(alphaPropertyId);
		}
		set {
			lostTrackingMaterial.SetFloat(alphaPropertyId, value);
		}
	}

	/**
	 * Only show/hide reticlePointer when its visibility changes
	 * within the context of this class.
	 */
	bool _reticlePointerVisible;
	bool reticlePointerVisible {
		set {
			if( _reticlePointerVisible != value && reticlePointer != null ) {
				_reticlePointerVisible = value;
				Renderer reticleRenderer = reticlePointer.GetComponent<Renderer>();
				reticleRenderer.enabled = value;
			}
		}
	}

	void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		lostTrackingMaterial = spriteRenderer.material;
		alphaPropertyId = Shader.PropertyToID("_Alpha");

		reticlePointer = GameObject.FindObjectOfType<GvrReticlePointer>();
		if( reticlePointer ) {
			Renderer reticleRenderer = reticlePointer.GetComponent<Renderer>();
			_reticlePointerVisible = reticleRenderer.enabled;
		}
	}

	void Start()
	{
		if (beUnity == null) {
			beUnity = BridgeEngineUnity.main;
		}
	}
	
	void Update ()
    {
		if( DisableWarning ) {
			targetAlpha = 0;
		} else {
			BETrackerPoseAccuracy state = beUnity.TrackerPoseAccuracy();
			if( state == BETrackerPoseAccuracy.NotAvailable
			|| state == BETrackerPoseAccuracy.Uninitialized )
			{
				targetAlpha = 1;
			} else {
				targetAlpha = 0;
			}
		}

		var spriteAlpha = Mathf.MoveTowards(this.alpha, targetAlpha, Time.deltaTime * 3.0f);
		spriteRenderer.enabled = spriteAlpha > 0;
        this.alpha = spriteAlpha;

		// Show/Hide specifically the Gaze Reticle, as the other pointer isn't stuck in your face.
		if( reticlePointer != null && reticlePointer.enabled ) {
			reticlePointerVisible = (spriteAlpha < .25);
		}
	}
}
