using System;
using System.Collections;
using UnityEngine;

namespace Game.Controller
{
    /// <summary>
    /// This controlls the swinging-style hook.
    /// </summary>
    public class SwingHookController : GenericHook
    {
        [Header("Swing Hoook Fields")]
        [SerializeField]
        protected GameObject swingAnchorPrefab;
        
        //Runtime vars
        protected DistanceJoint2D joint; //this is both the DistanceJoint2D, aswell as the static Rigidbody/transform end point of the hook.
        
        //Setup if needed
        protected override void Start()
        {
            base.Start();
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
        }
        
        //TODO: fix
        /// <summary>
        /// The actual hooking
        /// </summary>
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
            joint.enabled = true;
            joint.transform.position = hit.point;
            joint.distance = Vector2.Distance(hit.point, entity.Position);
            while (ShouldPerform)
            {
                if (Vector2.Distance(hookHit, entity.Position) > maxDistance)
                {
                    BreakChain();
                    yield return DoCooldown();
                    yield break;
                }
                //TODO: optional: 1. break the hook when something gets in the way,
                //TODO:           2. Jump from the hook and break it
                //TODO:           3. Move up and down the hook while possible (using yMove)
                UpdateChain();
                yield return null;
            }
            BreakChain();
            yield return DoCooldown();
        }

        protected override void UpdateChain()
        {
            base.UpdateChain();
        }

        protected override void BreakChain()
        {
            base.BreakChain();
            joint.enabled = false;
        }
    }
}
