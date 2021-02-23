using System.Collections.Generic;
using UnityEngine;

namespace Game.Demo.Boss
{
    ///<summary>Claps the hands together.</summary>
    public class HandClapState : HandBehaviourState
    {
        float movementDirection;
        List<Entity> clappedEntities = new List<Entity>(2);

        public HandClapState(float direction) => movementDirection = direction;

        public override void Enter(BossHand o)
        {
            //Enter might not be needed really.
        }

        public override void OnCollisionEnter(Collision2D collision, BossHand hand)
        {
            if (collision.gameObject.isStatic)
                return; //static collisions are irrelevant.
            Entity entity = collision.collider.GetComponentInParent<Entity>();
            BossHand otherHand = collision.gameObject.GetComponent<BossHand>();
            if (entity)
            {
                Debug.Log("test");
                //Ignore collisions with invincible entities.
                if (entity.IsInvincible)
                {
                    hand.IgnoreCollisionWith(entity.Collider);
                }
                else
                {
                    float collisionNormalX = collision.GetContact(0).normal.x;
                    //if the collisions normal is roughly pointing in the same direction as where the hand is going:
                    if (collisionNormalX > 0.5f && movementDirection < 0f || collisionNormalX < -0.5f && movementDirection > 0f)
                    {
                        //stun the entity for 1000 seconds (thats enough)
                        entity.Stun(1000f, true);
                        //add the entity to the list of clapped entities that will be killed at the end of the clap.
                        clappedEntities.Add(entity);
                        //in here: disable the collider.
                        entity.Collider.enabled = false;
                        //use a joint to move the entity along with this hand.
                        var joint = hand.gameObject.AddComponent<RelativeJoint2D>();
                        joint.connectedBody = entity.Body;
                        joint.breakForce = Mathf.Infinity; //double check idk.
                    }
                }
            }
            else if (otherHand)
            {
                Game.Controller.CameraController.Shake(1.0f);
                //"kill" all entities inside the clap.
                for (int i = 0; i < clappedEntities.Count; i++)
                {
                    clappedEntities[i].Damage(999999f);
                    clappedEntities[i].Collider.enabled = true; //re-enable the collider we disabled earlier.
                }
                foreach (var joint in hand.GetComponents<RelativeJoint2D>())
                    Object.Destroy(joint);
                //stay in this position for half a second, then return to idle.
                hand.TransitionToState(new HandWaitQueue(0.5f, BossHand.NoControlState));
            }
        }

        //Actually fixedUpdate! this simply moves the hand.
        public override void Update(BossHand o, float speedMultiplier)
        {
            float x = o.slamSpeed * Time.deltaTime * movementDirection;
            o.Body.MovePosition(o.Body.position + new Vector2(x, 0));
        }
    }
}