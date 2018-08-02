using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tornado : MonoBehaviour {
    float  m_Speed = 0.5f;
    [SerializeField]float m_SpeedMax = 1f;
    [SerializeField]float m_SpeedMin = .2f;
    const float C_MinDistance = .1f;
    [SerializeField] float m_Penalty = 20f;

    Vector3 m_TargetPosition;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Vector3.Distance (transform.position, m_TargetPosition) < C_MinDistance) {
            SetNewTarget();
        }
        m_Speed = Random.Range(m_SpeedMin, m_SpeedMax);

        transform.position = Vector3.Lerp(transform.position, m_TargetPosition, m_Speed * Time.deltaTime);
	}

    void SetNewTarget() {
        m_TargetPosition = CustomRaycasting.RayCastToScene(transform.position + Vector3.up * 2);
    }

	private void OnTriggerStay(Collider other)
	{
        if (other.gameObject.CompareTag("Player")) {
            Debug.Log("[Tornado.cs]: Hit player");
            Game.GameManager.Instance.Game.TimeRemaining -= m_Penalty * Time.deltaTime;
        }
	}
}
