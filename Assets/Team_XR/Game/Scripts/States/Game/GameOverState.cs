using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Appl.UI;

namespace Appl.State
{
    public class GameOverState : GameState
    {
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            
            //uiMessagePanel = UIManager.Instance.GetUIComponent("MessagePanel").GetComponent<UI.UIMessagePanel>();

            base.OnStateEnter(animator, stateInfo, layerIndex);
            Game.GameManager.Instance.Game.Player.GetComponent<Rigidbody>().isKinematic = true;
            UIManager.Instance.GetUIComponent("CountdownTextPanel").GetComponentInChildren<UnityEngine.UI.Text>().text = "GameOver";
            UIManager.Instance.GetUIComponent("CountdownTextPanel").gameObject.SetActive(true);
            animator.SetInteger("State", 3);
        }

    }
}
