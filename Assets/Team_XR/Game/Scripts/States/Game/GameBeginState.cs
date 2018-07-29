using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Appl.UI;
using Game;

namespace Appl.State
{
    public class GameBeginState : GameState
    {



        // Use this for initialization
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            UIManager.Instance.ClearUI();
           

            UIManager.Instance.GetUIComponent("GameVisualInfoPanel").gameObject.SetActive(true);


            Game.Game game = GameManager.Instance.Game;
            game.StartCoroutine(
               game.CountDown());
            animator.SetTrigger("NextSubState");


        }


    }
}