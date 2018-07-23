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
            KillItSelf(false);
        }
	}

    public void KillItSelf (bool isActive) {

        if (isActive ){
            StartCoroutine (Activate());
        }
        else {
            StartCoroutine (Deactivate());
        }
    }

    IEnumerator Activate () {
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(true);
    }

    IEnumerator Deactivate()
    {
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }
}
