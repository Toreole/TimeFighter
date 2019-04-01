using UnityEngine;
using System.Collections;

namespace Game.Controller
{
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
            Hook hook = StaticHook; //static by default.
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
                yield return null;
            }
            BreakChain();
            yield return DoCooldown();
        }

        private void StaticHook()
        {
            entity.Body.AddForce((hookHit - entity.Position).normalized * pullStrength);
            if (Vector2.Distance(hookHit, entity.Position) > maxDistance)
            {
                CancelAction();
            }
        }

        void DynamicHook()
        {
            hookHit = hookable.Position + hookOffset;
            entity.Body.AddForce((hookHit - entity.Position).normalized * pullStrength);
            if (Vector2.Distance(hookHit, entity.Position) > maxDistance)
            {
                CancelAction();
            }
        }

        void DragOther()
        {
            //TODO: this
        }
    }
}