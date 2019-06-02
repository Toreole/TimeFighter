using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game.Controller
{
    public class GrapHookController : GenericHook 
    {
        [Header("Grapling Hook Fields")]
        [SerializeField]
        protected float pullSpeed = 5f;
        [SerializeField]
        protected float gravity = -9.81f;
        [SerializeField]
        protected float minSquareDist = 3f;
        [SerializeField]
        protected CircleCollider2D triggerCollider;
        [Header("Extra Display")]
        [SerializeField]
        protected GameObject displayPrefab;
        protected GameObject activeDisplay;

        protected Vector2 hookOffset;
        protected List<IHookable> hookables = new List<IHookable>();
        protected Transform preferredTarget;

#if UNITY_EDITOR
        private void Update()
        {
            triggerCollider.radius = maxDistance;
        }
#endif

        //This basically acts as the Start() so instantiate the prefab in here
        public override void ClaimOwnership(Entity target)
        {
            base.ClaimOwnership(target);

            activeDisplay = Instantiate(displayPrefab);
            activeDisplay.SetActive(false);
        }

        //
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.CompareTag(targetTag))
            {
                //Debug.Log("found hookable");
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
            preferredTarget = null;
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
            {
                activeDisplay.SetActive(false);
                return;
            }

            activeDisplay.SetActive(true);
            activeDisplay.transform.position = preferredTarget.position;
        }

        /// <summary>
        /// Do Hook yay
        /// </summary>
        //TODO: this handles everything as static anchor right now, pulling movable objects should be implemented soon (tm)
        protected override IEnumerator DoHook()
        {
            if (!preferredTarget)
            {
                yield break;
            }
            //yikes
            var sqrDistance = Vector2.SqrMagnitude((Vector2)preferredTarget.position - entity.Position);
            if (sqrDistance < minSquareDist || sqrDistance > maxDistance*maxDistance)
                yield break;
            CanPerform   = false;
            IsPerforming = true;

            //hook "animation"
            ropeRenderer.gameObject.SetActive(true);
            yield return ShootHook(preferredTarget.position);

            AddHookForce();

            for(float t = 0f; t < 0.1f; t += Time.deltaTime)
            {
                UpdateChain(preferredTarget.position);
                yield return null;
            }

            BreakChain();
            yield return DoCooldown();
        }

        /// <summary>
        /// Add the hook force needed to get the player to the needed position
        /// </summary>
        //TODO: vary speed based on distance? (min, max, lerp)
        void AddHookForce()
        {
            //distance 
            var ds = (Vector2)preferredTarget.position - entity.Position;
            //required time for the diagonal
            var dt = ds.magnitude / pullSpeed;

            //linear yVelocity
            var vy = ds.y / dt;
            //inverse square yVelocity for curve
            var vy0 = 0.5f * gravity * dt;
            //combined velocity on y
            var yVel = vy - vy0;

            //velocity on x
            var xVel = ds.x / dt;

            //apply the velocity to the entity
            entity.Body.velocity = new Vector2(xVel, yVel);
        }
    }
}