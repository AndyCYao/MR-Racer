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

//#define PADCONTROL
//#define PADROTATIONTRIGGERACCELERATION
#define ORIENTATIONROTATIONCONTROL

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

    #region CONSTANTS
    const float c_MinimumRotationMargin = 2f;

    const float c_ScrewPhysicThreshold = .8f;

    #endregion


    /// <summary>
    /// Store motor information of the car
    /// </summary>
    [Serializable]
    class CarControlInput
    {
        public float motorTorque = 0;
        public float steerAngle = 0;
        public float brakeTorque = 0;
    }


    [SerializeField] CarControlInput m_CarControlInput;

    [SerializeField]
    float
        /// <summary>
        /// max motor force.
        /// </summary>
        m_MaxMotorForce,

    /// <summary>
    /// The max steer force.
    /// </summary>
        m_MaxSteerAngle,

        /// <summary>
        /// The max brake force.
        /// </summary>
        m_MaxBrakeForce;

    [SerializeField] Transform m_CentreOfMass;
    [SerializeField] Transform m_WheelParent;

    /// <summary>
    /// Wheel position enum 
    /// 0: RearLeft, 1: RearRight, 2: FrontLeft, 3: FrontRight
    /// </summary>
    public enum WheelPosition : int { RearLeft, RearRight, FrontLeft, FrontRight };

    /// <summary>
    /// Array of wheel colliders arranged in the following order:
    /// 0: FrontLeft, 1: FrontRight, 2: RearLeft, 3: RearRight
    /// </summary> 
    WheelCollider[] m_WheelColliders;



    /// <summary>
    /// Get the correct wheel colllider given the Wheel position parameter
    /// </summary>
    /// <returns>The wheel collider.</returns>
    /// <param name="wheelPosition">Wheel position.</param>
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
            beUnity.onControllerMotionEvent.AddListener(OnMotionEvent);
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
        // BEScene beScene = BEScene.FindBEScene();
        //Debug.Assert(beScene != null, "BESampleMRController requires a valid @BridgeEngineScene to work properly");

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
        }
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


        GetWheelCollider(WheelPosition.RearLeft).motorTorque = 
            GetWheelCollider(WheelPosition.RearRight).motorTorque = 
            GetWheelCollider(WheelPosition.FrontLeft).motorTorque = 
            GetWheelCollider(WheelPosition.FrontRight).motorTorque = m_CarControlInput.motorTorque;
        
        GetWheelCollider(WheelPosition.FrontLeft).steerAngle = GetWheelCollider(WheelPosition.FrontRight).steerAngle = m_CarControlInput.steerAngle;

#if UNITY_EDITOR
        if (!Mathf.Approximately(Input.GetAxis("Vertical"), 0f))
        {
            GetWheelCollider(WheelPosition.RearLeft).motorTorque = GetWheelCollider(WheelPosition.RearRight).motorTorque = Input.GetAxis("Vertical") * m_MaxMotorForce;
        }
        if (!Mathf.Approximately(Input.GetAxis("Horizontal"), 0f))
        {
            GetWheelCollider(WheelPosition.FrontLeft).steerAngle = GetWheelCollider(WheelPosition.FrontRight).steerAngle = Input.GetAxis("Horizontal") * m_MaxSteerAngle;
        }
#endif

        //Debug.Log(GetWheelCollider(WheelPosition.RearLeft).motorTorque);

        if (Input.GetKey(KeyCode.Space))
        {
            GetWheelCollider(WheelPosition.RearLeft).brakeTorque = GetWheelCollider(WheelPosition.RearRight).brakeTorque = m_MaxBrakeForce;

        }


        //Debug.LogFormat("Vertical {0}", Input.GetAxis("Vertical").Equals(0f));
        // Debug.LogFormat("Brake ", GetWheelCollider(WheelPosition.RearLeft).brakeTorque);
    }

    /**
     * Primary Button interacts, placing and moving items on the ground, or picking up and throwing the ball.
     */
    public void OnButtonEvent(BEControllerButtons current, BEControllerButtons down, BEControllerButtons up)

    {
  


        if (current == BEControllerButtons.ButtonPrimary || down == BEControllerButtons.ButtonPrimary)
        {
            if (m_CarControlInput.motorTorque < m_MaxMotorForce)
            {
                Debug.Log("Primary held down");
                m_CarControlInput.motorTorque = Mathf.Clamp(m_CarControlInput.motorTorque + m_MaxMotorForce * Time.deltaTime * 0.2f, 0, m_MaxMotorForce);


            }
            else
            {
                Debug.Log("MotorTorque at maximum");
                m_CarControlInput.motorTorque = m_MaxMotorForce;
            }
        }

        if (up == BEControllerButtons.ButtonPrimary)
        {
            m_CarControlInput.motorTorque = 0;
        }


        m_CarControlInput.brakeTorque =
            (current == BEControllerButtons.ButtonSecondary || down == BEControllerButtons.ButtonSecondary) ? m_MaxBrakeForce : 0f;



        return;

    }

    void OnTouchEvent(Vector2 position, BEControllerTouchStatus touchStatus)
    {
        if (touchStatus == BEControllerTouchStatus.TouchFirstContact || touchStatus == BEControllerTouchStatus.TouchMove)
        {
            #if PADCONTROL || ORIENTATIONROTATIONCONTROL
            m_CarControlInput.motorTorque = m_MaxMotorForce * position.y;
            #endif

            #if PADCONTROL || PADROTATIONTRIGGERACCELERATION
            m_CarControlInput.steerAngle = m_MaxSteerAngle * position.x;
            #endif
        }

        if (touchStatus == BEControllerTouchStatus.TouchReleaseContact)
        {
            m_CarControlInput.steerAngle = 0;

            #if PADCONTROL || ORIENTATIONROTATIONCONTROL
            m_CarControlInput.motorTorque = 0;
            #endif

        }

    }


    void OnMotionEvent(Vector3 position, Quaternion orientation)
    {
#if ORIENTATIONROTATIONCONTROL
  
        //float toAngle, fromAngle;
        //Vector3 dumbVector = Vector3.zero;
        //orientation.ToAngleAxis(out toAngle, out dumbVector);
        //transform.rotation.ToAngleAxis(out fromAngle, out dumbVector);

        Vector3 toRotation =  Vector3.ProjectOnPlane(orientation.eulerAngles, transform.up);
        Vector3 fromRotation = Vector3.ProjectOnPlane(transform.eulerAngles, transform.up);
        //Vector3 x = transform.rotation.eulerAngles;
        float diffAngle = 
            ((m_CarControlInput.motorTorque > 0) ? 1 : -1 ) 
            *
            (
                (orientation.eulerAngles.y  - ((orientation.eulerAngles.y   > 180) ? 360 : 0 )) 
                - 
                (transform.eulerAngles.y    - ((transform.eulerAngles.y     > 180) ? 360 : 0 ))
            );



        // Vector3.SignedAngle(toRotation, fromRotation, transform.up);
        

        m_CarControlInput.steerAngle =  (Mathf.Abs(diffAngle) > c_MinimumRotationMargin) ?
            Mathf.Clamp (diffAngle, -m_MaxSteerAngle, m_MaxSteerAngle) : 0;
        
#endif
    }
}
