using System;
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
        [Header("Common Entity")]
        [SerializeField]
        protected Rigidbody2D body;
        [SerializeField]
        protected float maxHealth;
        [SerializeField]
        protected float currentHealth;
        [SerializeField]
        protected bool isInvincible = false;
        [Header("Physics")]
        [SerializeField]
        protected new Collider2D collider;

        public Collider2D Collider => collider;

        public float Health { get => currentHealth; }
        public float MaxHealth => maxHealth;
        public bool IsInvincible => isInvincible;
        public bool IsDead => currentHealth < 0f;
        public Rigidbody2D Body => body;
        public Vector2 Position => transform.position;

        public abstract void Damage(float amount);
        public abstract void Stun(float time, bool cancelMovement);
    }
}
