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
           // isHit = true;

            CheckpointPassedEvent(index);
           // transform.position = CustomRaycasting.RayCastToScene(CheckpointManager.
            //KillItSelf();
        }
	}

    public void KillItSelf () {
     
            StartCoroutine (Deactivate());

    }

    void OnEnable () {
        isHit = false;
    }

    IEnumerator Deactivate()
    {
        yield return new WaitForSeconds(1.5f);
        gameObject.SetActive(false);
    }
}
