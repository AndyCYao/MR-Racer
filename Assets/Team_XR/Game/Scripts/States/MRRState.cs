using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MRRState : StateMachineBehaviour {

    public delegate void OnGameState(MRRState state);
    public static OnGameState OnGameStateChangedEnter;
    public static OnGameState OnGameStateChangedExit;
	// Use this for initialization
	

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        OnGameStateChangedEnter(this);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
        OnGameStateChangedExit(this);
	}
}
