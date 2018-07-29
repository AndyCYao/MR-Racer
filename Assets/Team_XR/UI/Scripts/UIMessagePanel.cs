﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("GameState")]
namespace Appl.UI
{
    public class UIMessagePanel : UIComponent
    {

        // Use this for initialization

        public Text m_Title;
        public Text m_Content;
        public Image m_TitleLine;
        public Image m_PanelImage;

        protected void Awake()
        {
            base.Awake();
            m_Title = transform.Find("Title").GetComponent<Text>();
            m_Content = transform.Find("Content").GetComponent<Text>();
            m_TitleLine = transform.Find("Line").GetComponent<Image>();
            m_PanelImage = transform.Find("Panel").GetComponent<Image>();
            //        m_ButtonsPanel = transform.Find("ButtonsPanel").gameObject;
#if DEBUG
            if (!m_Title)
                UIManager.LogMissingChildError(this.GetType().ToString(), "Title");
            if (!m_Content)
                UIManager.LogMissingChildError(this.GetType().ToString(), "Content");
            if (!m_TitleLine)
                UIManager.LogMissingChildError(this.GetType().ToString(), "TitleLine");
            if (!m_PanelImage)
                UIManager.LogMissingChildError(this.GetType().ToString(), "Panel");
#endif

        }




    }
}