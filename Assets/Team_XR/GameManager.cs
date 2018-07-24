using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public float currCountdownValue = 0f;
    private float startTime;
    public Text countDown;

	private void Awake()
	{
        CheckpointManager.NewCheckpointCreatedEvent += AddMoreTime;
	}

	// Use this for initialization
	void Start () {
        StartCoroutine(CountDown());
        Time.timeScale = 1;
        startTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () 
    {

	}

    // This method can be improve to have dynamic addtional time
    // base on the distance between the car and the new checkpoint
    private void AddMoreTime(Vector3 newCheckPointLocation)
    {
        currCountdownValue += 10f;
    }

    private IEnumerator CountDown()
    {
        while (currCountdownValue >= 0)
        {
            countDown.text = ("Time Left: " + currCountdownValue);
            yield return new WaitForSeconds(1.0f);
            currCountdownValue--;
        }
        countDown.text = ("GAME OVER - You lasted for " + (Time.time - startTime).ToString("0.00"));
    }
}
