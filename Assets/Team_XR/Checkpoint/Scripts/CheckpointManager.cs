using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{


   // public GameObject plane;
    public GameObject checkpointPrefab;
    public int numberOfCheckpoints;

    [SerializeField]
    List<GameObject> checkpoints;
    int remainingCheckPoints = 0;
  


    private void Awake()
    {

        //rayCastPoint = transform.position + transform.up * 2;
        //   int numOfCheckpointsLeft = numberOfCheckpoints;
        checkpoints = new List<GameObject>();
        BEScene.OnEnvironmentMeshCreated += Reset;
    }

	private void Reset()
	{
        remainingCheckPoints = 0;

        Checkpoint.CheckpointPassedEvent += OnCheckpointReached;

        while (remainingCheckPoints < numberOfCheckpoints)

        {
            CreateCheckpointOnFloor();
            //remainingCheckPoints++;
        }
	}


	private void Start()
	{
        
	}

	// Update is called once per frame
	void Update()
    {
        
    }

    private void CreateCheckpointOnFloor()
    {
        RaycastHit hit;
        Ray ray = new Ray();
        ray.origin = transform.position;
        
        bool hasCreated = false;
        while(!hasCreated){
            ray.direction = new Vector3(Random.value, -Random.value, Random.value);
            if(Physics.Raycast(ray, out hit) && 
               hit.transform.gameObject.layer == 8)
            {
                Debug.Log("hit x\t" + hit.point.x + " hit y\t" + hit.point.y +
                        " hit z\t" + hit.point.z);
                
                if(Vector3.Angle (hit.normal, Vector3.up) < 10f && hit.point.y < .2f)    // .2 on y is considered floor height
                {
                    checkpoints.Add (
                        Instantiate(checkpointPrefab, hit.point, Quaternion.identity)
                    );
                    checkpoints[remainingCheckPoints].name = string.Format("Checkpoint_{0}", remainingCheckPoints);
                    checkpoints[remainingCheckPoints].GetComponent<Checkpoint>().index = remainingCheckPoints;
 
                    remainingCheckPoints++;
                    hasCreated = true;
                }
            }            
        }
    }

    void OnCheckpointReached (int index) {
        remainingCheckPoints--;
        Debug.Log(string.Format("CheckpointManager - OnCheckpointReached: {0}", index));
        if (remainingCheckPoints == 0) {
            Debug.Log("All check points are collected yay");
            Reset();
        }
    }
}
