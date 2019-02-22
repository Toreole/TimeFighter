using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game;

namespace GameEditor
{
    /// <summary>
    /// simple editor for enemy settings
    /// </summary>
    [CustomEditor(typeof(EnemySettings))]
    public class EnemySettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            //"initialize" the settings
            EnemySettings settings = (EnemySettings)target;
            EditorUtility.SetDirty(settings);

            //basic settings
            settings.HP = EditorGUILayout.IntField("Max Health", settings.HP);
            settings.TimeGain = EditorGUILayout.FloatField("Time Gain", settings.TimeGain);
            settings.TurnSpeed = EditorGUILayout.FloatField("Turn Speed", settings.TurnSpeed);

            //movement settings (inside a box)
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            settings.Movement = (MovementPattern) EditorGUILayout.EnumPopup("Movement Pattern", settings.Movement);
            if (settings.Movement != MovementPattern.Stationary)
            {
                if(settings.Movement == MovementPattern.Teleportation)
                    settings.MovementSpeed = EditorGUILayout.FloatField("Teleport Cooldown", settings.MovementSpeed);
                else
                    settings.MovementSpeed = EditorGUILayout.FloatField("Movement Speed", settings.MovementSpeed);
                if (settings.Movement == MovementPattern.ShortDistance)
                    settings.WanderDistance = EditorGUILayout.FloatField("Wander Distance", settings.WanderDistance);
            }
            EditorGUILayout.EndVertical();
        }
    }
}