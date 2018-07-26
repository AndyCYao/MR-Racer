using UnityEngine;
using System.Collections;

/*
 * Trigger                    - Unmapped
 * D-pad                      - Up - forward accel , Back - Reverse . Left - Rotate Left Accel , 
 *                              Right - Rotate right accel
 * Secondary Button + Trigger - hold for 1+ second to reset
 */


namespace BridgeEngine.Input{
    public class CarControllerInput_A : CarControllerInput
    {
		public override void OnTouchEvent(Vector2 position, BEControllerTouchStatus touchStatus)
		{
			//base.OnTouchEvent(position, touchStatus);
            if (touchStatus == BEControllerTouchStatus.TouchFirstContact || 
                touchStatus == BEControllerTouchStatus.TouchMove)
            {
                m_CarMotionData.motorTorque = Mathf.Pow(position.y,3);
                m_CarMotionData.steerAngle  = position.x;


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

        // Trigger is unmapped 
		public override void OnButtonEvent(BEControllerButtons current, BEControllerButtons down, BEControllerButtons up)
		{
            return;
		}
	}
}