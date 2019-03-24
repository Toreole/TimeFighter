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

        //Runtime vars
        protected SpriteRenderer ropeRenderer;
        protected DistanceJoint2D joint; //this is both the DistanceJoint2D, aswell as the static Rigidbody/transform end point of the hook.
        protected Vector2 hookHit = Vector2.positiveInfinity;
        protected bool hooking = false;
        protected bool canHookJump = false;

        //Properties
        public new bool IsPerforming { get => hooking; protected set => hooking = value; }

        public override void CancelAction()
        {
            throw new NotImplementedException();
        }

        public override void PerformAction()
        {
            throw new NotImplementedException();
        }
        
        //TODO: fix
        /// <summary>
        /// The actual hooking
        /// </summary>
        /// <returns>coroutine stuffs</returns>
        private IEnumerator DoHook()
        {
            hooking = true;
            //1. try to find location to hook to
            RaycastHit2D hit;
            if (hit = Physics2D.Raycast(transform.position, DirToMouse, maxDistance, targetLayer))
            {
                hookHit = hit.point;
            }
            else
            {
                yield return new WaitForSeconds(0.2f);
                hooking = false;
                yield break; //STOP, THIS VIOLATES THE LAW
            }
            //2. fire hook
            ropeRenderer.gameObject.SetActive(true);
            yield return ShootHook();
            //3. Do the hooking
            joint.enabled = true;
            joint.transform.position = hit.point;
            while (IsPerforming)
            {
                if (Vector2.Distance(hookHit, transform.position) > maxDistance)
                {
                    joint.enabled = false;
                    BreakChain();
                    yield break;
                }
                //TODO: maybe make it momentum based whether you can jump or not. this is a temporary fix tho.
                if (Input.GetButtonDown("Jump") && canHookJump)
                {
                    canHookJump = false;
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
            BreakChain();
        }

        //TODO: this is fucking broken, i dont know how but it just fucks up.
        private IEnumerator ShootHook()
        {
            Vector2 ch = hookHit - (Vector2)transform.position;
            float totalDist = ch.magnitude;
            float reqTime = totalDist / tossSpeed;
            for (float t = 0f; t < 1; t += Time.deltaTime / reqTime)
            {
                //now do the adjusting
                ropeRenderer.transform.position = (Vector2)transform.position + ch * t / 2f;
                ropeRenderer.size = new Vector2(1f, t * totalDist);
                ropeRenderer.transform.rotation = Quaternion.LookRotation(Vector3.forward, -ch.normalized);

                //recalculate this shit every single time ugh
                ch = hookHit - (Vector2)transform.position;
                totalDist = ch.magnitude;
                reqTime = totalDist / tossSpeed;
                yield return null;
            }
        }

        //Update the hooks size and that.
        private void UpdateChain()
        {
            var ch = hookHit - (Vector2)transform.position;
            ropeRenderer.transform.position = Vector2.Lerp(transform.position, hookHit, 0.5f);
            ropeRenderer.size = new Vector2(1f, ch.magnitude);
            ropeRenderer.transform.rotation = Quaternion.LookRotation(Vector3.forward, -ch.normalized);
        }

        private void BreakChain()
        {
            hookHit = Vector2.positiveInfinity;
            hooking = false;
            ropeRenderer.gameObject.SetActive(false);
        }

    }
}
