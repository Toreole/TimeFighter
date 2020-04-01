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

        /// <summary>
        /// try loading the save.
        /// </summary>
        /// <param name="data">The data to get</param>
        /// <returns>returns true if there was a file to load.</returns>
        internal static bool TryLoad(out SaveData data)
        {
            data = null;
            //default path
            var path = FullFilePath;

            if (!File.Exists(path))
                return false;
            FileStream file = File.Open(path, FileMode.Open);
            if (file.Length < 10) //check for empty stream. 
            {
                file.Close();
                return false;
            }
            byte[] buffer = new byte[file.Length]; //adapt the buffer size to be equal to the files length.
            file.Read(buffer, 0, (int)file.Length);
            string json = System.Text.Encoding.UTF8.GetString(buffer);
            data = JsonUtility.FromJson<SaveData>(json);
            //BinaryFormatter formatter = new BinaryFormatter();
            //data = formatter.Deserialize(file) as SaveData;
            file.Flush();
            file.Dispose();
            file.Close();
            return true;
        }
        //TODO: encryption for the json files. 
        //Tries to save. Doesnt return anything tho.
        internal static void TrySave(SaveData data)
        {
            var path = FullFilePath;
            //data to json
            var json = JsonUtility.ToJson(data);
            //json to byte[]
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(json);
            Debug.Log(path);
            //check if the file exists, delete it
            if (File.Exists(path))
                File.Delete(path);
            //create a new file.
            FileStream file = File.Open(path, FileMode.CreateNew);
            //write the buffer to the file.
            file.Write(buffer, 0, buffer.Length);
            //BinaryFormatter formatter = new BinaryFormatter();
            //formatter.Serialize(file, data);
            file.Flush();
            file.Dispose();
            file.Close();
        }
    }
}