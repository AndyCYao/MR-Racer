using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHowToPlayState : TutorialState
{
    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        UIManager.Instance.ClearUI();
        UIManager.Instance.GetUIComponent("MessagePanel").gameObject.SetActive(true);

        uiMessagePanel.m_Title.text = "HOW TO PLAY";
        uiMessagePanel.m_Content.text = "Destroy all that your enemies hold dear.";


        UIManager.Instance.GetUIComponent("ButtonsPanel").gameObject.SetActive(false);

        base.OnStateEnter(animator, stateInfo, layerIndex);
    }

}