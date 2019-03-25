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
            RaycastHit2D hit;
            if (hit = Physics2D.Raycast(entity.Position, DirToMouse, maxDistance, targetLayer))
            {
                hookHit = hit.point;
                if (!hit.collider.CompareTag(targetTag))
                {
                    //Not the correct tag.
                    IsPerforming = false;
                    CanPerform = true;
                    yield break;
                }
            }
            else
            {
                //the raycast doesnt hit anything
                IsPerforming = false;
                CanPerform = true;
                yield break; //STOP, THIS VIOLATES THE LAW
            }
            //2. fire hook
            IsPerforming = true;
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
                //TODO: optional: 1. break the hook when something gets in the way,
                //TODO:           2. Jump from the hook and break it
                //TODO:           3. Move up and down the hook while possible (using yMove)
                UpdateChain();
                yield return null;
            }
            BreakChain();
            yield return DoCooldown();
        }
    }
}