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
        Vector2 rootVector = new Vector2(0, 0);
        float direction;

        //UnityStandardAssets.Vehicles.Car.CarController m_CarController;
        //BridgeEngineUnity beUnity;

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }


        public override void OnMotionEvent(Vector3 position, Quaternion orientation)
        {}

        /**
        * Primary Button interacts, placing and moving items on the ground, or picking up and throwing the ball.
        */
        public override void OnButtonEvent(BEControllerButtons current, BEControllerButtons down, BEControllerButtons up)

        {

            //Debug.Log("In A OnButtonEvent");
            if (up == BEControllerButtons.ButtonPrimary)
            {
                m_CarMotionData.motorTorque = 0;
            }

            return;

        }

        public override void OnTouchEvent(Vector2 position, BEControllerTouchStatus touchStatus)
        {
            // Debug.Log("In A OnTouchEvent");
            if (touchStatus == BEControllerTouchStatus.TouchFirstContact || touchStatus == BEControllerTouchStatus.TouchMove)
            {
                direction                   = Mathf.Sign(position.y);
                m_CarMotionData.motorTorque = direction * Vector2.Distance(position,rootVector);
                //Debug.Log("In A OnTouchEvent position x is " + position.x);
                m_CarMotionData.steerAngle  = Mathf.Clamp(position.x / 0.8f ,-1,1);
            }

            if (touchStatus == BEControllerTouchStatus.TouchReleaseContact)
            {
                m_CarMotionData.steerAngle = 0;
                m_CarMotionData.motorTorque = 0;
            }

        }

    }
}
