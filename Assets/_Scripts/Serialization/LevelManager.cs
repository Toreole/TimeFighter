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
        [SerializeField, HideInInspector]
        protected ObjectDictionary idToObject = new ObjectDictionary();

        public void Load(LevelData data)
        {
            ISerialized target;
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
            foreach(var sObj in idToObject.values)
            {
                data.objectData.Add((sObj as ISerialized).Serialize());
            }
            return data;
        }

        ///<summary>
        ///level is loaded -> load the levels data if necessary.
        /// </summary>
        private void OnEnable()
        {
            if(GameManager.TryGetLevelData(levelID, out LevelData data))
            {
                Load(data);
            }
            GameManager.AddAutoSaveLevel(this);
        }

        //Level is unloaded -> save the levels data.
        private void OnDisable()
        {
            GameManager.ManualSave(Save()); //manually save this scene.
            GameManager.RemoveAutoSaveLevel(this);
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
            GameObject[] sceneObjects = GameObject.FindObjectsOfType<GameObject>();
            List<ISerialized> serializeTargets = new List<ISerialized>(sceneObjects.Length >> 3);
            foreach(var go in sceneObjects)
            {
                var serialized = go.GetComponent<ISerialized>();
                if(serialized != null)
                    serializeTargets.Add(serialized);
            }
            if (serializeTargets.Count == 0)
                return;
            foreach (var serializedObject in serializeTargets)
            {
                //if the string is Null or Empty it means that it does no have an ID yet.
                if (string.IsNullOrEmpty(serializedObject.ObjectID))
                {
                    serializedObject.ObjectID = GetUniqueID(manager);
                    EditorUtility.SetDirty(serializedObject as Object); //mark the object for saving in the editor.
                }
            }
            //clear the dictionary ones.
            manager.idToObject.Clear();
            foreach (var sObject in serializeTargets)
                manager.idToObject.Add(sObject.ObjectID, sObject as Object);

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

    [System.Serializable]
    public class ObjectDictionary
    {
        public List<string> keys = new List<string>();
        public List<Object> values = new List<Object>(); //AAAAA has to be Object and cast to ISerialized because unity doesnt want to serialize interfaces.

        public void Clear()
        {
            keys.Clear();
            values.Clear();
        }

        public void Add(string key, Object value)
        {
            keys.Add(key);
            values.Add(value);
        }

        public bool ContainsKey(string key)
        {
            return keys.Contains(key);
        }

        public bool TryGetValue(string key, out ISerialized value)
        {
            if(ContainsKey(key))
            {
                value = values[keys.IndexOf(key)] as ISerialized;
                return true;
            }
            value = null;
            return false;
        }
    }
}