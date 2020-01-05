using System;
using System.Collections.Generic;

namespace Game.Serialization
{
    [Serializable]
    internal class SaveData
    {
        public PlayerData playerData;
        public List<LevelData> levelData;

        public SaveData()
        {
            playerData = new PlayerData();
            levelData = new List<LevelData>();
        }
    }
}
