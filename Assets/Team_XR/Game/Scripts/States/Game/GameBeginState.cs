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

            //Game.GameManager.Instance.Game.Player.SetActive(true);

            //game.Player.transform.Find("Car_Body").GetComponent<Renderer>().enabled = true;

            //foreach (Renderer wheel_renderer in game.Player.transform.Find("Car_Body").Find("wheels").GetComponentsInChildren<Renderer>())
            //{
            //    wheel_renderer.enabled = true;
            //}


            Game.GameManager.Instance.Game.Player
                .GetComponent<BridgeEngine.Input.CarControllerInput>()
                .SetAnimationBool("isDriverEjected", false);

            Game.GameManager.Instance.Game.Player
                .GetComponent<BridgeEngine.Input.CarControllerInput>()
                .m_CarVisibility.SetActive(true);

        }
    }
}