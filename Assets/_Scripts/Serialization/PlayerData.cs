using System;
using UnityEngine;
using System.Collections.Generic;

namespace Game.Serialization
{
    public class PlayerData
    {
        //simple information.
        public string currentLevel;
        public Float3 position;
        //inventory, gamestate, "objectives", etc.
        //public SerializeInventory inventory;
        //public GameState state;
        //public SerializeObjectives objectives;
    }

    /// <summary>
    /// Helper to serialize Vector3
    /// </summary>
    [Serializable]
    public struct Float3
    {
        public float x, y, z;
        public Float3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Float3(float x, float y)
        {
            this.x = x;
            this.y = y;
            this.z = 0;
        }
        public Float3(Vector3 pos) 
        {
            this.x = pos.x;
            this.y = pos.y;
            this.z = pos.z;
        }
        public static implicit operator Float3(Vector3 pos) => new Float3(pos);
        public static implicit operator Vector3(Float3 pos) => new Vector3(pos.x, pos.y, pos.z);
    }
}
