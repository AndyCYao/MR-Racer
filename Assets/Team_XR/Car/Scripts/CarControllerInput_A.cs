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
        float resetConfirmTime = -1;
        //UnityStandardAssets.Vehicles.Car.CarController m_CarController;
       

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
            if (up == BEControllerButtons.ButtonPrimary)
            {
                m_CarMotionData.motorTorque = 0;
            }

            if (current == (BEControllerButtons.ButtonPrimary | BEControllerButtons.ButtonSecondary))
            {

                if (down == BEControllerButtons.ButtonSecondary
                && Mathf.Approximately(resetConfirmTime, -1))
                {
                    Debug.Log("CarControllerInput_A - OnControllerButton: Reset button sequence being pressed!");
                    resetConfirmTime = 0;
                    StartCoroutine(CheckButtonHold());
                }


            }

            // New design, pressing the secondary button will reset the car
            if (up == BEControllerButtons.ButtonSecondary)
            {

                if (resetConfirmTime > 1)
                {
                    Debug.Log(string.Format("CarControllerInput_A - OnControllerButton: Reset Vehicle!"));
                    SpawnPlayerRandomly();
                }
                resetConfirmTime = -1;
            }

            return;

        }

        public override void OnTouchEvent(Vector2 position, BEControllerTouchStatus touchStatus)
        {
            // Debug.Log("In A OnTouchEvent");
            if (touchStatus == BEControllerTouchStatus.TouchFirstContact || touchStatus == BEControllerTouchStatus.TouchMove)
            {
                direction                   = Mathf.Sign(position.y);
                m_CarMotionData.motorTorque = direction * Mathf.Pow (Vector2.Distance(position,rootVector), m_ThrustPower);
                Debug.Log("In A OnTouchEvent position x is " + position.x);
                m_CarMotionData.steerAngle  = Mathf.Pow (Mathf.Clamp(position.x / 0.8f ,-1,1), m_RotatePower);
            }

            if (touchStatus == BEControllerTouchStatus.TouchReleaseContact)
            {
                m_CarMotionData.steerAngle = 0;
                m_CarMotionData.motorTorque = 0;
            }

        }

        IEnumerator CheckButtonHold()
        {

            while (resetConfirmTime >= 0 && resetConfirmTime < 1)
            {
                resetConfirmTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            if (!Mathf.Approximately(resetConfirmTime, -1))
            {
                resetConfirmTime = -1;
                SpawnPlayerRandomly();
            }

        }
    }
}
