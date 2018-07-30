#define ORIENTATIONROTATIONCONTROL

using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using System;
namespace BridgeEngine.Input
{
    public class CarControllerInput_A : CarControllerInput
    {
        const float c_MinimumRotationMargin = 2f;



        //UnityStandardAssets.Vehicles.Car.CarController m_CarController;
        BridgeEngineUnity beUnity;

        public override void FixedUpdate()
        {
            // pass the input to the car!
            float h = m_CarMotionData.steerAngle / 360;
            float v = m_CarMotionData.motorTorque;
            //float handbrake = CrossPlatformInputManager.GetAxis("Jump");

            #if !MOBILE_INPUT
                float handbrake = CrossPlatformInputManager.GetAxis("Jump");
                 //   m_CarController.Move(h, v, v, handbrake);
            #else


            Debug.Log("h " + h + " v " + v );
            m_CarController.Move(h,v,v,0);
            #endif
        }


        public override void OnMotionEvent(Vector3 position, Quaternion orientation)
        {
            //Debug.Log("In A OnMotionEvent");
            //#if ORIENTATIONROTATIONCONTROL
              
            //        //float toAngle, fromAngle;
            //        //Vector3 dumbVector = Vector3.zero;
            //        //orientation.ToAngleAxis(out toAngle, out dumbVector);
            //        //transform.rotation.ToAngleAxis(out fromAngle, out dumbVector);

            //        Vector3 toRotation =  Vector3.ProjectOnPlane(orientation.eulerAngles, transform.up);
            //        Vector3 fromRotation = Vector3.ProjectOnPlane(transform.eulerAngles, transform.up);
            //        //Vector3 x = transform.rotation.eulerAngles;
            //        float diffAngle = 
            //            ((m_CarMotionData.motorTorque > 0) ? 1 : -1 ) 
            //            *
            //            (
            //                (orientation.eulerAngles.y  - ((orientation.eulerAngles.y   > 180) ? 360 : 0 )) 
            //                - 
            //                (transform.eulerAngles.y    - ((transform.eulerAngles.y     > 180) ? 360 : 0 ))
            //            );



            //// Vector3.SignedAngle(toRotation, fromRotation, transform.up);


            //m_CarMotionData.steerAngle = diffAngle *   
            //    ((Mathf.Abs(diffAngle) > c_MinimumRotationMargin) ? 1 : 0);
                    
            //#endif
        }

        /**
        * Primary Button interacts, placing and moving items on the ground, or picking up and throwing the ball.
        */
        public override void OnButtonEvent(BEControllerButtons current, BEControllerButtons down, BEControllerButtons up)

        {



            //if (current == BEControllerButtons.ButtonPrimary || down == BEControllerButtons.ButtonPrimary)
            //{
            //    if (m_CarMotionData.motorTorque < m_MaxMotorForce)
            //    {
            //        Debug.Log("Primary held down");
            //        //m_CarMotionData.motorTorque = Mathf.Clamp(m_CarMotionData.motorTorque + m_MaxMotorForce * Time.deltaTime * 0.2f, 0, m_MaxMotorForce);


            //    }
            //    else
            //    {
            //        Debug.Log("MotorTorque at maximum");
            //       // m_CarMotionData.motorTorque = m_MaxMotorForce;
            //    }
            //}
            Debug.Log("In A OnButtonEvent");
            if (up == BEControllerButtons.ButtonPrimary)
            {
                m_CarMotionData.motorTorque = 0;
            }


           // m_CarMotionData.brakeTorque =
            //    (current == BEControllerButtons.ButtonSecondary || down == BEControllerButtons.ButtonSecondary) ? m_MaxBrakeForce : 0f;



            return;

        }

        public override void OnTouchEvent(Vector2 position, BEControllerTouchStatus touchStatus)
        {
            Debug.Log("In A OnTouchEvent");
            if (touchStatus == BEControllerTouchStatus.TouchFirstContact || touchStatus == BEControllerTouchStatus.TouchMove)
            {
                m_CarMotionData.motorTorque = position.y;
                m_CarMotionData.steerAngle  = position.x;
            }

            if (touchStatus == BEControllerTouchStatus.TouchReleaseContact)
            {
                m_CarMotionData.steerAngle = 0;
                m_CarMotionData.motorTorque = 0;
            }

        }

    }
}
