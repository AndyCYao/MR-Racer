using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CheckpointEventData
{
    public Vector3 newLocation;
    public int numberOfCheckpoints;
}

public class CheckpointManager : MonoBehaviour
{


   // public GameObject plane;
 //   public GameObject checkpointPrefab;
    [SerializeField]
    GameObject player;

    //public int numberOfCheckpoints;

 //   [SerializeField]
 //   List<GameObject> checkpoints;
   // int remainingCheckPoints = 0;
    [SerializeField]
    int currentCheckPoints = 0;
    Transform checkPointObject;

    public delegate void NewCheckpointCreated(CheckpointEventData data);
    public static event NewCheckpointCreated NewCheckpointCreatedEvent;

    private void Awake()
    {

        //rayCastPoint = transform.position + transform.up * 2;
        //   int numOfCheckpointsLeft = numberOfCheckpoints;
       // checkpoints = new List<GameObject>();
        BEScene.OnEnvironmentMeshCreated += Reset;
       
        checkPointObject = transform.Find("Checkpoint");
        //player = GameManager.Instance.player;
    }

	private void Reset()
	{
        Checkpoint.CheckpointPassedEvent += OnCheckpointReached;
        BridgeEngineUnity.main.onControllerButtonEvent.AddListener(OnControllerButton);
        SpawnPlayerRandomly();
        checkPointObject.transform.position = RayCastCheckpoint();

	}

    [SerializeField]
    float confirmTime = -1;
    private void OnControllerButton(BEControllerButtons current, BEControllerButtons down, BEControllerButtons up) { 
        

        if (current == (BEControllerButtons.ButtonPrimary | BEControllerButtons.ButtonSecondary)) {

            if ((down == BEControllerButtons.ButtonPrimary || down == BEControllerButtons.ButtonSecondary)
            && Mathf.Approximately(confirmTime, -1))
            {
                Debug.Log("CheckpointManager - OnControllerButton: Reset button sequence being pressed!");
                confirmTime = 0;
            }

            else
            {
               


                    confirmTime += Time.deltaTime;
                
            }
        }

        if (up == BEControllerButtons.ButtonPrimary || up == BEControllerButtons.ButtonSecondary) {
            
            if (confirmTime > 1)
            {
                Debug.Log(string.Format("CheckpointManager - OnControllerButton: Reset Vehicle!"));
                SpawnPlayerRandomly();
               
            }
            confirmTime = -1;
        }
    }

    private void SpawnPlayerRandomly () {
        GameManager.Instance.player.transform.position = RayCastCheckpoint() + Vector3.up * .1f;
    }


    private Vector3 RayCastCheckpoint()
    {
        RaycastHit hit;
        Ray ray = new Ray();
        ray.origin = transform.position;
        
       // bool hasCreated = false;
        while(true){
            ray.direction = Random.onUnitSphere;
            ray.direction = new Vector3(
                ray.direction.x,
                -Mathf.Abs(ray.direction.y),
                ray.direction.z
            );

            ray.direction.Normalize();

            if(Physics.Raycast(ray, out hit) && hit.transform.gameObject.layer == 8)
            {
                Debug.Log(string.Format ("Hit at point {0}", hit.point.ToString()));
                         
                
                if(Vector3.Angle (hit.normal, Vector3.up) < 10f && hit.point.y < .2f)    // .2 on y is considered floor height
                {
                    return  hit.point;

                }
            }            
        }
    }

    void OnCheckpointReached (int index) 
    {
        Debug.Log(string.Format("CheckpointManager - OnCheckpointReached: {0}", index));
        checkPointObject.transform.position = RayCastCheckpoint();
        currentCheckPoints++;
        CheckpointEventData payload = new CheckpointEventData
        {
            newLocation = checkPointObject.transform.position,
            numberOfCheckpoints = currentCheckPoints
        };
        NewCheckpointCreatedEvent(payload);

        Debug.Log(string.Format("New checkpoint spawned at {0}", checkPointObject.transform.position.ToString()));
    }
}
