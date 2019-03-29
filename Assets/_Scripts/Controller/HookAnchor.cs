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
        public Transform M_Transform => this.transform;
        public bool CanBeDragged => false;
        public Rigidbody2D Body => null;
        public Vector2 Position => this.transform.position;

        public bool HasFlag(HookInteraction interaction)
        {
            return hookInteraction.HasFlag(interaction);
        }
    }
}
