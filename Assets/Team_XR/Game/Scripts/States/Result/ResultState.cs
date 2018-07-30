using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Appl.UI;

namespace Appl.State
{
    public class ResultState : State
    {
        protected UIMessagePanel uiMessagePanel;
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {


            uiMessagePanel = UIManager.Instance.GetUIComponent("MessagePanel").GetComponent<UI.UIMessagePanel>();

            base.OnStateEnter(animator, stateInfo, layerIndex);
        }
    }
}