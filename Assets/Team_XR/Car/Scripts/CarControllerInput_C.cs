using UnityEngine;
using System.Collections;

/*
 * Trigger Button as Acceleration
 * Secondary Button as Break
 * D-pad as direction
 */

namespace BridgeEngine.Input{
    public class CarControllerInput_C : CarControllerInput
    {
        float primaryButtonTimeHeld = -1;
        float secondaryButtonTimeHeld = -1;
        float direction = 0;

        // m_CarMotionData.steerAngle is updated here
		public override void OnTouchEvent(Vector2 position, BEControllerTouchStatus touchStatus)
		{
            Debug.Log("In C OnTouchEvent");
            //base.OnTouchEvent(position, touchStatus);
            if (touchStatus == BEControllerTouchStatus.TouchFirstContact ||
                touchStatus == BEControllerTouchStatus.TouchMove)
            {
                //m_CarMotionData.motorTorque = Mathf.Pow(position.y, 3);
                m_CarMotionData.steerAngle = position.x;
                direction = Mathf.Sign(position.y);
            }

            if (touchStatus == BEControllerTouchStatus.TouchReleaseContact)
            {
                m_CarMotionData.steerAngle = 0;
            }
		}

		public override void OnMotionEvent(Vector3 position, Quaternion orientation)
		{
            base.OnMotionEvent(position, orientation);
		}

		public override void FixedUpdate()
		{
            base.FixedUpdate();
		}


		public override void OnButtonEvent(BEControllerButtons current, BEControllerButtons down, BEControllerButtons up)
		{
            // Implementing the primary button acceleration
            if (down == BEControllerButtons.ButtonPrimary)
            {
                primaryButtonTimeHeld = 0;
                StartCoroutine(acclerateUntilUnpress());
            }
            else if(down == BEControllerButtons.ButtonSecondary)
            {
                secondaryButtonTimeHeld = 0;
                StartCoroutine(reverseUntilUnpress());
            }

            if (up == BEControllerButtons.ButtonPrimary)
            {
                primaryButtonTimeHeld = -1;
                Debug.Log("In C OnButtonEvent primary button lifted, setting torque to 0");
                m_CarMotionData.motorTorque = 0;
            }
            else if(up == BEControllerButtons.ButtonSecondary)
            {
                Debug.Log("In C OnButtonEvent secondary button lifted setting torque to 0");
                secondaryButtonTimeHeld = -1;
                m_CarMotionData.motorTorque = 0;
            }
		}

        // m_CarMotionData.MotorTorque is updated here
        private IEnumerator acclerateUntilUnpress()
        {
            // While the releaseSignal is not given, keep accelerating
            float newSpeed = 0;
            while(primaryButtonTimeHeld >= 0)
            {
                //Debug.Log("Accelerating... primaryButtonTimeHeld " + primaryButtonTimeHeld);
                // m_CarMotionData.motorTorque =  Mathf.Clamp(Mathf.Pow(primaryButtonTimeHeld, 3),0,1);
                newSpeed = m_CarMotionData.motorTorque + (float)0.5 * Time.deltaTime;
                m_CarMotionData.motorTorque = Mathf.Min(1, newSpeed);
                primaryButtonTimeHeld += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        // m_CarMotionData.MotorTorque is updated here
        private IEnumerator reverseUntilUnpress()
        {
            float newSpeed = 0;
            // While the releaseSignal is not given, keep reversing.
            while (secondaryButtonTimeHeld >= 0)
            {
                //Debug.Log("reversing... secondaryButtonTimeHeld " + secondaryButtonTimeHeld);
                newSpeed = m_CarMotionData.motorTorque - (float)0.5 * Time.deltaTime;
                m_CarMotionData.motorTorque = newSpeed;
                secondaryButtonTimeHeld += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
	}
}