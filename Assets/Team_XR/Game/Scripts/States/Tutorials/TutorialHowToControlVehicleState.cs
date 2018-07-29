using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Appl.UI;

namespace Appl.State
{
    public class TutorialHowToControlVehicleState : TutorialState
    {

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            uiMessagePanel.m_Title.text = "HOW TO CONTROL THE VEHICLE";
            uiMessagePanel.m_Content.text = "Crash it into the old lady";


            UIManager.Instance.GetUIComponent("ButtonsPanel").gameObject.SetActive(true);

      
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (Input.GetMouseButtonUp(0))
            {
                animator.SetInteger("SubState", 0);
                animator.SetInteger("State", 1);
            }
        }
    }
}