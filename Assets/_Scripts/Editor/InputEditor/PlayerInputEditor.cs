using UnityEngine;
using UnityEditor;
using Game.Controller.Input;

namespace Game.Editor
{
    [CustomEditor(typeof(PlayerInput))]
    public class PlayerInputEditor : UnityEditor.Editor
    {
        //simple inspector
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if(GUILayout.Button("Open Input Map Editor"))
            {
                InputMapEditor.OpenWindow();
            }
        }
    }
}