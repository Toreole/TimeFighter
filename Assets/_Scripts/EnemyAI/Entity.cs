using System;
using System.Collections.Generic;
using Game.Controller;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// The base class for all moving entities
    /// </summary>
    public abstract class Entity : MonoBehaviour, IDamageable
    {
        public abstract void ProcessHit(AttackHitData data);
        public abstract void ProcessHit(AttackHitData data, bool onlyDamage);
        internal abstract void ResetEntity();

        [SerializeField, Tooltip("The Rigidbody component, will be automatically set in Start()")]
        protected Rigidbody2D body;

        internal float Health { get; set; }
        public Rigidbody2D Body => body;
    }
}
