using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.CompilerServices;

namespace Game
{
    [RequireComponent(typeof(Game))]
    public class GameManager : MonoBehaviour
    {


        Game m_Game;
        public Game Game
        {
            get
            {
                return m_Game;
            }
        }

        private static GameManager s_GameManager;

        public static GameManager Instance
        {
            get
            {
                return s_GameManager;
            }
        }

        private void Awake()
        {
            if (!s_GameManager)
            {
                s_GameManager = this;
            }
            else
            {
                Destroy(this);
            }

            m_Game = GetComponent<Game>();
        }




    }
}