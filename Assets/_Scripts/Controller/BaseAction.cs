using System;
using System.Collections;
using UnityEngine;
using Game;

namespace Game.Controller
{
    /// <summary>
    /// The base class for all actions.
    /// </summary>
    public abstract class BaseAction : MonoBehaviour
    {
        [Header("Base Action Fields")]
        [SerializeField, Range(0.0f, 512), Tooltip("The cooldown for this action in seconds.")]
        protected float cooldown;
        [SerializeField, Tooltip("The Entity that performs this action.")]
        protected Entity entity;
        [SerializeField, Tooltip("The UI image to display this as")]
        protected Sprite uiSprite;

        public bool HasOwner { get; protected set; } = false;
        public bool IsPerforming { get; protected set; } = false;
        public bool CanPerform   { get; protected set; } = true;
        public bool ShouldPerform { get; set; } = false;
        public float RemainingCooldown { get; protected set; } = 0f;
        public Sprite UISprite => uiSprite;
        protected Vector2 DirToMouse => (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;

        public abstract void CancelAction();

        public abstract void PerformAction();
        
        public void ClaimOwnership(Entity owner)
        {
            if (HasOwner)
                return;
            HasOwner = true;
            entity = owner;
        }

        protected virtual IEnumerator DoCooldown()
        {
            RemainingCooldown = cooldown;
            for(float t = 0; t <= cooldown; t += Time.deltaTime)
            {
                RemainingCooldown = cooldown - t;
                yield return null;
            }
            RemainingCooldown = 0f;
            CanPerform = true;
        }
    }
}