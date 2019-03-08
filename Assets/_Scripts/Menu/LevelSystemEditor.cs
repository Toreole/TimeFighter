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
        internal static LevelNode activeNode;
        LevelSystem _target { get { return target as LevelSystem; } }
        GUIStyle bold = new GUIStyle();

        public override void OnInspectorGUI()
        {
            bold.fontStyle = FontStyle.Bold;

            base.OnInspectorGUI();
            EditorUtility.SetDirty(_target);
            if (GUILayout.Button("Add Node"))
            {
                var newLevel = _target.CreateNewLevel();
                activeNode = newLevel;
                return;
            }
            if (activeNode != null)
            {
                InspectActiveNode();
                MoveSelection();
            }
        }

        private void InspectActiveNode()
        {
            EditorUtility.SetDirty(activeNode);
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Active Node:", bold);
            activeNode.Position = EditorGUILayout.Vector3Field("Position:", activeNode.Position);
            activeNode.depth = EditorGUILayout.IntField("Depth:", activeNode.depth);
            activeNode.TargetScene = EditorGUILayout.TextField("Target Scene:", activeNode.TargetScene);
            activeNode.BaseColor = EditorGUILayout.ColorField("Active Color:", activeNode.BaseColor);
            activeNode.Unlocked = EditorGUILayout.Toggle("Unlocked:", activeNode.Unlocked);

            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Select As Node"))
            {
                Selection.objects = new UnityEngine.Object[] { activeNode.gameObject };
                EditorGUILayout.EndHorizontal();
                return;
            }
            if(GUILayout.Button("Remove Node"))
            {
                RemoveActiveNode();
                EditorGUILayout.EndHorizontal();
                return;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Neighbours:", bold);
            EditorGUILayout.BeginHorizontal();
                var north = EditorGUILayout.ObjectField("Northern:", activeNode.GetConnection(Connection.North), typeof(Transform),true) as Transform;
            if (north != null && GUILayout.Button("Select"))
            {
                activeNode = north.GetComponent<LevelNode>();
                EditorGUILayout.EndHorizontal();
                return;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
                var east = EditorGUILayout.ObjectField("Eastern:", activeNode.GetConnection(Connection.East), typeof(Transform), true) as Transform;
            if (east != null && GUILayout.Button("Select"))
            {
                activeNode = east.GetComponent<LevelNode>();
                EditorGUILayout.EndHorizontal();
                return;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
                var south = EditorGUILayout.ObjectField("Southern:", activeNode.GetConnection(Connection.South), typeof(Transform), true) as Transform;
            if (south != null && GUILayout.Button("Select"))
            {
                activeNode = south.GetComponent<LevelNode>();
                EditorGUILayout.EndHorizontal();
                return;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
                var west = EditorGUILayout.ObjectField("Western:", activeNode.GetConnection(Connection.West), typeof(Transform), true) as Transform;
            if (west != null && GUILayout.Button("Select"))
            {
                activeNode = west.GetComponent<LevelNode>();
                EditorGUILayout.EndHorizontal();
                return;
            }
            EditorGUILayout.EndHorizontal();

            bool changed = false;
            if (north != activeNode.GetConnection(Connection.North))
            {
                var other = north.GetComponent<LevelNode>();
                other.SetConnection(activeNode.transform, Connection.South);
                activeNode.SetConnection(other.transform, Connection.North);
                changed = true;
            }
            if (east != activeNode.GetConnection(Connection.East))
            {
                var other = east.GetComponent<LevelNode>();
                other.SetConnection(activeNode.transform, Connection.West);
                activeNode.SetConnection(other.transform, Connection.East);
                changed = true;
            }
            if (south != activeNode.GetConnection(Connection.South))
            {
                var other = south.GetComponent<LevelNode>();
                other.SetConnection(activeNode.transform, Connection.North);
                activeNode.SetConnection(other.transform, Connection.South);
                changed = true;
            }
            if (west != activeNode.GetConnection(Connection.West))
            {
                var other = west.GetComponent<LevelNode>();
                other.SetConnection(activeNode.transform, Connection.East);
                activeNode.SetConnection(other.transform, Connection.West);
                changed = true;
            }
            if (changed)
                activeNode.UpdateConnectors(true);
        }

        void RemoveActiveNode()
        {
            _target.levels.Remove(activeNode);
            activeNode.Remove();
            DestroyImmediate(activeNode.gameObject);
        }

        void MoveSelection()
        {
            Event e = Event.current;
            if (e.type != EventType.KeyDown)
                return;
            if (e.keyCode == KeyCode.Keypad8)
                Move(Connection.North);
            else if (e.keyCode == KeyCode.Keypad6)
                Move(Connection.East);
            else if (e.keyCode == KeyCode.Keypad2)
                Move(Connection.South);
            else if (e.keyCode == KeyCode.Keypad4)
                Move(Connection.West);
        }

        void Move(Connection dir)
        {
            var next = activeNode.GetConnection(dir);
            if (next == null)
                return;
            activeNode = next.GetComponent<LevelNode>();
        }

        private void OnSceneGUI()
        {
            foreach (var node in _target.levels)
                Handles.Label(node.Position, node.name);
            if (activeNode == null)
                return;
            activeNode.Position = Handles.PositionHandle(activeNode.Position, activeNode.transform.rotation);
        }
    }
}
#endif