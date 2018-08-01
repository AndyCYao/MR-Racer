using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using System;
namespace BridgeEngine.Input
{
    public class CarControllerInput : MonoBehaviour
    {
        const float c_MinimumRotationMargin = 2f;
        const float C_DeadZone = 0.25f;

        [SerializeField] protected float m_ThrustPower = 4;
        [SerializeField] protected float m_RotatePower = 3;

        protected UnityStandardAssets.Vehicles.Car.CarController m_CarController;
        protected BridgeEngineUnity beUnity;

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
        public GameObject m_EffectExplosionBoom;
        [SerializeField]
        public GameObject[] m_EffectExplosionCrash;
        [SerializeField]
        public GameObject m_EffectGroundImpact;

        bool isReadyEffect = true;
        float effectCoolDown = 1.5f;


        public GameObject m_CarVisibility;

        protected void Awake()
        {
            m_CarController = GetComponent<UnityStandardAssets.Vehicles.Car.CarController>();
            m_CarMotionData = new CarMotionData();
            //m_Explosion = transform.Find("CartoonBoom_V2").gameObject;

            m_CarVisibility = transform.Find("Car_Body").gameObject;

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
            //Debug.Log("In Parent OnMotionEvent");
        }

        /**
        * Primary Button interacts, placing and moving items on the ground, or picking up and throwing the ball.
        */
        public virtual void OnButtonEvent(BEControllerButtons current, BEControllerButtons down, BEControllerButtons up)

        {
            //Debug.Log("In Parent OnButtonEvent");
        }

        public virtual void OnTouchEvent(Vector2 position, BEControllerTouchStatus touchStatus)
        {
            //Debug.Log("In Parent OnTouchEvent");

        }

        /// <summary>
        /// If the Car hits the wall, then a random wall effect will trigger
        /// if the car hits the ground, the ground impact effect will trigger
        /// </summary>
        /// <param name="collision">Collision.</param>
		public void OnCollisionEnter(Collision collision)
		{
            Debug.Log("Collided with " + collision.gameObject.name);
      
            if(isReadyEffect && (Vector3.Angle(collision.contacts[0].normal, Vector3.up) < 10f))
            {

                StartCoroutine(TriggerGroundImpactEffect(collision.contacts[0].point, collision ));  
            }
           
            else if (isReadyEffect &&
                     (Vector3.Angle(Vector3.up, Vector3.forward) > 80f) && 
                     (Vector3.Angle(Vector3.up, Vector3.forward) < 100f))
            {
                StartCoroutine(TriggerWallImpactEffect(collision.contacts[0].point, collision));   
            }
		}

        public IEnumerator TriggerGroundImpactEffect(Vector3 position, Collision col)
        {
            isReadyEffect = false;
            float lifeTime = effectCoolDown;
            GameObject newImpactVFX = Instantiate(m_EffectGroundImpact, position, Quaternion.identity);
            float initialScale = col.relativeVelocity.magnitude;
            newImpactVFX.transform.localScale *= Mathf.Clamp (initialScale, 0.1f, .5f);
            while (lifeTime > 0)
            {
                newImpactVFX.transform.localScale = Vector3.one * initialScale * Mathf.Lerp (0, initialScale, lifeTime);
                
                lifeTime -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            Destroy(newImpactVFX);
            isReadyEffect = true;
        }

        public IEnumerator TriggerWallImpactEffect(Vector3 position, Collision col)
        {
            isReadyEffect = false;
            float initialScale = Mathf.Clamp (col.relativeVelocity.magnitude, 01f, .5f);
            float lifeTime = effectCoolDown;
            System.Random rnd = new System.Random();
            int explosionIdx = rnd.Next(0, m_EffectExplosionCrash.Length);
           
            GameObject newImpactVFX = Instantiate(m_EffectExplosionCrash[explosionIdx], position, Quaternion.identity);
           // newImpactVFX.transform.localScale *= 0.25f;
            newImpactVFX.transform.localScale *= initialScale;
            newImpactVFX.transform.LookAt(transform.position);
            while (lifeTime > 0)
            {
                newImpactVFX.transform.localScale = Vector3.one * initialScale * Mathf.Lerp (0, initialScale, lifeTime);
                lifeTime -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            Destroy(newImpactVFX);
       
            isReadyEffect = true;
        }

        public void TriggerGameOverBoomEffect(Vector3 position)
        {

            Animator animator = transform.Find("Driver").GetComponent<Animator>();
            animator.SetTrigger("DriverEjectedTrigger");
            GameObject newImpactVFX = Instantiate(m_EffectExplosionBoom, position, Quaternion.identity);
            newImpactVFX.transform.localScale *= 0.5f;
 
            Destroy (newImpactVFX, 1.5f);

            SetAnimationBool("isDriverEjected", true);
            ///transform.Find("Car_Body").gameObject.SetActive(false);
            //transform.Find("Car_Body").GetComponent<Renderer>().enabled = false;
            //foreach (Renderer wheel_renderer in transform.Find("Car_Body").Find("wheels").GetComponentsInChildren<Renderer>())
            //{
            //    wheel_renderer.enabled = false;
            //}
            m_CarVisibility.SetActive(false);


        }

        public void SetAnimationBool(string animationName, bool newBool)
        {
            Animator animator = transform.Find("Driver").GetComponent<Animator>();
            animator.SetBool(animationName, newBool);
        }

    }
}
