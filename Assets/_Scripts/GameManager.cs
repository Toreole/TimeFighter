using System;
using System.Collections;
using UnityEngine;
using Game.Controller;
using Discord;
using DiscordApp = Discord.Discord;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance = null;
        
        private SaveData save = null;
        private bool firstShutdown = true;
        [SerializeField]
        new AudioSource audio;

#if DISCORD
        public static DiscordApp discord;
        public static Discord.User user;
#endif

        private void Awake()
        {
#if DISCORD
            //epic gamer hours hmmm
            var mang = discord.GetUserManager();
            mang.OnCurrentUserUpdate += () =>
            {
                user = mang.GetCurrentUser();
                Debug.Log(user.Username);
                if (mang.CurrentUserHasFlag(UserFlag.HypeSquadHouse1))
                    Debug.Log("BRAVERY!");
                else if (mang.CurrentUserHasFlag(UserFlag.HypeSquadHouse2))
                    Debug.Log("BRILLIANCE!");
                else if (mang.CurrentUserHasFlag(UserFlag.HypeSquadHouse3))
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
            if (firstShutdown)
            {
                firstShutdown = false;
                Application.CancelQuit();
                audio.Play();
                StartCoroutine(DelayedShutdown());
                return;
            }
            TrySave();
        }

        private System.Collections.IEnumerator DelayedShutdown()
        {
            yield return new WaitForSeconds(2.0f);
            Application.Quit();
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