using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Appl.State
{
    public class State : StateMachineBehaviour
    {
        protected Animator m_Animator;
        public delegate void OnGameState(State state);
        public static OnGameState OnGameStateChangedEnter;
        public static OnGameState OnGameStateChangedExit;
        // Use this for initialization


        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_Animator = animator;
            BridgeEngineUnity.main.onControllerButtonEvent.AddListener(OnControllerButtonEvent);
            OnGameStateChangedEnter(this);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            BridgeEngineUnity.main.onControllerButtonEvent.RemoveListener(OnControllerButtonEvent);
            OnGameStateChangedExit(this);
        }

        public virtual void OnControllerButtonEvent(BEControllerButtons current, BEControllerButtons down, BEControllerButtons up)
        {

        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            BETrackerPoseAccuracy poseStatus = BridgeEngineUnity.main.TrackerPoseAccuracy();
            if (poseStatus == BETrackerPoseAccuracy.NotAvailable)
            {
                Appl.UI.UIManager.Instance.GetComponent<Canvas>().enabled = false;
            }
            else {
                Appl.UI.UIManager.Instance.GetComponent<Canvas>().enabled = true;
            }
        }
    }
}