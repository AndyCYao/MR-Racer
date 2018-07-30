using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using System;
namespace BridgeEngine.Input
{
    public class CarControllerInput : MonoBehaviour
    {
        const float c_MinimumRotationMargin = 2f;

        protected UnityStandardAssets.Vehicles.Car.CarController m_CarController;
        BridgeEngineUnity beUnity;

        [Serializable]
        protected class CarMotionData
        {
            public float motorTorque = 0;
            public float steerAngle = 0;
         //   public float brakeTorque = 0;
        }
        [SerializeField]
        protected CarMotionData m_CarMotionData;
        [SerializeField]
        GameObject m_ExplosionBoom;
        [SerializeField]
        GameObject m_ExplosionCrash;

        protected void Awake()
        {
            m_CarController = GetComponent<UnityStandardAssets.Vehicles.Car.CarController>();
            m_CarMotionData = new CarMotionData();
            //m_Explosion = transform.Find("CartoonBoom_V2").gameObject;

            beUnity = BridgeEngineUnity.main;
            if (beUnity)
            {
                beUnity.onControllerMotionEvent.AddListener(OnMotionEvent);
                beUnity.onControllerButtonEvent.AddListener(OnButtonEvent);
                beUnity.onControllerTouchEvent.AddListener(OnTouchEvent);
            }
            else
            {
                Debug.LogWarning("Cannot connect to BridgeEngineUnity controller.");
            }
        }


        public virtual void FixedUpdate()
        {
            // pass the input to the car!
            float h = m_CarMotionData.steerAngle;
            float v = m_CarMotionData.motorTorque;
            //float handbrake = CrossPlatformInputManager.GetAxis("Jump");
            m_CarController.Move(h,v,v,0);
        }

        public virtual void OnMotionEvent(Vector3 position, Quaternion orientation)
        {
            Debug.Log("In Parent OnMotionEvent");
        }

        /**
        * Primary Button interacts, placing and moving items on the ground, or picking up and throwing the ball.
        */
        public virtual void OnButtonEvent(BEControllerButtons current, BEControllerButtons down, BEControllerButtons up)

        {
            Debug.Log("In Parent OnButtonEvent");
        }

        public virtual void OnTouchEvent(Vector2 position, BEControllerTouchStatus touchStatus)
        {
            Debug.Log("In Parent OnTouchEvent");

        }

		public void OnCollisionEnter(Collision collision)
		{
            Debug.Log("Collided with " + collision.gameObject.name);
            TriggerExplosion(collision.contacts[0].point);
		}

        public void TriggerExplosion(Vector3 position){
            //m_Explosion.SetActive(true);
            Destroy(Instantiate (m_ExplosionBoom, position, Quaternion.identity) , 1.5f);
        }
    }
}
