using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Serialization;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Game
{
    /// <summary>
    /// Manages the Game. Starts the game after some delay in the scene.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public string levelID;
        protected Dictionary<string, SerializedMonoBehaviour> idToObject;

        public void Load(LevelData data)
        {
            SerializedMonoBehaviour target;
            foreach(ObjectData objData in data.objectData)
            {
                //get the targetted serializedobject in the scene
                if(idToObject.TryGetValue(objData.objectID, out target))
                {
                    //pass the data to it.
                    target.Deserialize(objData);
                }
            }
        }
        //Save the level.
        public LevelData Save()
        {
            LevelData data = new LevelData();
            data.levelID = this.levelID;
            data.objectData = new List<ObjectData>();
            //add all serialized objects to the list.
            foreach(var sObj in idToObject.Values)
            {
                data.objectData.Add(sObj.Serialize());
            }
            return data;
        }

#if UNITY_EDITOR
        [MenuItem("Tools/Recollect Save Objects")]
        public static void RecollectSaveObjects()
        {
            if(EditorSceneManager.loadedSceneCount > 0)
            {
                Debug.LogWarning("Recollecting Save Objects does not support multi-scene editing!");
                return;
            }
            LevelManager manager = FindObjectOfType<LevelManager>();
            if(!manager)
            {
                Debug.LogWarning("There was no LevelManager in-scene. Create one and try again!");
                return;
            }
            Recollect(manager);
        }

        static void Recollect(LevelManager manager)
        {
            SerializedMonoBehaviour[] objects = FindObjectsOfType<SerializedMonoBehaviour>();
            if (objects.Length == 0)
                return;
            foreach (var serializedObject in objects)
            {
                //if the string is Null or Empty it means that it does no have an ID yet.
                if (string.IsNullOrEmpty(serializedObject.objectID))
                {
                    serializedObject.objectID = GetUniqueID(manager);
                    EditorUtility.SetDirty(serializedObject); //mark the object for saving in the editor.
                }
            }
            //clear the dictionary ones.
            manager.idToObject.Clear();
            foreach (var sObject in objects)
                manager.idToObject.Add(sObject.objectID, sObject);

            EditorUtility.SetDirty(manager);//mark the manager for saving in the editor.
        }

        //idk why hexadecimal but its neat
        const string characters = "0123456789abcdef";
        const int ID_LENGTH = 8;
        static string GetUniqueID(LevelManager manager)
        {
            //build a random string from those characters.
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            for (int i = 0; i < ID_LENGTH; i++)
            {
                builder.Append(characters[Random.Range(0, 15)]);
            }
            string id = builder.ToString();
            //if the key already exists (which should rarely ever happen at all), get a new one recursively.
            if (manager.idToObject.ContainsKey(id))
                return GetUniqueID(manager);
            return id;
        }
#endif
    }
}