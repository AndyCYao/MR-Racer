using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("GameState")]
public class Game : MonoBehaviour {

    public delegate void OnGameOver();
    public event OnGameOver OnGameOverEvent;

    
    class TimeAllowanceSetting {
        public const short C_StartTime = 10;
        public const short C_BaseTimeBonus = 5;

        public static float GetTimeBonusBasedOnNumberOfCheckPoints(int checkpointCount){
            return C_BaseTimeBonus - checkpointCount * 0.2f;
        }
    }






    GameObject m_player;
    public GameObject Player{
        get { return m_player; }
    }

    int m_CheckpointReached = 0;
    public int CheckPointCount {
        get { return m_CheckpointReached; }
    }

 
    private float m_TimeRemaining = 0f;
    public float TimeRemaining
    {
        get { return m_TimeRemaining; }
    }

    Coroutine m_CountdownRoutine = null;

	private void Awake()
	{
        
        m_player = GameObject.FindObjectOfType<BridgeEngine.Input.CarControllerInput>().gameObject;

        CheckpointManager.NewCheckpointCreatedEvent += OnNewCheckpointCreated;

        MRRState.OnGameStateChangedEnter    += OnGameStateChangedEnter;
        MRRState.OnGameStateChangedExit     += OnGameStateChangedExit;
	}

    private void OnNewCheckpointCreated(CheckpointEventData data)
    {
        m_CheckpointReached++;

        m_TimeRemaining += TimeAllowanceSetting.GetTimeBonusBasedOnNumberOfCheckPoints(m_CheckpointReached);

    }

	public  IEnumerator CountDown()
    {
        if (m_CountdownRoutine == null)
        {
        m_TimeRemaining = TimeAllowanceSetting.C_StartTime;
            UICountdownPanel timerPanel = (UICountdownPanel)UIManager.Instance.GetUIComponent("TimerPanel");

            while (m_TimeRemaining >= 0)
            {
                m_TimeRemaining -= Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }

            OnGameOverEvent();
            m_CountdownRoutine = null;
        }

    }

    public void OnGameStateChangedEnter(MRRState gameState){
        Debug.Log(string.Format("[Game]: OnGameState changed to {0}", gameState.GetType().ToString()));


       
    }
    public void OnGameStateChangedExit(MRRState gameState){}
    // Use this for initialization


}
