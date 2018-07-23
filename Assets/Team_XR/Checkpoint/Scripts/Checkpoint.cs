using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {
    public int index;
    public bool isHit = false;

    public delegate void CheckpointPassed (int index);
    public static event CheckpointPassed CheckpointPassedEvent;

	private void OnTriggerEnter(Collider other)
	{
        if (other.CompareTag("Player") && !isHit )
        {
            isHit = true;

            CheckpointPassedEvent(index);

            SetActive(false);
        }
	}

    public void SetActive (bool isActive) {

        if (isActive ){
            StartCoroutine (Activate());
        }
        else {
            StartCoroutine (Deactivate());
        }
    }

    IEnumerator Activate () {
        yield return new WaitForSeconds(0.5f);
        SetActive(true);
    }

    IEnumerator Deactivate()
    {
        yield return new WaitForSeconds(1.5f);
        SetActive(false);
    }
}
