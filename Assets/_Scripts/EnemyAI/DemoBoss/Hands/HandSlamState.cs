using System.Collections.Generic;
using UnityEngine;

namespace Game.Demo.Boss
{
    ///<summary>Slams down until static collision is hit. Kills all vulnerable entities on the way.</summary>
    public class HandSlamState : HandBehaviourState
    {
        List<Entity> slammedEntities = new List<Entity>();

        float speed;
        public override void Enter(BossHand o)
        {
            o.ActivityStatus = HandState.Attacking;
            //o.SetActiveCollision(true);
            speed = o.slamSpeed;
        }

        public override void OnCollisionEnter(Collision2D collision, BossHand hand)
        {
            //checking whether we collided with an entity. Entities usually have colliders in the first child so based on that we're checking a parent.
            Entity entity = collision.collider.GetComponentInParent<Entity>(); //--NOTE: Should theoretically get all DAMAGABLES instead
            if (entity)
            {
                Debug.Log("Slam found entity", entity);
                if (entity.IsInvincible)
                {   //ignore invincible entities!!!
                    hand.IgnoreCollisionWith(entity.Collider);
                    return;
                }
                else
                {
                    //if the collision normal is facing upwards (its from the object hit towards the hands collider)
                    if (collision.GetContact(0).normal.y > 0.6f)
                    {
                        //stun the entity for 1000 seconds (thats enough)
                        entity.Stun(1000f, true);
                        //add the entity to the list of slammed entities that will be killed at the end of the slam.
                        slammedEntities.Add(entity);
                        //now just ignore the collision between these objects for now.
                        hand.IgnoreCollisionWith(entity.Collider);
                        //use a joint to move the entity along with this hand.
                        var joint = hand.gameObject.AddComponent<RelativeJoint2D>();
                        joint.connectedBody = entity.Body;
                    }
                }
            }
            else if (collision.gameObject.isStatic)
            {
                //Powerful slam hit the ground, shake the camera to show the power of this powerful power boi
                Game.Controller.CameraController.Shake(1f);
                //static collision hit!!!
                foreach (var ent in slammedEntities)
                    ent.Damage(999999f);
                foreach (var joint in hand.GetComponents<RelativeJoint2D>())
                    Object.Destroy(joint);
                hand.TransitionToState(new HandWaitQueue(2f * hand.speedMultiplier, new HandPostSlamRise()));
            }
        }

        public override void Update(BossHand hand, float speedMultiplier)
        {
            speed -= Physics2D.gravity.y * Time.deltaTime * 2f;
            var body = hand.Body;                            //speeds are already multiplied with the multi
            body.MovePosition(body.position + new Vector2(0, -speed * Time.deltaTime)); //!!!!! yucky!! rigidbody should only be moved in FixedUpdate!
        }
    }
}