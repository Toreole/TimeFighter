using UnityEngine;
using System.Collections;

namespace Game.Controller
{
    [System.Obsolete("bruuuuuh")]
    public class GrapHookController : GenericHook 
    {
        [Header("Grapling Hook Fields")]
        [SerializeField]
        protected float pullStrength;

        protected Vector2 hookOffset;
        protected IHookable hookable;

        delegate void Hook();

        protected override IEnumerator DoHook()
        {
            CanPerform = false;
            IsPerforming = true;
            //1. try to find location to hook to
            if (!TestForTarget(out hookable))
            {
                CanPerform = true;
                IsPerforming = false;
                yield break;
            }

            //2. fire hook
            ropeRenderer.gameObject.SetActive(true);
            yield return ShootHook();
            hookOffset = hookHit - hookable.Position;
            //Set up the hook loop
            Hook hook = HookForce; //static by default.
            if (hookable.HasFlag(HookInteraction.DynamicBody))
            {
                //is lightweight and should be dragged
                if (hookable.HasFlag(HookInteraction.Draggable))
                    hook = DragOther;
                else //is a moving body
                    hook = DynamicHook;
            }

            //3. Do the hooking
            while (ShouldPerform)
            {
                UpdateChain();
                hook?.Invoke();
                if (Vector2.Distance(hookHit, entity.Position) > maxDistance)
                {
                    CancelAction();
                }
                yield return null;
            }
            BreakChain();
            yield return DoCooldown();
        }

        /// <summary>
        /// default hooking behaviour.
        /// </summary>
        private void HookForce()
        {
            entity.Body.AddForce((hookHit - entity.Position).normalized * pullStrength);
        }

        void DynamicHook()
        {
            hookHit = hookable.Position + hookOffset;
            HookForce();
        }

        void DragOther()
        {
            //TODO: this is shit again lmao should NOT cause the player to fly xD
            hookHit = hookable.Position + hookOffset;
            hookable.Body.AddForce((entity.Position - hookHit).normalized * pullStrength);
            if (hookable is null)
            {
                CancelAction();
            }
        }
    }
}