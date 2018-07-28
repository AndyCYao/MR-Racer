using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameState : MRRState {

    protected bool isPaused = false;
  
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameManager.Instance.Game.

        OnGameStateChangedEnter(this);
    }
}
