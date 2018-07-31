#define ORIENTATIONROTATIONCONTROL

using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using System;
namespace BridgeEngine.Input
{
    public class CarControllerInput_Orig : CarControllerInput
    {
        
        [SerializeField] float c_MinimumRotationMargin = 5f;
    
        [SerializeField] float c_MaxAngleTwist = 65;

        // Use this for initialization
        void Start()
        {
        }

        public override void FixedUpdate()
        {
            // pass the input to the car!

            float v = m_CarMotionData.motorTorque;
            float h = m_CarMotionData.steerAngle;
            //float handbrake = CrossPlatformInputManager.GetAxis("Jump");


            m_CarController.Move(h,v,v,0);

        }


        public override void OnMotionEvent(Vector3 position, Quaternion orientation)
        {
    
            float controllerZTilt = (Mathf.Abs (orientation.eulerAngles.z) <= c_MinimumRotationMargin) ? 0 :
                
                                   (((orientation.eulerAngles.z > 180) ? 360 : 0) - orientation.eulerAngles.z);

            float horizontalInput = Math.Sign(controllerZTilt) * 
                                    Mathf.Pow(
                                        Mathf.Clamp (
                                                
                                                (Mathf.Abs (controllerZTilt) - c_MinimumRotationMargin) / c_MaxAngleTwist, 
                                            -1, 1),
                                        m_RotatePower);

            m_CarMotionData.steerAngle = horizontalInput;
                    

        }



        public override void OnTouchEvent(Vector2 position, BEControllerTouchStatus touchStatus)
        {
            if (touchStatus == BEControllerTouchStatus.TouchFirstContact || touchStatus == BEControllerTouchStatus.TouchMove)
            {

                m_CarMotionData.motorTorque =   Mathf.Sign(position.y) *  
                                                Mathf.Pow( 
                                                          Mathf.Abs (position.y), 
                                                          m_ThrustPower);
              
            }

            if (touchStatus == BEControllerTouchStatus.TouchReleaseContact)
            {
                m_CarMotionData.motorTorque = 0;
            }
               

        }

    }
}
