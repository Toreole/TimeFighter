#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Game;

namespace Game.Menu
{
    [CustomEditor(typeof(LevelSystem))]
    public class LevelSystemEditor : Editor
    {
        LevelSystem _target { get { return target as LevelSystem; } }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorUtility.SetDirty(_target);
            if (GUILayout.Button("Add Node"))
                _target.CreateNewLevel();
        }

        private void OnSceneGUI()
        {
            
        }
    }
}
#endif