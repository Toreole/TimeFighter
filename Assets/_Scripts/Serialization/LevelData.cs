using System;
using System.Collections.Generic;

namespace Game.Serialization
{
    [Serializable]
    public class LevelData
    {
        public string levelID;
        public List<ObjectData> objectData;
    }
}
