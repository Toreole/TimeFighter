using UnityEngine;
using System.Collections.Generic;

namespace Game.Menu
{
    /// <summary>
    /// The visible map in the "menu"
    /// </summary>
    public class LevelSystem : MonoBehaviour
    {
        [SerializeField]
        internal List<LevelNode> levels;
        [SerializeField]
        protected GameObject levelNodePrefab;

        [SerializeField]
        protected Transform player;

        protected Transform currentLevel;
     
        internal void CreateNewLevel()
        {
            var temp = Instantiate(levelNodePrefab, this.transform);
            levels.Add(temp.GetComponent<LevelNode>());
        }

    }
}