#define ORIENTATIONROTATIONCONTROL

using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using System;
namespace BridgeEngine.Input
{
    public class CarControllerInput_Orig : CarControllerInput
    {
        const float c_MinimumRotationMargin = 2f;


        // Use this for initialization
        void Start()
        {
        }

        public override void FixedUpdate()
        {
            // pass the input to the car!
           
            float h = m_CarMotionData.steerAngle / 65;
            float v = m_CarMotionData.motorTorque;
            //float handbrake = CrossPlatformInputManager.GetAxis("Jump");

            m_CarController.Move(h,v,v,0);

        }


        public override void OnMotionEvent(Vector3 position, Quaternion orientation)
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
        }

        /**
        * Primary Button interacts, placing and moving items on the ground, or picking up and throwing the ball.
        */
        public override void OnButtonEvent(BEControllerButtons current, BEControllerButtons down, BEControllerButtons up)

        {

            if (up == BEControllerButtons.ButtonPrimary)
            {
                m_CarMotionData.motorTorque = 0;
            }

        return;

        }

        public override void OnTouchEvent(Vector2 position, BEControllerTouchStatus touchStatus)
        {
            if (touchStatus == BEControllerTouchStatus.TouchFirstContact || touchStatus == BEControllerTouchStatus.TouchMove)
            {
                #if PADCONTROL || ORIENTATIONROTATIONCONTROL
                            m_CarMotionData.motorTorque = position.y;
                #endif

                #if PADCONTROL || PADROTATIONTRIGGERACCELERATION
                            m_CarMotionData.steerAngle = m_MaxSteerAngle * position.x;
                #endif
                            }

                            if (touchStatus == BEControllerTouchStatus.TouchReleaseContact)
                            {
                                m_CarMotionData.steerAngle = 0;

                #if PADCONTROL || ORIENTATIONROTATIONCONTROL
                            m_CarMotionData.motorTorque = 0;
                #endif
            }

        }

    }
}
