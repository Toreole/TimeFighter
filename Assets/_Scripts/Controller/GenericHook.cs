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

        protected SpriteRenderer ropeRenderer;
        protected Vector2 hookHit = Vector2.positiveInfinity;
        protected bool hooking = false;

        //Properties
        public new bool IsPerforming { get => hooking; protected set => hooking = value; }

        protected virtual void Start()
        {
            if (entity == null)
                entity = GetComponent<Entity>();
            if(swingVisualPrefab != null)
                ropeRenderer = Instantiate(swingVisualPrefab).GetComponent<SpriteRenderer>();
        }

        //STOP RIGHT THERE
        public override void CancelAction()
        {
            StopAllCoroutines();
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