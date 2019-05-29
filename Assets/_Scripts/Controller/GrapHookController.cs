using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game.Controller
{
    public class GrapHookController : GenericHook 
    {
        [Header("Grapling Hook Fields")]
        [SerializeField]
        protected float yMaxTime = 0.6f;
        [SerializeField]
        protected float xPullSpeed = 5f;
        [SerializeField]
        protected float gravity = -9.81f;
        [SerializeField]
        protected float minSquareDist = 1f;
        [SerializeField]
        protected CircleCollider2D triggerCollider;
        [Header("Extra Display")]
        [SerializeField]
        protected GameObject displayPrefab;
        protected GameObject activeDisplay;

        protected Vector2 hookOffset;
        protected List<IHookable> hookables = new List<IHookable>();
        protected Transform preferredTarget;

        //This basically acts as the Start() so instantiate the prefab in here
        public override void ClaimOwnership(Entity target)
        {
            base.ClaimOwnership(target);

            activeDisplay = Instantiate(displayPrefab);
            activeDisplay.SetActive(false);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.CompareTag(targetTag))
            {
                Debug.Log("found hookable");
                var temp = collision.GetComponent<IHookable>();
                if (temp != null)
                    if(temp.HasFlag(this.hookType))
                        hookables.Add(temp);
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag(targetTag))
            {
                var temp = collision.GetComponent<IHookable>();
                if (temp != null)
                    hookables.Remove(temp);
            }
        }

        //draw the currently optimal target and display it i guess
        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!IsSelected || hookables.Count == 0)
            {
                activeDisplay.SetActive(false);
                return;
            }
            FindPreferredTarget();
        }

        /// <summary>
        /// Find the preferred hook target, then display it.
        /// </summary>
        private void FindPreferredTarget()
        {
            //! TargetDirection
            float lastDot = -1000f;
            Vector2 dist;
            foreach(var hookable in hookables)
            {
                var offset = hookable.Position - entity.Position;
                dist = (hookable.Position - entity.Position).normalized;
                var dot = Vector2.Dot(TargetDirection, dist);
                if(dot > lastDot && offset.sqrMagnitude >= minSquareDist)
                {
                    preferredTarget = hookable.M_Transform;
                    lastDot = dot;
                }
            }
            if (preferredTarget == null)
                return;

            activeDisplay.SetActive(true);
            activeDisplay.transform.position = preferredTarget.position;
        }

        /// <summary>
        /// Do Hook yay
        /// </summary>
        /// <returns></returns>
        protected override IEnumerator DoHook()
        {
            if (!preferredTarget)
            {
                yield break;
            }
            CanPerform   = false;
            IsPerforming = true;
            
            //hook "animation"
            yield return ShootHook(preferredTarget.position);

            AddHookForce();

            CanPerform   = true;
            IsPerforming = false;
            BreakChain();
            yield return DoCooldown();
        }

        /// <summary>
        /// Add the hook force needed to get the player to the needed position
        /// </summary>
        void AddHookForce()
        {
            //TODO: y fling is now too much for some reason
            var xDist = preferredTarget.position.x - entity.Position.x;
            var tEnd = Mathf.Abs(xDist / xPullSpeed);
                                   //yMaxTime * tEnd * -gravity;
            var antiGravitySpeed = yMaxTime * tEnd * -gravity;
            var yOffsetSpeed = (preferredTarget.position.y - entity.Position.y) / tEnd;
            var ySpeed = antiGravitySpeed + yOffsetSpeed;

            entity.Body.velocity = new Vector2(xPullSpeed * Util.Normalized(xDist), ySpeed);
        }
    }
}