using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Appl.UI;
namespace Appl.State
{
    public class ResultDisplayState : ResultState
    {

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {


            UIManager.Instance.ClearUI();
            base.OnStateEnter(animator, stateInfo, layerIndex);

            uiMessagePanel.gameObject.SetActive(true);
            uiMessagePanel.Clear();
            uiMessagePanel.m_Title.gameObject.SetActive(true);
            uiMessagePanel.m_Content.gameObject.SetActive(true);

            uiMessagePanel.m_Title.text = string.Format("You've hit {0:D2} checkpoints.",
                                                   Game.GameManager.Instance.Game.CheckPointCount

                                                   );
            uiMessagePanel.m_Content.text = "Try again for only $2.99?";


            UIManager.Instance.GetUIComponent("ButtonsPanel").gameObject.SetActive(true);
        }

        public override void OnControllerButtonEvent(BEControllerButtons current, BEControllerButtons down, BEControllerButtons up)
        {
            if (down == BEControllerButtons.ButtonPrimary)
            {

                m_Animator.SetInteger("State", 1);
            }

        }
    }
}