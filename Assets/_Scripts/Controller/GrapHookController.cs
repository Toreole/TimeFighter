using UnityEngine;
using System.Collections;

namespace Game.Controller
{
    public class GrapHookController : GenericHook 
    {
        [Header("Grapling Hook Fields")]
        [SerializeField]
        protected float pullStrength;

        protected override IEnumerator DoHook()
        {
            CanPerform = false;
            IsPerforming = true;
            //1. try to find location to hook to
            if (!TestForTarget(out IHookable hookable))
            {
                CanPerform = true;
                IsPerforming = false;
                yield break;
            }

            //2. fire hook
            ropeRenderer.gameObject.SetActive(true);
            yield return ShootHook();

            //3. Do the hooking
            while (ShouldPerform)
            {
                if (Vector2.Distance(hookHit, entity.Position) > maxDistance)
                {
                    BreakChain();
                    yield return DoCooldown();
                    yield break;
                }
                entity.Body.AddForce((hookHit - entity.Position).normalized * pullStrength);
                UpdateChain();
                yield return null;
            }
            BreakChain();
            yield return DoCooldown();
        }
    }
}