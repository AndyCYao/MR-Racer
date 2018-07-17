using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class CarMovements : MonoBehaviour {

    [SerializeField] float C_MaxAcceleration = 1f;
    [SerializeField] float C_RotationAcceleration = 1f;
    // Use this for initialization

    float m_Acceleration;
    float m_Velocity;

    Rigidbody m_RigidBody;

    void Awake () {
        m_RigidBody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        //m_RigidBody.AddForce (Input.)




	}
}
