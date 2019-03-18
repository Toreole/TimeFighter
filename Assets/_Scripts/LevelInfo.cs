using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu]
    public class LevelInfo : ScriptableObject
    {
        [SerializeField]
        protected string descriptiveName;
        [SerializeField]
        protected     string sceneName;
        [SerializeField]
        protected float time;
        [SerializeField]
        protected string additionalInfo;

        public string DescriptiveName { get => descriptiveName; }
        public string SceneName { get => sceneName; }
        public float LevelTime { get => time; }
        public string AdditionalInfo { get => additionalInfo; }
    }
}
