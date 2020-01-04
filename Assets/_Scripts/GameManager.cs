using System;
using System.Collections;
using UnityEngine;
using Game.Controller;
using Discord;
using Game.Serialization;
using DiscordApp = Discord.Discord;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance = null;
        private bool initialized = false;

        //current active saveData.
        private SaveData saveData = null;

        //autoSave:
        private Func<LevelData> OnAutoSave;

#if DISCORD
        public static DiscordApp discord;
        public static Discord.User user;
        public static UserManager userManager;
#endif

        private void Awake()
        {
            if (initialized)
                return;
#if DISCORD
            //epic gamer hours hmmm
            if (discord != null)
            {
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
            }
#endif
            if (instance)
            {
                Destroy(this.gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (saveData == null)
            {
                if (!SaveManager.TryLoad(out saveData))
                {   //try to load, if it cant load, create a new save
                    saveData = new SaveData();
                    //Debug.Log("Creating New SaveData"); 
                }
            }
        }
        private void OnGUI()
        {
            GUILayout.Label(GameInfo.Version);
        }
        private void Update()
        {
#if DISCORD
            discord.RunCallbacks();
#endif
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
            if (!instance)
            {
                return null;
            }
            return instance.saveData;
        }

        //try saving.
        internal static bool TrySave()
        {
            if (!instance)
            {
                return false;
            }
            SaveManager.TrySave(instance.saveData);
            return true;
        }
        
        //Getting the data for this level.
        public static bool TryGetLevelData(string levelID, out LevelData data)
        {
            if (!instance)
            {
                data = null;
                return false;
            }
            return instance.M_TryGetLevelData(levelID, out data);
        }
        public bool M_TryGetLevelData(string levelID, out LevelData data)
        {
            data = null;
            foreach (var lData in saveData.levelData)
                if (lData.levelID == levelID)
                {
                    data = lData;
                    return false;
                }
            return false;
        }

        //Manually save a level.
        public static void ManualSave(LevelData data)
        {
            if (!instance)
            {
                return;
            }
            instance.M_ManualSave(data);
        }
        public void M_ManualSave(LevelData data)
        {
            for (int i = 0; i < saveData.levelData.Count; i++)
            {
                if (saveData.levelData[i].levelID.Equals(data.levelID))
                {
                    //Remove the item at this index, to replace it with a new one.
                    saveData.levelData.RemoveAt(i);
                    break;
                }
            }
            saveData.levelData.Add(data);
            SaveManager.TrySave(saveData);
        }

        public static void AddAutoSaveCall(Func<LevelData> dataCallback)
        {
            if (!instance)
            {
                return;
            }
            instance.OnAutoSave += dataCallback;
        }
        public static void RemoveAutoSaveCall(Func<LevelData> dataCallback)
        {
            if (!instance)
            {
                return;
            }
            instance.OnAutoSave -= dataCallback;
        }

        //TODO: implement this in the GameManager loop.
        void AutoSave()
        {
            foreach(Func<LevelData> call in OnAutoSave.GetInvocationList())
            {
                //get each levels data.
                LevelData result = call();
                //replace the data in the save, then save it.
                for(int i = 0; i < saveData.levelData.Count; i++)
                {
                    if(saveData.levelData[i].levelID.Equals(result.levelID))
                    {
                        //Remove the item at this index, to replace it with a new one.
                        saveData.levelData.RemoveAt(i);
                        break;
                    }
                }
                saveData.levelData.Add(result);
            }
            //TODO: also save player in here.
            //saveData.playerData = Player.Save(); <- temp.
            SaveManager.TrySave(saveData);
        }

        private static GameManager CreateNewInstance()
        {
            GameObject obj = Instantiate(new GameObject("_New GameManager"));
            instance = null;
            var manager = obj.AddComponent<GameManager>();
            return manager;
        }
    }
}