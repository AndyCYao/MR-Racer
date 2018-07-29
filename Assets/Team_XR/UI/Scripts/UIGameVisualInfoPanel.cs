using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Appl.UI
{
    public class UIGameVisualInfoPanel : UIComponent
    {


        [SerializeField]
        Text m_TimerDisplay;
        public float Timer {
            set
            {
                value = (value > 0) ? value : 0;
                    
   
                TimeSpan t = TimeSpan.FromSeconds(value);


                m_TimerDisplay.text = string.Format("{0:D2}:{1:D2}:{2:D2}",

                                t.Minutes,
                                t.Seconds,
                                t.Milliseconds/10);
            }
        }

        [SerializeField]
        Text m_CheckpointCountDisplay;
        public int CheckpointCount {
            set { m_CheckpointCountDisplay.text = string.Format("{0:D2}", value); }

        }

        // Use this for initialization
        void Awake()
        {
            m_TimerDisplay = transform.Find("TimerPanel").GetComponentInChildren<Text>();
            if (!m_TimerDisplay)
            {
                Debug.LogError("[UIVisualInfoPanel]: Missing Timer text component");
            }

            m_CheckpointCountDisplay = transform.Find("CheckpointCountPanel").GetComponentInChildren<Text>();
            if (!m_TimerDisplay)
            {
                Debug.LogError("[UICountdownPanel]: Missing CheckpointText text component");
            }
        }

      

        // Update is called once per frame

    }
}