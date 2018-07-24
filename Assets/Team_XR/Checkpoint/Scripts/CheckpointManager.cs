using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{


   // public GameObject plane;
 //   public GameObject checkpointPrefab;
    GameObject player;

    //public int numberOfCheckpoints;

 //   [SerializeField]
 //   List<GameObject> checkpoints;
   // int remainingCheckPoints = 0;
    [SerializeField]
    int currentCheckPoints = 0;
    Transform checkPointObject;

    private void Awake()
    {

        //rayCastPoint = transform.position + transform.up * 2;
        //   int numOfCheckpointsLeft = numberOfCheckpoints;
       // checkpoints = new List<GameObject>();
        BEScene.OnEnvironmentMeshCreated += Reset;
        checkPointObject = transform.Find("Checkpoint");
        player = GameObject.FindWithTag("Player");
    }

	private void Reset()
	{
      //  remainingCheckPoints = 0;

        Checkpoint.CheckpointPassedEvent += OnCheckpointReached;

        player.transform.position = RayCastCheckpoint() + Vector3.up * 2f;
        checkPointObject.transform.position = RayCastCheckpoint();

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

    void OnCheckpointReached (int index) {
        Debug.Log(string.Format("CheckpointManager - OnCheckpointReached: {0}", index));
        checkPointObject.transform.position = RayCastCheckpoint();
        currentCheckPoints++;
        Debug.Log(string.Format("New checkpoint spawned at {0}", checkPointObject.transform.position.ToString()));

    }
}
