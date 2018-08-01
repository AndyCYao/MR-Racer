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

    static CheckpointManager s_Instance;
    public static CheckpointManager Instance {
        get { return s_Instance; }
    }

    Transform checkPointObject;

    public delegate void NewCheckpointCreated(CheckpointEventData data);
    public static event NewCheckpointCreated NewCheckpointCreatedEvent;

    public Vector3 CurrentCheckpointPosition(){
        return checkPointObject.transform.position;
    }

    private void Awake()
    {
        

        if (!s_Instance)
            s_Instance = this;
        else {
            Destroy(gameObject);
        }

        BEScene.OnEnvironmentMeshCreated += () => { Reset(true); };

        checkPointObject = transform.Find("Checkpoint");
  
    }

	public void Reset(bool isResetSubscription=false)
	{
        if (isResetSubscription)
            Checkpoint.CheckpointPassedEvent += OnCheckpointReached;
    
        //BridgeEngineUnity.main.onControllerButtonEvent.AddListener(OnControllerButton);

        SpawnPlayerRandomly();
        checkPointObject.transform.position = CustomRaycasting.RayCastToScene(transform.position);

	}


    private void SpawnPlayerRandomly()
    {
        
        Game.GameManager.Instance.Game.Player.transform.position = CustomRaycasting.RayCastToScene(transform.position) + Vector3.up * .1f;
    }


 

    public void OnCheckpointReached (int index) {
        Debug.Log(string.Format("CheckpointManager - OnCheckpointReached: {0}", index));
        //  checkPointObject.transform.position = CustomRaycasting.RayCastToScene(transform.position);
        StartCoroutine(MakeNewCheckpoint(CustomRaycasting.RayCastToScene(transform.position)));
    
        Debug.Log(string.Format("New checkpoint spawned at {0}", checkPointObject.transform.position.ToString()));
    }

    public IEnumerator MakeNewCheckpoint(Vector3 newCheckpointPosition)
    {
        float transittingTime = 2f;
        float currentTimePassSinceTransition = 0f;
        Vector3 oldPosition = checkPointObject.position;

        Game.GameManager.Instance.Game.TimeRemaining += Game.Game.TimeAllowanceSetting.GetTimeBonus(
            Game.GameManager.Instance.Game.CheckPointCount, Vector3.Distance(oldPosition, newCheckpointPosition));

        Game.GameManager.Instance.Game.CheckPointCount++;
        Checkpoint.CheckpointPassedEvent -= OnCheckpointReached;
        while (currentTimePassSinceTransition < transittingTime)
        {
            currentTimePassSinceTransition += Time.deltaTime;
            checkPointObject.position = Vector3.Lerp(oldPosition, newCheckpointPosition, currentTimePassSinceTransition / transittingTime);
            yield return new WaitForEndOfFrame();
        }

        Checkpoint.CheckpointPassedEvent += OnCheckpointReached;
    }

}
