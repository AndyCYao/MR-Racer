using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Appl.UI;
using Game;
namespace Appl.State
{
    public class GamePlayingState : GameState
    {
   
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
       //     Checkpoint.CheckpointPassedEvent += OnCheckpointPassed;
            m_Game = GameManager.Instance.Game;
            m_Game.OnGameOverEvent += OnGameOver;
            //uiMessagePanel = UIManager.Instance.GetUIComponent("MessagePanel").GetComponent<UI.UIMessagePanel>();
            m_Animator = animator;
           
            Game.GameManager. Instance.Game.Player.GetComponent<Rigidbody>().isKinematic = false;


            //Game.GameManager.Instance.Game.Player
                //.GetComponent<BridgeEngine.Input.CarControllerInput>()
                //.SetAnimationBool("isDriverEjected", false);


            //Checkpoint.CheckpointPassedEvent += CheckpointManager.Instance.OnCheckpointReached;
            m_Game.CheckPointCount = 0;
            base.OnStateEnter(animator, stateInfo, layerIndex);
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

            base.OnStateUpdate(animator, stateInfo, layerIndex);


        }

 

        void OnGameOver (){
            m_Animator.SetTrigger("GameOver");
        }

		public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
		{
            base.OnStateMachineExit(animator, stateMachinePathHash);
           // Checkpoint.CheckpointPassedEvent -= CheckpointManager.Instance.OnCheckpointReached;
		}
	}
}