/*
 * This file is part of the Structure SDK.
 * Copyright © 2016 Occipital, Inc. All rights reserved.
 * http://structure.io
 *
 * Mixed Reality interaction demo:
 *  - Add an event trigger to the BEScene geometry, and listen for when we're interacting with it.
 *  - Show a placement ring when we're actively looking at the BEScene, otherwise hide it.
 *  - Using a listener for onControllerDidPressButton events for button presses
 *     Pull trigger to place objects into the scene.
 */

using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using BEDummyGoogleVR;
namespace BEDummyGoogleVR { }

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Serializable] class CarControlInput  {
        public float motorTorque = 0;
        public float steerAngle = 0;
        public float brakeTorque = 0;
    }

    [SerializeField] CarControlInput m_CarControlInput;

    [SerializeField] float m_MaxMotorForce, m_MaxSteerForce, m_MaxBrakeForce;
    [SerializeField] Transform m_WheelParent;
    public enum WheelPosition { RearLeft, RearRight, FrontLeft, FrontRight };
    /// <summary>
    /// Array of wheel colliders arranged in the following order:
    /// 0: FrontLeft, 1: FrontRight, 2: RearLeft, 3: RearRight
    /// </summary>
    /// 
    WheelCollider[] m_WheelColliders;




    WheelCollider GetWheelCollider(WheelPosition wheelPosition)
    {
        return m_WheelColliders[(int)wheelPosition];
    }

    BEScene beScene;
    BridgeEngineUnity beUnity;

    //public GameObject placementRing;
    //Transform touch;
 //   public List<GameObject> placeObjects;
    List<EventTrigger> worldEventTriggers = new List<EventTrigger>();
   // const float FallHeight = 0.3f;
    Rigidbody m_RigidBody;

    /**
     * Automatically attach to BEUnity on the default MainCamera, and add a listener for onControllerDidPressButton events.
     */
    void Awake()
    {
        m_RigidBody = GetComponent<Rigidbody>();

        m_WheelColliders = m_WheelParent.GetComponentsInChildren<WheelCollider>();
        m_CarControlInput = new CarControlInput();
        beScene = BEScene.FindBEScene();
        if (beScene == null)
        {
            Debug.LogWarning("BESampleMRController needs a @BridgeEngineScene to work properly.");
            return;
        }

        beUnity = BridgeEngineUnity.main;
        if (beUnity)
        {
            beUnity.onControllerButtonEvent.AddListener(OnButtonEvent);
            beUnity.onControllerTouchEvent.AddListener(OnTouchEvent);
        }
        else
        {
            Debug.LogWarning("Cannot connect to BridgeEngineUnity controller.");
        }


    }

    // Use this for initialization
    void Start()
    {



        // Hide placement ring by default on start.
        //placementRing.SetActive(false);

        // Create an EventTriggers on every collidable mesh in BEScene
        BEScene beScene = BEScene.FindBEScene();
        Debug.Assert(beScene != null, "BESampleMRController requires a valid @BridgeEngineScene to work properly");
        /*
        foreach (MeshCollider collider in beScene.GetComponentsInChildren<MeshCollider>())
        {
            var trigger = collider.gameObject.AddComponent<EventTrigger>();
            worldEventTriggers.Add(trigger);

            EventTrigger.Entry entryEnter = new EventTrigger.Entry();
            entryEnter.eventID = EventTriggerType.PointerEnter;
            entryEnter.callback.AddListener((data) => { OnPlaceHilight(true); });
            trigger.triggers.Add(entryEnter);

            EventTrigger.Entry entryExit = new EventTrigger.Entry();
            entryExit.eventID = EventTriggerType.PointerExit;
            entryExit.callback.AddListener((data) => { OnPlaceHilight(false); });
            trigger.triggers.Add(entryExit);
        }*/
    }

    /// Utility to get the GameObject bounds
    Bounds GameObjectMaxBounds(GameObject g)
    {
        var b = new Bounds(g.transform.position, Vector3.zero);
        foreach (Renderer r in g.GetComponentsInChildren<Renderer>())
        {
            b.Encapsulate(r.bounds);
        }
        return b;
    }

    void OnPlaceHilight(bool showPlacementRing)
    {
        //placementRing.SetActive(showPlacementRing);

        // // Scale the placement ring to whatever size is the current objectToPlace.
        // if( showPlacementRing ) {
        //  var objectToPlace = placeObjects.First();
        //  if( objectToPlace ) {
        //      float radius = GameObjectMaxBounds(objectToPlace).extents.magnitude;
        //      float thickness = placementRing.transform.localScale.y;
        //      placementRing.transform.localScale = new Vector3(radius, thickness, radius);
        //  }
        // }
    }

    void Update()
    {


        GetWheelCollider(WheelPosition.RearLeft).motorTorque = GetWheelCollider(WheelPosition.RearRight).motorTorque = m_CarControlInput.motorTorque;
        GetWheelCollider(WheelPosition.FrontLeft).steerAngle = GetWheelCollider(WheelPosition.FrontRight).steerAngle = m_CarControlInput.steerAngle;

        Debug.Log(GetWheelCollider(WheelPosition.RearLeft).motorTorque);

        if (Input.GetKey(KeyCode.Space))
        {
            GetWheelCollider(WheelPosition.RearLeft).brakeTorque = GetWheelCollider(WheelPosition.RearRight).brakeTorque = m_MaxBrakeForce;

        }
        /*
        if (Input.GetKeyUp(KeyCode.Space))
        {
            GetWheelCollider(WheelPosition.RearLeft).brakeTorque = GetWheelCollider(WheelPosition.RearRight).brakeTorque = 0;
        }

        if (Input.GetAxis("Vertical").Equals(0f))
        {
            GetWheelCollider(WheelPosition.RearLeft).brakeTorque = GetWheelCollider(WheelPosition.RearRight).brakeTorque = 0.1f;

        }
        else
        {
            GetWheelCollider(WheelPosition.RearLeft).brakeTorque = GetWheelCollider(WheelPosition.RearRight).brakeTorque = 0f;
        }*/

        Debug.LogFormat("Vertical {0}", Input.GetAxis("Vertical").Equals(0f));
       // Debug.LogFormat("Brake ", GetWheelCollider(WheelPosition.RearLeft).brakeTorque);
    }

    /**
     * Primary Button interacts, placing and moving items on the ground, or picking up and throwing the ball.
     */
    public void OnButtonEvent(BEControllerButtons current, BEControllerButtons down, BEControllerButtons up)
    {
        return;
        /*
        if (down == BEControllerButtons.ButtonPrimary && placementRing.activeSelf && placeObjects.Count() > 0)
        {
            GameObject objectToPlace = placeObjects.First();
            placeObjects.RemoveAt(0);
            Vector3 pos = placementRing.transform.position;
            pos.y += FallHeight; // Fall from above the ground.
            objectToPlace.transform.position = pos;
            objectToPlace.SetActive(true);

            // Reset the object's physics.
            var objectRigidBody = objectToPlace.GetComponent<Rigidbody>();
            if (objectRigidBody)
            {
                objectRigidBody.velocity = Vector3.zero;
                objectRigidBody.angularVelocity = Vector3.zero;
            }

            // Clean-up if last object is placed in scene.
            if (placeObjects.Count() == 0)
            {
                OnPlaceHilight(false);
                foreach (var trigger in worldEventTriggers)
                {
                    Destroy(trigger);
                }
            }
        }*/
    }

    void OnTouchEvent(Vector2 position, BEControllerTouchStatus touchStatus)
    {
        if (touchStatus == BEControllerTouchStatus.TouchFirstContact || touchStatus == BEControllerTouchStatus.TouchMove)
        {
            m_CarControlInput.motorTorque = m_MaxMotorForce * position.y;
            m_CarControlInput.steerAngle = m_MaxSteerForce * position.x;
        }
        // touchStatus = BEControllerTouchStatus.
        if (touchStatus == BEControllerTouchStatus.TouchReleaseContact){
            m_CarControlInput.motorTorque = m_CarControlInput.steerAngle = 0;

        }

    }
}
