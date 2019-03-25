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

        protected abstract void OnLevelStart();
        protected abstract void OnLevelFail();
        protected abstract void OnLevelComplete();

        [Header("Common Entity")]
        [SerializeField]
        protected bool active = false;
        [SerializeField, Tooltip("The Rigidbody component, will be automatically set in Start()")]
        protected Rigidbody2D body;
        [SerializeField, Tooltip("The actions this entity can perform")]
        protected List<BaseAction> actions;
        
        protected internal float currentHealth;
        internal float Health { get => currentHealth; set => currentHealth = value; }
        public Rigidbody2D Body => body;
        public Vector2 Position => transform.position;

        internal void AddAction(BaseAction action) { if(!actions.Exists(x => action)) actions.Add(action); }
        protected virtual void Start()
        {
            actions = new List<BaseAction>(GetComponents<BaseAction>());
            if (actions.Count > 0)
                foreach (var ac in actions)
                    ac.ClaimOwnership(this);
        }
    }
}
