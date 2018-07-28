using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountingDownState : CountdownState {


    float m_RealCountdownTime;
    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {

        UIManager.Instance.ClearUI();

        UIManager.Instance.GetUIComponent("CheckpointCountPanel").gameObject.SetActive(true);
        UIManager.Instance.GetUIComponent("TimerPanel").gameObject.SetActive(true);
        UIManager.Instance.GetUIComponent("CountdownTextPanel").gameObject.SetActive(true);

        m_CountdownText = UIManager.Instance.GetUIComponent("CountdownTextPanel").GetComponentInChildren<Text>();

        m_RealCountdownTime = 3f;

        base.OnStateEnter(animator, stateInfo, layerIndex);
	}

	// OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (m_RealCountdownTime < 0)
        {
            animator.SetTrigger("NextSubState");
            return;
        }
        m_CountdownText.text = Mathf.Round(m_RealCountdownTime).ToString();
        m_RealCountdownTime -= Time.deltaTime;

	}

	// OnStateExit is called before OnStateExit is called on any state inside this state machine
	//override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateMove is called before OnStateMove is called on any state inside this state machine
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called before OnStateIK is called on any state inside this state machine
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateMachineEnter is called when entering a statemachine via its Entry Node
	//override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash){
	//
	//}

	// OnStateMachineExit is called when exiting a statemachine via its Exit Node
	//override public void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
	//
	//}
}
