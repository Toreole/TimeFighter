using UnityEngine;
using System.Collections.Generic;

namespace Game.Menu
{
    [System.Serializable]
    public class LevelData
    {
        [SerializeField]
        internal string targetScene;
        [SerializeField]
        internal bool isUnlocked = false;
        [SerializeField]
        internal bool isCompleted = false;
        [SerializeField]
        internal string uselessText = "ur mum gae";
    }
}