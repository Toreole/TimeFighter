using System;
using System.Collections.Generic;
using Game.Controller;
using Game.Generics;
using UnityEditor;
using Game.Misc;
using UnityEngine;

namespace GameEditor
{
    public class AnimatorControllerEditor<TParam, TEnum> : Editor where TParam : AnimatorParameterLink<TEnum>, new() where TEnum : Enum
    {
        List<TParam> links;
        new AnimatorController<TParam, TEnum> target;
        //Setup editor
        private void OnEnable()
        {
            target = base.target as AnimatorController<TParam, TEnum>;
            links = (target as AnimatorController<TParam, TEnum>).GetParams();
        }
        public override void OnInspectorGUI()
        {
            target.Entity = (Game.Entity)EditorGUILayout.ObjectField("Entity", target.Entity, typeof(Game.Entity), true);
            GUI.backgroundColor = target.Animator ? Color.white: Color.red;
            target.Animator = (Animator)EditorGUILayout.ObjectField("Animator", target.Animator, typeof(Animator), true);

            if (target.Animator)
            {
                DoListEditor();
                if (GUILayout.Button("Manual Save"))
                    EditorUtility.SetDirty(target);
            }
            else
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Animator needs to be assigned first!");
                EditorGUILayout.EndVertical();
            }
            GUI.backgroundColor = Color.white;
            if (serializedObject.hasModifiedProperties)
                EditorUtility.SetDirty(target);
        }

        void DoListEditor()
        {//draw links:
            foreach (var link in links)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                link.paramName = EditorGUILayout.TextField("Parameter Name", link.paramName);
                link.purpose = (TEnum)EditorGUILayout.EnumPopup("Enum Value", link.purpose); //yes
                EditorGUILayout.EndVertical();
            }
            if (GUILayout.Button("Populate from Enum"))
            {
                links.Clear();
                foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
                {

                    var x = new TParam();
                    x.purpose = value;
                    x.paramName = value.ToString();
                    links.Add(x);
                }
            }
            if (GUILayout.Button("Popupate from Parameters"))
            {
                links.Clear();
                for (int i = 0; i < target.Animator.parameterCount; i++)
                {
                    var param = target.Animator.GetParameter(i);
                    var x = new TParam();
                    x.paramName = param.name;
                    x.purpose = default;
                    links.Add(x);
                }
            }
        }
    }
    [CustomEditor(typeof(PlayerAnimator))]
    public class PlayerAnimEditor : AnimatorControllerEditor<PlayerParameterLink, PlayerAnimation>
    {
          
    }
}
