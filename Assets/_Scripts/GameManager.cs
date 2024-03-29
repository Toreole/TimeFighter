﻿using System;
using System.Collections;
using UnityEngine;
using Game.Controller;
using Discord;
using DiscordApp = Discord.Discord;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get => instance; }
        private static GameManager instance = null;
        
        private SaveData save = null;

#if DISCORD
        public static DiscordApp discord;
        public static Discord.User user;
        public static UserManager userManager;
#endif

        private void Awake()
        {
#if DISCORD
            //epic gamer hours hmmm
            userManager = discord.GetUserManager();
            userManager.OnCurrentUserUpdate += () =>
            {
                user = userManager.GetCurrentUser();
                Debug.Log(user.Username);
                if (userManager.CurrentUserHasFlag(UserFlag.HypeSquadHouse1))
                    Debug.Log("BRAVERY!");
                else if (userManager.CurrentUserHasFlag(UserFlag.HypeSquadHouse2))
                    Debug.Log("BRILLIANCE!");
                else if (userManager.CurrentUserHasFlag(UserFlag.HypeSquadHouse3))
                    Debug.Log("BALANCE!");
                else
                    Debug.Log("lmao normie");
            };
#endif
            if (instance != null)
            {
                Destroy(this.gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (save == null)
            {
                if (!SaveManager.TryLoad(out save))
                {   //try to load, if it cant load, create a new save
                    save = new SaveData();
                    //Debug.Log("Creating New SaveData"); 
                }
            }
        }
        private void OnGUI()
        {
            GUILayout.Label(GameInfo.Version); 
        }
#if DISCORD
        private void Update()
        {
            discord.RunCallbacks();
        }
#endif
        private void Start()
        {
            if (instance != this)
                return;
            //Debug.Log("hallo");
        }

        /// <summary>
        /// forcefully save before quitting.
        /// </summary>
        private void OnApplicationQuit()
        {
            bool isCrash = !Input.GetKey(KeyCode.F4); //alt f4 pressed hmmm
            if(isCrash)
            {
                //TODO: maybe try and give some error stuff idk wtf. probably: opened scenes, last known player state, player x/y, OS version, similar stuff.
            }
            TrySave();
        }

        /// <summary>
        /// Try to fetch the SaveData
        /// </summary>
        /// <returns></returns>
        internal static SaveData FetchSave()
        {
            if (instance == null)
                return null;
            return instance.save;
        }

        internal static bool TrySave()
        {
            if (instance == null)
                return false;
            SaveManager.TrySave(instance.save);
            return true;
        }

        internal static void SetLastLevel(string level)
        {
            if (instance == null)
                return;
            instance.save.lastLevel = level;
        }

        internal static string GetLastLevel() => (instance != null)? instance.save.lastLevel : "";

        /// <summary>
        /// Add a level to the completed ones.
        /// </summary>
        /// <param name="level"></param>
        internal static void SetLevelComplete(string level)
        {
            if(!instance.save.completedLevels.Exists(x => x == level))
                instance.save.completedLevels.Add(level);
        }

        internal static GameManager CreateNewInstance()
        {
            GameObject obj = new GameObject();
            instance = null;
            var manager = obj.AddComponent<GameManager>();
            return manager;
        }
    }
}