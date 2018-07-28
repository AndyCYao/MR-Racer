using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICountdownPanel : UIComponent {



    Text m_TimerDisplay;

	// Use this for initialization
	void Awake () {
        m_TimerDisplay = transform.Find("TimerPanel").GetComponentInChildren<Text>();
        if (!m_TimerDisplay ){
            Debug.LogError("[UICountdownPanel]: Missing text component");
        }
	}
	
    public void SetTimer (float newTime ){
        m_TimerDisplay.text = newTime.ToString();
    }
	// Update is called once per frame

}
