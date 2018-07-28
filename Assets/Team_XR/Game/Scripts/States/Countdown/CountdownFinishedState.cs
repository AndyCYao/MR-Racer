using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountdownFinishedState : CountdownState {

    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

       
        m_CountdownText = UIManager.Instance.GetUIComponent("CountdownTextPanel").GetComponentInChildren<Text>();
        m_CountdownText.text = "START";
        animator.SetInteger("State", 2);

        base.OnStateEnter(animator, stateInfo, layerIndex);
    }

 
}
