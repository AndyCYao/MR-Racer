using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using System;
namespace BridgeEngine.Input
{
    public class CarControllerInput : MonoBehaviour
    {
        const float c_MinimumRotationMargin = 2f;

        protected UnityStandardAssets.Vehicles.Car.CarController m_CarController;
        BridgeEngineUnity beUnity;

        [Serializable]
        protected class CarMotionData
        {
            public float motorTorque = 0;
            public float steerAngle = 0;
         //   public float brakeTorque = 0;
        }
        [SerializeField]
        protected CarMotionData m_CarMotionData;
        protected void Awake()
        {
            m_CarController = GetComponent<UnityStandardAssets.Vehicles.Car.CarController>();
            m_CarMotionData = new CarMotionData();

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


        public virtual void FixedUpdate()
        {
            // pass the input to the car!
            float h = m_CarMotionData.steerAngle;
            float v = m_CarMotionData.motorTorque;
            //float handbrake = CrossPlatformInputManager.GetAxis("Jump");
<<<<<<< HEAD

#if !MOBILE_INPUT
                                    float handbrake = CrossPlatformInputManager.GetAxis("Jump");
                                 //   m_CarController.Move(h, v, v, handbrake);
#else


//            Debug.Log("h " + h + " v " + v );
            m_CarController.Move(h,v,v,0);
            #endif
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
                
                -(orientation.eulerAngles.z - ((orientation.eulerAngles.z > 180) ? 360 : 0));
 


            // Vector3.SignedAngle(toRotation, fromRotation, transform.up);


            m_CarMotionData.steerAngle = diffAngle *   
                ((Mathf.Abs(diffAngle) > c_MinimumRotationMargin) ? 1 : 0);
                    
            #endif
=======
            Debug.Log("h " + h + " v " + v);
            m_CarController.Move(h, v, v, 0);
    
        }


        public virtual void OnMotionEvent(Vector3 position, Quaternion orientation)
        {
            Debug.Log("In Parent OnMotionEvent");
>>>>>>> master
        }

        /**
        * Primary Button interacts, placing and moving items on the ground, or picking up and throwing the ball.
        */
        public virtual void OnButtonEvent(BEControllerButtons current, BEControllerButtons down, BEControllerButtons up)

        {
            Debug.Log("In Parent OnButtonEvent");
            return;
        }

        public virtual void OnTouchEvent(Vector2 position, BEControllerTouchStatus touchStatus)
        {
            Debug.Log("In Parent OnTouchEvent");

        }
    }
}
