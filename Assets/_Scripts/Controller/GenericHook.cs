using UnityEngine;
using System.Collections;

namespace Game.Controller
{
    public abstract class GenericHook : BaseAction
    {
        [Header("Generic Hook Fields")]
        [SerializeField]
        protected GameObject swingVisualPrefab;
        [SerializeField]
        protected float maxDistance;
        [SerializeField]
        protected float tossSpeed;
        [SerializeField]
        protected LayerMask targetLayer;
        [SerializeField]
        protected string targetTag;
        [SerializeField, Tooltip("Grapling or Swing?")]
        protected HookInteraction hookType;

        protected SpriteRenderer ropeRenderer;
        protected Vector2 hookHit = Vector2.positiveInfinity;
        
        public override void ClaimOwnership(Entity entity)
        {
            base.ClaimOwnership(entity);
            if(swingVisualPrefab != null)
                ropeRenderer = Instantiate(swingVisualPrefab).GetComponent<SpriteRenderer>();
        }

        //STOP RIGHT THERE
        public override void CancelAction()
        {
            StopAllCoroutines();
            BreakChain();
            StartCoroutine(DoCooldown());
        }

        //Perform
        public override void PerformAction()
        {
            StartCoroutine(DoHook());
        }

        protected abstract IEnumerator DoHook();
        protected virtual IEnumerator ShootHook()
        {
            Vector2 ch = hookHit - entity.Position;
            float totalDist = ch.magnitude;
            float reqTime = totalDist / tossSpeed;
            for (float t = 0f; t < 1; t += Time.deltaTime / reqTime)
            {
                //now do the adjusting
                ropeRenderer.transform.position = (Vector2)transform.position + ch * t / 2f;
                ropeRenderer.size = new Vector2(1f, t * totalDist);
                ropeRenderer.transform.rotation = Quaternion.LookRotation(Vector3.forward, -ch.normalized);

                //recalculate this shit every single time ugh
                ch = hookHit - entity.Position;
                totalDist = ch.magnitude;
                reqTime = totalDist / tossSpeed;
                yield return null;
            }
        }

        protected virtual bool TestForTarget(out IHookable hookable)
        {
            RaycastHit2D hit = Physics2D.Raycast(entity.Position, TargetDirection, maxDistance, targetLayer);
            if (!hit)
            {
                hookable = null;
                return false;
            }
            hookHit = hit.point;
            hookable = hit.collider.gameObject.GetComponent<IHookable>();
            if (hookable == null)
                return false;
            return hookable.HookInteract.HasFlag(hookType);
        }

        //Update the hooks size and that.
        protected virtual void UpdateChain()
        {
            var ch = hookHit - entity.Position;
            ropeRenderer.transform.position = Vector2.Lerp(transform.position, hookHit, 0.5f);
            ropeRenderer.size = new Vector2(1f, ch.magnitude);
            ropeRenderer.transform.rotation = Quaternion.LookRotation(Vector3.forward, -ch.normalized);
        }

        protected virtual void BreakChain()
        {
            IsPerforming = false;
            hookHit = Vector2.positiveInfinity;
            ropeRenderer.gameObject.SetActive(false);
        }
    }
}