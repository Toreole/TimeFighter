using UnityEngine;
using System.Collections;
using Game.Controller;

namespace Game
{
    public class DraggableItem : MonoBehaviour, IHookable
    {
        [SerializeField]
        protected Rigidbody2D body;
        [SerializeField]
        protected HookInteraction interaction;

        public bool CanBeDragged => true;
        public Rigidbody2D Body => body;

        public HookInteraction HookInteract => interaction;

        public Transform M_Transform => transform;

        public Vector2 Position => transform.position;

        public bool HasFlag(HookInteraction interact)
        {
            return this.interaction.HasFlag(interact);
        }
    }
}