using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Appl.UI;

namespace Appl.State
{
    public class TutorialHowToPlayState : TutorialState

    {
        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //      Time.timeScale = 0f;
            base.OnStateEnter(animator, stateInfo, layerIndex);
            Game.GameManager.Instance.Game.Player.GetComponent<Rigidbody>().isKinematic = true;
          

            UIManager.Instance.ClearUI();
            UIManager.Instance.GetUIComponent("MessagePanel").gameObject.SetActive(true);

            uiMessagePanel.m_Title.text = "HOW TO PLAY";
            uiMessagePanel.m_Content.text = "Destroy all that you hold dear.";


            UIManager.Instance.GetUIComponent("ButtonsPanel").gameObject.SetActive(false);

           
        }

    }
}