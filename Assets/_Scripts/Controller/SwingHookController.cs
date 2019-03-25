using System;
using System.Collections;
using UnityEngine;

namespace Game.Controller
{
    /// <summary>
    /// This controlls the swinging-style hook.
    /// </summary>
    public class SwingHookController : BaseAction
    {
        [Header("Swing Hoook Fields")]
        //Serialized fields
        [SerializeField]
        protected GameObject swingVisualPrefab;
        [SerializeField]
        protected GameObject swingAnchorPrefab;
        [SerializeField]
        protected float maxDistance;
        [SerializeField]
        protected float tossSpeed;
        [SerializeField]
        protected LayerMask targetLayer;
        [SerializeField]
        protected string targetTag;

        //Runtime vars
        protected SpriteRenderer ropeRenderer;
        protected DistanceJoint2D joint; //this is both the DistanceJoint2D, aswell as the static Rigidbody/transform end point of the hook.
        protected Vector2 hookHit = Vector2.positiveInfinity;
        protected bool hooking = false;
        protected bool canHookJump = false;

        //Properties
        public new bool IsPerforming { get => hooking; protected set => hooking = value; }

        //Setup if needed
        private void Start()
        {
            if (entity == null)
                entity = GetComponent<Entity>();
            if (swingAnchorPrefab == null)
            {
                var tempGO = new GameObject();
                var tempBody = tempGO.AddComponent<Rigidbody2D>();
                tempBody.bodyType = RigidbodyType2D.Static;
                joint = tempGO.AddComponent<DistanceJoint2D>();
                joint.autoConfigureDistance = false;
                tempGO.SetActive(false);
            }
            else
                joint = Instantiate(swingAnchorPrefab).GetComponent<DistanceJoint2D>();
            joint.connectedBody = entity.Body;
            joint.maxDistanceOnly = true;
            joint.autoConfigureDistance = false;
            joint.enabled = false;
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
        
        //TODO: fix
        /// <summary>
        /// The actual hooking
        /// </summary>
        private IEnumerator DoHook()
        {
            CanPerform = false;
            IsPerforming = true;
            //1. try to find location to hook to
            RaycastHit2D hit;
            if (hit = Physics2D.Raycast(entity.Position, DirToMouse, maxDistance, targetLayer))
            {
                hookHit = hit.point;
            }
            else
            {
                //the raycast doesnt hit anything
                yield return new WaitForSeconds(cooldown);
                IsPerforming = false;
                CanPerform = true;
                yield break; //STOP, THIS VIOLATES THE LAW
            }
            if(!hit.collider.CompareTag(targetTag))
            {
                //Not the correct tag.
                yield return new WaitForSeconds(cooldown);
                IsPerforming = false;
                CanPerform = true;
                yield break;
            }
            //2. fire hook
            IsPerforming = true;
            ropeRenderer.gameObject.SetActive(true);
            yield return ShootHook();

            //3. Do the hooking
            joint.enabled = true;
            joint.transform.position = hit.point;
            joint.distance = Vector2.Distance(hit.point, entity.Position);
            while (ShouldPerform)
            {
                if (Vector2.Distance(hookHit, entity.Position) > maxDistance)
                {
                    CanPerform = false;
                    joint.enabled = false;
                    BreakChain();
                    yield break;
                }
                //TODO: optional: 1. break the hook when something gets in the way,
                //TODO:           2. Jump from the hook and break it
                //TODO:           3. Move up and down the hook while possible (using yMove)
                UpdateChain();
                yield return null;
            }
            joint.enabled = false;
            IsPerforming = false;
            CanPerform = true;
            BreakChain();
        }

        //TODO: this is fucking broken, i dont know how but it just fucks up.
        private IEnumerator ShootHook()
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
        private void UpdateChain()
        {
            var ch = hookHit - entity.Position;
            ropeRenderer.transform.position = Vector2.Lerp(transform.position, hookHit, 0.5f);
            ropeRenderer.size = new Vector2(1f, ch.magnitude);
            ropeRenderer.transform.rotation = Quaternion.LookRotation(Vector3.forward, -ch.normalized);
        }

        private void BreakChain()
        {
            IsPerforming = false;
            hookHit = Vector2.positiveInfinity;
            hooking = false;
            ropeRenderer.gameObject.SetActive(false);
        }

    }
}
