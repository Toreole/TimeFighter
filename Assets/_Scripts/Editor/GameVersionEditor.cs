using System;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public class GameVersionEditor : EditorWindow
    {
        [MenuItem("Window/Custom/Version")] 
        public static void OpenWindow()
        {
            var window = GetWindow<GameVersionEditor>("Version Editor", true);
        }

        static TextAsset target = null;

        private void OnGUI()
        {
            if(target is null)
            {
                target = Resources.Load<TextAsset>("Settings/Version");
            }
            //Parse it to the things, then change stuff.
            string version = target.text;
            var split = version.Split('.');
            //var year  = int.Parse(split[0]);
            //var month = int.Parse(split[1]);
            //var day   = int.Parse(split[2]);
            var bType = Enum.Parse(typeof(BuildType), split[3]);
            var topV  = int.Parse(split[4]);
            var midV  = int.Parse(split[5]);
            var botV  = int.Parse(split[6]);
            EditorGUILayout.BeginHorizontal();
            foreach (var x in split)
                EditorGUILayout.LabelField(x);
            EditorGUILayout.EndHorizontal();
            var today = DateTime.Today;
            Debug.Log(today.Day.ToString());
        }

        private enum BuildType
        {
            Alpha, Beta, Experimental, Release
        }
    }
}
