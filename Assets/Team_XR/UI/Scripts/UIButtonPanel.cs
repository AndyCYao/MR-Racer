using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Appl.UI{
	public class UIButtonPanel : UIComponent{

		Button[] m_uiButtons;

		void Awake() {
			base.Awake();
			m_uiButtons = GetComponentsInChildren<Button>();
			

		}

		public Button GetUIButton (int index) {
			if (index < m_uiButtons.Length) {
				return m_uiButtons[index];
			}
			return null;
		}
	}
}