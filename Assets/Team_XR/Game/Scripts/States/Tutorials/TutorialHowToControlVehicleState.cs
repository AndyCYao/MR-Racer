using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Appl.UI;
using UnityEngine.UI;
namespace Appl.State
{
    public class TutorialHowToControlVehicleState : TutorialState
    {
        Image m_Figure;


        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            uiMessagePanel.m_Figure.gameObject.SetActive(true);
            uiMessagePanel.m_Title.text = "HOW TO CONTROL THE VEHICLE";
            uiMessagePanel.m_Content.gameObject.SetActive(false);



            //UIManager.Instance.GetUIComponent("ButtonsPanel").gameObject.SetActive(true);

            UITextDescription textDescription = uiMessagePanel.m_UITextDescription;
            textDescription.gameObject.SetActive(true);
            textDescription.MakeDescriptionText("SecondaryButton", false, "Reset");
            textDescription.MakeDescriptionText("PowerButton", false, "Power");
            textDescription.MakeDescriptionText("TouchPad", true, "Touch Face\nHOLD & PRESS UP");
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