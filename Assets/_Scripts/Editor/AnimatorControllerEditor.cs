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
        SerializedProperty entityProperty;
        SerializedProperty animatorProperty;
        List<TParam> links;

        //Setup editor
        private void OnEnable()
        {
            entityProperty = serializedObject.FindProperty("entity");
            animatorProperty = serializedObject.FindProperty("animator");
            links = (target as AnimatorController<TParam, TEnum>).GetParams();
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.ObjectField(entityProperty);
            EditorGUILayout.ObjectField(animatorProperty);

            //draw links:
            foreach(var link in links)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                link.paramName = EditorGUILayout.TextField("Parameter Name", link.paramName);
                link.purpose = (TEnum)EditorGUILayout.EnumPopup("Enum Value", link.purpose); //yes
                EditorGUILayout.EndVertical();
            }
            if(GUILayout.Button("Populate from Enum"))
            {
                foreach(TEnum value in Enum.GetValues(typeof(TEnum)))
                {
                    if(!links.Exists(p => p.purpose.Equals(value)))
                    {
                        var x = new TParam();
                        x.purpose = value;
                        x.paramName = value.ToString(); 
                        links.Add(x); 
                    }
                }
            }
            if(GUILayout.Button("Popupate from Parameters"))
            {
                links.Clear();
                foreach(var param in (animatorProperty.objectReferenceValue as Animator).parameters)
                {
                    var x = new TParam();
                    x.paramName = param.name;
                    x.purpose = default;
                    links.Add(x);
                }
            }
            if (serializedObject.hasModifiedProperties)
                EditorUtility.SetDirty(target);
        }
    }
    [CustomEditor(typeof(PlayerAnimator))]
    public class PlayerAnimEditor : AnimatorControllerEditor<PlayerParameterLink, PlayerAnimation>
    {
          
    }
}
