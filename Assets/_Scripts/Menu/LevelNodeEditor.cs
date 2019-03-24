#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Game.Menu
{
    [CustomEditor(typeof(LevelNode))]
    public class LevelNodeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if(GUILayout.Button("Select in LevelSystem"))
            {
                LevelSystemEditor.activeNode = target as LevelNode;
                Selection.objects = new UnityEngine.Object[]{ (target as LevelNode).transform.parent.gameObject };
            }
        }
    }
}
#endif