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
        int direction = 0;

		public override void OnTouchEvent(Vector2 position, BEControllerTouchStatus touchStatus)
		{
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
            return;
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
            if (up == BEControllerButtons.ButtonPrimary)
            {
                primaryButtonTimeHeld = -1;
            }
		}

        private IEnumerator acclerateUntilUnpress()
        {
            // While the releaseSignal is not given, keep accelerating

            while(primaryButtonTimeHeld >= 0)
            {
                Debug.Log("Accelerating... primaryButtonTimeHeld " + primaryButtonTimeHeld);
                m_CarMotionData.motorTorque = direction * Mathf.Clamp(Mathf.Pow(primaryButtonTimeHeld, 3),0,1);
                primaryButtonTimeHeld += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
	}
}