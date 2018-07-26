#define ORIENTATIONROTATIONCONTROL

using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using System;
namespace BridgeEngine.Input
{
    public class CarControllerInput : MonoBehaviour
    {
        const float c_MinimumRotationMargin = 2f;



        public UnityStandardAssets.Vehicles.Car.CarController m_CarController;
        BridgeEngineUnity beUnity;
        [SerializeField]
        float
        /// <summary>
        /// max motor force.
        /// </summary>
        m_MaxMotorForce,

        /// <summary>
        /// The max steer force.
        /// </summary>
        m_MaxSteerAngle;

        [Serializable]
        class CarMotionData
        {
            public float motorTorque = 0;
            public float steerAngle = 0;
         //   public float brakeTorque = 0;
        }
        [SerializeField]
        CarMotionData m_CarMotionData;
        private void Awake()
        {
            m_CarController = GetComponent<UnityStandardAssets.Vehicles.Car.CarController>();
            m_CarMotionData = new CarMotionData();

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



        // Use this for initialization
        void Start()
        {

        }

        private void FixedUpdate()
        {
            // pass the input to the car!
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            float v = CrossPlatformInputManager.GetAxis("Vertical");

#if !MOBILE_INPUT
                                    float handbrake = CrossPlatformInputManager.GetAxis("Jump");
                                 //   m_CarController.Move(h, v, v, handbrake);
#else

            Debug.Log("Yeah " + h);
            m_CarController.Move(
                Mathf.Max (m_CarMotionData.steerAngle, h), 
                Mathf.Max (m_CarMotionData.motorTorque,v),
                0,//Mathf.Max (m_CarMotionData.motorTorque,v),
                0f);
            #endif
        }


        void OnMotionEvent(Vector3 position, Quaternion orientation)
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
                        ((m_CarMotionData.motorTorque > 0) ? 1 : -1 ) 
                        *
                        (
                            (orientation.eulerAngles.y  - ((orientation.eulerAngles.y   > 180) ? 360 : 0 )) 
                            - 
                            (transform.eulerAngles.y    - ((transform.eulerAngles.y     > 180) ? 360 : 0 ))
                        );



            // Vector3.SignedAngle(toRotation, fromRotation, transform.up);


            m_CarMotionData.steerAngle = diffAngle;
                /*(Mathf.Abs(diffAngle) > c_MinimumRotationMargin) ?
                        Mathf.Clamp (diffAngle, -m_MaxSteerAngle, m_MaxSteerAngle) : 0;*/
                    
            #endif
        }

        /**
        * Primary Button interacts, placing and moving items on the ground, or picking up and throwing the ball.
        */
        public void OnButtonEvent(BEControllerButtons current, BEControllerButtons down, BEControllerButtons up)

        {



            if (current == BEControllerButtons.ButtonPrimary || down == BEControllerButtons.ButtonPrimary)
            {
                if (m_CarMotionData.motorTorque < m_MaxMotorForce)
                {
                    Debug.Log("Primary held down");
                    //m_CarMotionData.motorTorque = Mathf.Clamp(m_CarMotionData.motorTorque + m_MaxMotorForce * Time.deltaTime * 0.2f, 0, m_MaxMotorForce);


                }
                else
                {
                    Debug.Log("MotorTorque at maximum");
                   // m_CarMotionData.motorTorque = m_MaxMotorForce;
                }
            }

            if (up == BEControllerButtons.ButtonPrimary)
            {
                m_CarMotionData.motorTorque = 0;
            }


           // m_CarMotionData.brakeTorque =
            //    (current == BEControllerButtons.ButtonSecondary || down == BEControllerButtons.ButtonSecondary) ? m_MaxBrakeForce : 0f;



            return;

        }

        void OnTouchEvent(Vector2 position, BEControllerTouchStatus touchStatus)
        {
            if (touchStatus == BEControllerTouchStatus.TouchFirstContact || touchStatus == BEControllerTouchStatus.TouchMove)
            {
#if PADCONTROL || ORIENTATIONROTATIONCONTROL
            m_CarMotionData.motorTorque = m_MaxMotorForce * position.y;
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
