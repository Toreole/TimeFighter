using System;
using System.Collections.Generic;
using UnityEngine;
using Game;

namespace Game.Controller
{
    /// <summary>
    /// most basic hook target.
    /// </summary>
    [AddComponentMenu("Hookables/Basic Anchor")]
    public class HookAnchor : MonoBehaviour, IHookable
    {
        [SerializeField]
        protected HookInteraction hookInteraction = HookInteraction.AnyStatic;
        [SerializeField]
        protected Transform hookCenter;
        [SerializeField]
        protected bool useCenterPoint = false;

        public HookInteraction HookInteract => hookInteraction;
        public Transform M_Transform { get => transform; }
        public bool CanBeDragged { get; }
        public Rigidbody2D Body { get; }
        public Vector2 Position { get => (useCenterPoint)? hookCenter.position :  transform.position; }

        public bool UseCenterPoint => useCenterPoint;
        public bool IsPlatform => CompareTag("Platform");

        public bool HasFlag(HookInteraction interaction)
        {
            return hookInteraction.HasFlag(interaction);
        }
    }
}
