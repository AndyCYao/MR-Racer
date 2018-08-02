using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Appl.UI
{
    public class UIManager : MonoBehaviour
    {

        static UIManager s_Instance;
        public static UIManager Instance
        {
            get { return s_Instance; }
        }

        Dictionary<string, int> m_UIComponentsHash;

        UI.UIComponent[] m_UIComponentsArray;

        // Use this for initialization
        void Awake()
        {
            if (s_Instance)
            {
                Destroy(this);
                return;
            }
            s_Instance = this;

            transform.localPosition = new Vector3 (0,0,
                (BridgeEngineUnity.main.isStereoModeActive) ?  0.35f : .32f);
            m_UIComponentsHash = new Dictionary<string, int>();
            m_UIComponentsArray = GetComponentsInChildren<UI.UIComponent>();
            for (int i = 0; i < m_UIComponentsArray.Length; i++)
            {
                m_UIComponentsHash.Add(m_UIComponentsArray[i].name, i);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// Disable all UI
        /// </summary>
        public void ClearUI()
        {
            for (int i = 0; i < m_UIComponentsArray.Length; i++)
            {
                m_UIComponentsArray[i].gameObject.SetActive(false);
            }
        }

        public UIComponent GetUIComponent(string componentName)
        {
            if (m_UIComponentsHash.ContainsKey(componentName))
            {
                return m_UIComponentsArray[m_UIComponentsHash[componentName]];
            }
            return null;
        }

        static public void LogMissingChildError(string className, string childName)
        {
            Debug.LogError(string.Format("[UIManager]: {0} Missing object {1}", className, childName));
        }
    }
}