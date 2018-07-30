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
        int direction = 0;

		public override void OnTouchEvent(Vector2 position, BEControllerTouchStatus touchStatus)
		{
            Debug.Log("In C OnTouchEvent");
            //base.OnTouchEvent(position, touchStatus);
            if (touchStatus == BEControllerTouchStatus.TouchFirstContact ||
                touchStatus == BEControllerTouchStatus.TouchMove)
            {
                //m_CarMotionData.motorTorque = Mathf.Pow(position.y, 3);
                m_CarMotionData.steerAngle = position.x;

                direction = (int)Mathf.Sign(position.y);
            }

            if (touchStatus == BEControllerTouchStatus.TouchReleaseContact)
            {
                m_CarMotionData.steerAngle = 0;
            }
		}

		public override void OnMotionEvent(Vector3 position, Quaternion orientation)
		{
            //base.OnMotionEvent(position, orientation);
            Debug.Log("In C OnMotionEvent");
            return;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
		}


		public override void OnButtonEvent(BEControllerButtons current, BEControllerButtons down, BEControllerButtons up)
		{
            Debug.Log("In C OnButtonEvent");
            // Implementing the primary button acceleration
            if (down == BEControllerButtons.ButtonPrimary)
            {
                primaryButtonTimeHeld = 0;
                StartCoroutine(acclerateUntilUnpress());
            }
            else if(down == BEControllerButtons.ButtonSecondary)
            {
                secondaryButtonTimeHeld = 0;
                StartCoroutine(brakeUntilUnpress());
            }

            if (up == BEControllerButtons.ButtonPrimary)
            {
                primaryButtonTimeHeld = -1;
            }
            else if(up == BEControllerButtons.ButtonSecondary)
            {
                secondaryButtonTimeHeld = -1;
            }
		}

        private IEnumerator acclerateUntilUnpress()
        {
            // While the releaseSignal is not given, keep accelerating
            float newSpeed = 0;
            while(primaryButtonTimeHeld >= 0)
            {
                Debug.Log("Accelerating... primaryButtonTimeHeld " + primaryButtonTimeHeld);
                // m_CarMotionData.motorTorque =  Mathf.Clamp(Mathf.Pow(primaryButtonTimeHeld, 3),0,1);
                newSpeed = m_CarMotionData.motorTorque + (float)0.5 * Time.deltaTime;
                m_CarMotionData.motorTorque = Mathf.Min(1, newSpeed);
                primaryButtonTimeHeld += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator brakeUntilUnpress()
        {
            // While the releaseSignal is not given, keep braking.
            while (secondaryButtonTimeHeld >= 0)
            {
                Debug.Log("Braking... secondaryButtonTimeHeld " + secondaryButtonTimeHeld);
                m_CarMotionData.motorTorque = Mathf.Clamp(1 / secondaryButtonTimeHeld, 0, 1);
                secondaryButtonTimeHeld += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
	}
}