using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Game.Controller;
using UnityEngine;

namespace Game.Serialization
{
    internal static class SaveManager
    {
        internal static Discord.StorageManager discordStorageManager;
        const string fileName = "pldata.tsd";

        public static string SaveLocation
#if DISCORD
             => discordStorageManager is null ? Application.persistentDataPath : discordStorageManager.GetPath();
#elif STEAM
            => "default";
#else
            => Application.persistentDataPath;
#endif

        static string FullFilePath => Path.Combine(SaveLocation, fileName);

        internal static bool TryLoad(out SaveData data)
        {
            data = null;
            //default path
            var path = FullFilePath;

            if (!File.Exists(path))
                return false;
            FileStream file = File.Open(path, FileMode.OpenOrCreate);
            if (file.Length == 0) //check for empty stream.
                return false;
            BinaryFormatter formatter = new BinaryFormatter();
            data = formatter.Deserialize(file) as SaveData;
            file.Flush();
            file.Dispose();
            return true;
        }

        internal static void TrySave(SaveData data)
        {
            Debug.Log(FullFilePath);
            FileStream file = File.Open(FullFilePath, FileMode.CreateNew);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(file, data);
            file.Flush();
            file.Dispose();
        }
    }

    [Serializable]
    internal class SaveData
    {
        public PlayerData playerData;
        public List<LevelData> levelData;

        public SaveData()
        {
            playerData = new PlayerData();
            levelData = new List<LevelData>();
        }
    }
}