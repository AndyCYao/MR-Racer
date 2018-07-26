using UnityEngine;
using System.Collections;

/*
 * Trigger Button as Acceleration 
 */

namespace BridgeEngine.Input{
    public class CarControllerInput_B : CarControllerInput
    {
        float primaryButtonTimeHeld = -1;

		public override void OnTouchEvent(Vector2 position, BEControllerTouchStatus touchStatus)
		{
            //base.OnTouchEvent(position, touchStatus);
            return;
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
            if (current == BEControllerButtons.ButtonPrimary)
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
                Debug.Log("Accelerating... ");
                m_CarMotionData.motorTorque = Mathf.Clamp(Mathf.Pow(primaryButtonTimeHeld, 3),-1,1);
                yield return new WaitForEndOfFrame();
            }
        }
	}
}