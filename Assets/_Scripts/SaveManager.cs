using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Game.Controller;
using UnityEngine;

namespace Game
{
    internal static class SaveManager
    {
        internal static Discord.StorageManager discordStorageManager;
        const string fileName = "HalfLifeThreeWillNeverExist.noobs";

        internal static bool TryLoad(out SaveData data)
        {
            data = null;
            //default path
            var path = Application.persistentDataPath + "/" + fileName;
#if DISCORD
            path = Path.Combine(discordStorageManager.GetPath(), fileName);
#elif STEAM
            path = "";
#endif
            if (!File.Exists(path))
                return false;
            FileStream file = File.Open(path, FileMode.OpenOrCreate);
            BinaryFormatter formatter = new BinaryFormatter();
            data = formatter.Deserialize(file) as SaveData;
            file.Flush();
            file.Dispose();
            return true;
        }

        internal static void TrySave(SaveData data)
        {
            Debug.Log(Application.persistentDataPath + "/" + fileName);
            FileStream file = File.Open(Application.persistentDataPath + "/" + fileName, FileMode.OpenOrCreate);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(file, data);
            file.Flush();
            file.Dispose();
        }
    }

    [Serializable]
    internal class SaveData
    {
        internal List<string> completedLevels = new List<string>();
        internal List<string> unlockedThrowables = new List<string>();
        internal string equippedThrowable = "";
        internal string lastLevel = "";
    }
}