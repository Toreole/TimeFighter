using UnityEngine;
using UnityEditor;
using Game;
using System.Collections.Generic;

namespace GameEditor
{
    [CustomEditor(typeof(TargetDummy))]
    public class TargetDummyEditor : Editor
    {
        Quaternion rotation; 
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var dummy = (TargetDummy)target;
            rotation = dummy.transform.rotation;

            if (dummy.Settings == null)
                return;

            if (dummy.Settings.Movement == MovementPattern.Railed)
            {

                if (GUILayout.Button("Add RailSystem Segment"))
                {
                    dummy.RailSystem.Add(new Vector3(0, 0, 0));
                }
                if (GUILayout.Button("Remove RailSystem Segment"))
                {
                    dummy.RailSystem.RemoveAt(dummy.RailSystem.Count - 1);
                }
            }
        }

        protected void OnSceneGUI()
        {
            var dummy = (TargetDummy)target;

            if (dummy.Settings == null)
                return;

            if (dummy.Settings.Movement == MovementPattern.Railed)
            {
                //RailSystemGUI(dummy);
                var system = dummy.RailSystem;
                Handles.color = Color.black;
                Handles.Label(system[0], "Start");
                Handles.color = Color.red;
                for (int i = 0; i < system.Count; i++)
                {
                    system[i] = Handles.PositionHandle(system[i], rotation);

                    //Draw the connecting lines
                    if (i + 1 < system.Count)
                    {
                        Handles.DrawLine(system[i], system[i + 1]);
                    }
                    //Close the loop if needed
                    else if (dummy.Looping)
                    {
                        Handles.DrawLine(system[system.Count - 1], system[0]);
                    }
                }
            }
        }
    }
}