using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBeginState : GameState{

    Game m_Game;

	// Use this for initialization
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        UIManager.Instance.ClearUI();
        m_Game = GameManager.Instance.Game;

        UIManager.Instance.GetUIComponent("TimerPanel").gameObject.SetActive(true);
  
        UIManager.Instance.GetUIComponent("CheckpointCountPanel").gameObject.SetActive(true);
      
        m_Game.StartCoroutine(m_Game.CountDown());
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }
}
