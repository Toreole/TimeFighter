using UnityEngine;
using UnityEditor;
using Game.Controller.Input.Obsolete;
using System.IO;
using System.Text;

namespace Game.Editor.Obsolete
{
    public class InputMapEditor : EditorWindow
    {
        public static TextAsset asset;

        public static InputMap target;
        public static string path;

        public static void OpenWindow()
        {
            var window = GetWindow<InputMapEditor>("Input Map Editor", true);
        }

        //simple 
        public void OnGUI()
        {
            CreateOrLoad();
            if(!(target is null)) 
                ShowTarget();
        }

        void CreateOrLoad()
        {
            if (GUILayout.Button("Create New Input Map"))
            {
                path = EditorUtility.SaveFilePanelInProject("Save New Input Map", "New InputMap", "json", "Save New Input Map");
                if (!string.IsNullOrEmpty(path))
                {
                    target = new InputMap();
                    string jsonContent = JsonUtility.ToJson(target);
                    byte[] buffer = Encoding.ASCII.GetBytes(jsonContent);
                    FileStream stream = File.Create(path);
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();
                    stream.Close();
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.Default);
                }
            }
            if (GUILayout.Button("Load Input Map From File"))
            {
                path = EditorUtility.OpenFilePanelWithFilters("Load Input Map", Application.dataPath, new string[] { "Text" , "json"});
                if(!string.IsNullOrEmpty(path))
                {
                    FileStream stream = File.Open(path, FileMode.Open);
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, (int)stream.Length);
                    string content = Encoding.ASCII.GetString(buffer);
                    stream.Flush();
                    stream.Close();
                    target = JsonUtility.FromJson<InputMap>(content);
                }
            }
        }

        void ShowTarget()
        {
            GUILayout.Label("Edit Input Map");
            target.versionID = EditorGUILayout.TextField("Version ID", target.versionID);
            foreach(var binding in target.bindings)
            {
                Show(binding);
            }

            GUI.backgroundColor = Color.blue;
            if(GUILayout.Button("Add Binding"))
            {
                var buffer = target.bindings;
                target.bindings = new InputBinding[buffer.Length + 1];
                for(int i = 0; i < buffer.Length; i++)
                {
                    target.bindings[i] = buffer[i];
                }
                target.bindings[buffer.Length] = new InputBinding();
            }

            GUI.backgroundColor = Color.green;
            if(GUILayout.Button("Save Asset"))
            {
                path = EditorUtility.SaveFilePanelInProject("Save Input Map", "InputMap", "json", "Save Input Map");
                if (!string.IsNullOrEmpty(path))
                {
                    string jsonContent = JsonUtility.ToJson(target);
                    byte[] buffer = Encoding.ASCII.GetBytes(jsonContent);
                    File.Delete(path);
                    FileStream stream = File.Create(path);
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();
                    stream.Close();
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.Default);
                }
            }
        }

        void Show(InputBinding bind)
        {
            bind.show = EditorGUILayout.Foldout(bind.show, bind.name);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            if (bind.show)
            {
                //now edit this stuff.
                bind.name = EditorGUILayout.TextField("Binding Name", bind.name);
                bind.axis = (InputButtonAxis)EditorGUILayout.EnumPopup("Axis", bind.axis);
                bind.positive = (KeyCode) EditorGUILayout.EnumPopup("Positive Key", bind.positive);
                bind.negative = (KeyCode)EditorGUILayout.EnumPopup("Negative Key", bind.negative);
                bind.fightOutcome = (AxisFightOutcome)EditorGUILayout.EnumPopup("Fight Outcome", bind.fightOutcome);
            }
            GUILayout.EndVertical();
        }
    }
}