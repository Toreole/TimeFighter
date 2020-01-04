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

    public abstract class ObjectData
    {
        public string objectID;
        Dictionary<string, int> dic;
    }
}
