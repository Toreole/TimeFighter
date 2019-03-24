using UnityEngine;
using System.Collections.Generic;

namespace Game.Menu
{
    [System.Serializable]
    public class LevelData
    {
        [SerializeField]
        internal LevelInfo levelInfo;
        [SerializeField]
        internal bool isUnlocked = false;
        [SerializeField]
        internal bool isCompleted = false;

        internal string SceneName { get => (levelInfo == null)? "" : levelInfo.SceneName; }
    }
}