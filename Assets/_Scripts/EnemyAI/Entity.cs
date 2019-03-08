using System;
using System.Collections.Generic;
using Game.Controller;
using UnityEngine;

namespace Game
{
    public abstract class Entity : MonoBehaviour
    {
        internal abstract void ProcessHit(AttackHitData data);
        internal abstract void ProcessHit(AttackHitData data, bool onlyDamage);
        internal abstract void ResetEntity();

        internal float Health { get; set; }
    }
}
