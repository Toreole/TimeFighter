using System;
using System.Collections.Generic;
using UnityEngine;
using Game;

namespace Game.Controller
{
    /// <summary>
    /// most basic hook target.
    /// </summary>
    public class HookAnchor : MonoBehaviour, IHookable
    {
        [SerializeField]
        protected HookInteraction hookInteraction = HookInteraction.AnyStatic;

        public HookInteraction HookInteract => hookInteraction;
        public Transform M_Transform { get => transform; }
        public bool CanBeDragged { get; }
        public Rigidbody2D Body { get; }
        public Vector2 Position { get => transform.position; }

        public bool HasFlag(HookInteraction interaction)
        {
            return hookInteraction.HasFlag(interaction);
        }
    }
}
