using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {
    public int index;
    bool isHit = false;

    public delegate void CheckpointPassed (int index);
    public static event CheckpointPassed CheckpointPassedEvent;

	private void OnTriggerEnter(Collider other)
	{
        if (other.CompareTag("Player"))
        {
            isHit = true;

            CheckpointPassedEvent(index);
        }
	}
}
