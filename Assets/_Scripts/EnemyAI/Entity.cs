﻿using System;
using System.Collections.Generic;
using Game.Controller;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// The base class for all moving entities
    /// </summary>
    public abstract class Entity : MonoBehaviour, IDamageable, IPhysicsObject
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
        
        protected float currentHealth;
        protected bool isInvincible = false;

        internal float Health { get => currentHealth; set => currentHealth = value; }
        public bool IsPlayer { get; protected set; } = false;
        public Rigidbody2D Body => body;
        public Vector2 Position => transform.position;
        public bool IsInvincible => isInvincible;
        public Vector2 LookDirection { get; protected set; } = Vector2.right;
        public abstract bool IsGrounded { get; }

        internal void AddAction(BaseAction action) { if(!actions.Exists(x => x == action)) actions.Add(action); }
        internal void RemoveAction(BaseAction action) { if (actions.Exists(x => x == action)) actions.Remove(action); }

        protected virtual void Start()
        {
            actions = new List<BaseAction>(GetComponents<BaseAction>());
            if (actions.Count > 0)
                foreach (var ac in actions)
                    ac.ClaimOwnership(this);
        }
    }
}
