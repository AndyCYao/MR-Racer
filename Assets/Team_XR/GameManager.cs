using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    public float currCountdownValue = 0f;
    private float startTime;
    public Text countDown;

    public GameObject player;

    private static GameManager s_GameManager;

    public static GameManager Instance {
        get
        {
            return s_GameManager;
        }
    }

	private void Awake()
	{
        if (!s_GameManager)
        {
            s_GameManager = this;
        }
        else{
            Destroy(this);
        }

        CheckpointManager.NewCheckpointCreatedEvent += AddMoreTime;
        player = GameObject.FindObjectOfType<CarController>().gameObject;
	}

	// Use this for initialization
	void Start () {
        StartCoroutine(CountDown());
        Time.timeScale = 1;
        startTime = Time.time;
	}
	

    // This method can be improve to have dynamic addtional time
    // base on the distance between the car and the new checkpoint
    private void AddMoreTime(CheckpointEventData data)
    {
        // Base Time  +  (10/#checkpoint);
        float BaseTime = 10f;
        //float distanceBetweenCarAndCP = Vector3.Distance(player.transform.position, 
                                                         //data.newLocation);
        
        currCountdownValue += BaseTime + (10 / data.numberOfCheckpoints) ;
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
