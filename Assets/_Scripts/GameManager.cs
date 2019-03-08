using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Controller;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance = null;

        private SaveData save = new SaveData();
        
        private void Awake()
        {
            if (instance != null)
            {
                Destroy(this.gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}