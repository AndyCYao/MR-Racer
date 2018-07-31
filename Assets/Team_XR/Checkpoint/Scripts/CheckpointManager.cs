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
    [SerializeField]
    GameObject player;

    [SerializeField]
    int m_CheckpointCount = 0;
    public int CheckpointCount
    {
        get { return m_CheckpointCount; }
    }

    Transform checkPointObject;

    public delegate void NewCheckpointCreated(CheckpointEventData data);
    public static event NewCheckpointCreated NewCheckpointCreatedEvent;

    public Vector3 CurrentCheckpointPosition(){
        return checkPointObject.transform.position;
    }

    private void Awake()
    {
        BEScene.OnEnvironmentMeshCreated += Reset;
       
        checkPointObject = transform.Find("Checkpoint");
        //player = GameManager.Instance.player;
    }

	private void Reset()
	{
        Checkpoint.CheckpointPassedEvent += OnCheckpointReached;
        //BridgeEngineUnity.main.onControllerButtonEvent.AddListener(OnControllerButton);
        SpawnPlayerRandomly();
        checkPointObject.transform.position = CustomRaycasting.RayCastToScene(transform.position);

	}


    private void SpawnPlayerRandomly()
    {
        
        Game.GameManager.Instance.Game.Player.transform.position = CustomRaycasting.RayCastToScene(transform.position) + Vector3.up * .1f;
    }


 

    void OnCheckpointReached (int index) {
        Debug.Log(string.Format("CheckpointManager - OnCheckpointReached: {0}", index));
        checkPointObject.transform.position = CustomRaycasting.RayCastToScene(transform.position);

        Debug.Log(string.Format("New checkpoint spawned at {0}", checkPointObject.transform.position.ToString()));
    }

    IEnumerator MakeNewCheckpoint(Vector3 newCheckpointPosition)
    {
        float transittingTime = 3f;
        float currentTimePassSinceTransition = 0f;
        Vector3 oldPosition = checkPointObject.position;

        while (currentTimePassSinceTransition < transittingTime)
        {
            currentTimePassSinceTransition += Time.deltaTime;

            checkPointObject.position = Vector3.Lerp(oldPosition, newCheckpointPosition, currentTimePassSinceTransition / transittingTime);
            yield return new WaitForEndOfFrame();
        }
    }

}
