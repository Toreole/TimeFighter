using UnityEngine;
using Game.Patterns.States;
using System.Collections.Generic;

namespace Game.Demo.Boss
{
    ///<summary>Has to be instanced and given a target. Moves above the target and enters the SlamState.</summary>
    public class HandTrackTargetState : HandBehaviourState
    {
        Entity target;
        float startTime; 
        float groundedY;

        public HandTrackTargetState(Entity target)
        {
            this.target = target;
            groundedY = target.Position.y;
        }

        public override void Enter(BossHand hand)
        {
            hand.ActivityStatus = HandState.Attacking;
            startTime = Time.time;
        }

        //Move above the player.
        public override void Update(BossHand hand, float speedMultiplier)
        {
            //this whole thing isnt perfect in any way, but at least it somewhat works.
            Vector2 targetPosition = target.Position;
            if(target.IsGrounded)
                groundedY = Mathf.Min(target.Position.y, groundedY);
            targetPosition.y = groundedY + 7.0f;
            var body = hand.Body;
            float timeOffset = (Time.time - startTime) / 7f;
            //accelerate a little bit over time. //-- Unclamped is funky, maybe ill use it lmao.
            float speed = hand.trackSpeed + timeOffset * 3f;
            body.MovePosition(Vector2.MoveTowards(body.position, targetPosition, speed * Time.deltaTime));
            if(Vector2.SqrMagnitude(body.position - targetPosition) < 0.05f) //if its about there.
            {
                hand.TransitionToState(new HandWaitQueue(0.3f, new HandSlamState()));
            }
        }
    }

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
            if(entity)
            {
                Debug.Log("Slam found entity", entity);
                if(entity.IsInvincible)
                {   //ignore invincible entities!!!
                    hand.IgnoreCollisionWith(entity.Collider);
                    return;
                }
                else
                {
                    //if the collision normal is facing upwards (its from the object hit towards the hands collider)
                    if(collision.GetContact(0).normal.y > 0.6f)
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
            else if(collision.gameObject.isStatic)
            {
                //Powerful slam hit the ground, shake the camera to show the power of this powerful power boi
                Game.Controller.CameraController.Shake(1f);
                //static collision hit!!!
                foreach(var ent in slammedEntities)
                    ent.Damage(999999f);
                foreach(var joint in hand.GetComponents<RelativeJoint2D>())
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

    ///<summary>Rises up for some time / distance after hitting the ground, then returns to idle</summary>
    public class HandPostSlamRise : HandBehaviourState
    {
        Vector2 targetPosition;
        public override void Enter(BossHand hand)
        {
            targetPosition = hand.Body.position + new Vector2(0f, 7f);
        }

        public override void Update(BossHand hand, float speedMultiplier)
        {
            hand.Body.MovePosition(hand.Body.position + new Vector2(0, hand.slamSpeed / 2f * Time.deltaTime));
            if(hand.Body.position.y >= targetPosition.y) //Lose control once the hand has raised high enough.
                hand.TransitionToState(BossHand.NoControlState);
        }
    }

    ///<summary>Moves the hands into position for "clapping"</summary>
    public class HandClapPrepState : HandBehaviourState
    {
        private static int handsReady = 0;
        private Transform target;
        private float side;
        bool ready = false;
        public HandClapPrepState(Entity target, float xOffset)
        {
            this.target = target.transform;
            side = xOffset;
        }

        public override void Enter(BossHand o)
        {
            //hacky, but should do the job.
            //reset the amount of hands that are ready.
            handsReady = 0;
            o.ActivityStatus = HandState.Attacking;
        }

        public override void Update(BossHand o, float speedMultiplier)
        {
            //5 clapping should start at 5 meters from the player.
            Vector2 targetPosition = target.position + new Vector3(side * 5f, 0f);
            Vector2 nextPosition = Vector2.MoveTowards(o.Body.position, targetPosition, o.trackSpeed * Time.deltaTime);
            o.Body.MovePosition(nextPosition);

            float sqrDist = Vector2.SqrMagnitude(nextPosition - targetPosition);
            if(!ready &&  sqrDist <= 0.1f )
            {
                handsReady++;
                ready = true;
            }
            else if(ready && sqrDist > 0.1f)
            {
                ready = false;
                handsReady --;
            }
            //Check whether both hands are ready, then go.
            if(handsReady == 2)
            {
                //the hand should move in the opposite direction of where it started.
                o.TransitionToState(new HandClapState(-side));
            }
        }
    }

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
            if(collision.gameObject.isStatic)
                return; //static collisions are irrelevant.
            Entity entity = collision.collider.GetComponentInParent<Entity>();
            BossHand otherHand = collision.gameObject.GetComponent<BossHand>();
            if(entity)
            {
                Debug.Log("test");
                //Ignore collisions with invincible entities.
                if(entity.IsInvincible)
                {
                    hand.IgnoreCollisionWith(entity.Collider);
                }
                else
                {
                    float collisionNormalX = collision.GetContact(0).normal.x;
                    //if the collisions normal is roughly pointing in the same direction as where the hand is going:
                    if(collisionNormalX > 0.5f && movementDirection < 0f || collisionNormalX < -0.5f && movementDirection > 0f)
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
            else if(otherHand)
            {
                Game.Controller.CameraController.Shake(1.0f);
                //"kill" all entities inside the clap.
                for(int i = 0; i < clappedEntities.Count; i++)
                {
                    clappedEntities[i].Damage(999999f);
                    clappedEntities[i].Collider.enabled = true; //re-enable the collider we disabled earlier.
                }
                foreach(var joint in hand.GetComponents<RelativeJoint2D>())
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

    ///<summary>Queues up a State to be entered after a given delay. Default is 2 seconds.</summary>
    public class HandWaitQueue : HandBehaviourState
    {
        HandBehaviourState nextState;
        float enterTime = 0f;
        float waitTime = 2f;

        public HandWaitQueue(HandBehaviourState next)
        {
            nextState = next;
        }

        public HandWaitQueue(float waitTime, HandBehaviourState next)
        {
            nextState = next;
            this.waitTime = waitTime;
        }

        public override void Enter(BossHand o)
        {
            o.ActivityStatus = HandState.Waiting;
            enterTime = Time.time;
        }

        public override void Update(BossHand o, float speedMultiplier)
        {
            if(Time.time - enterTime >= waitTime / speedMultiplier)
                o.TransitionToState(nextState);
        }
    }

    public class HandNoControlState : HandBehaviourState
    {
        public override void Enter(BossHand hand)
        {
            hand.ActivityStatus = HandState.Returning;
            //hand.SetActiveCollision(false);
        }

        public override void Update(BossHand hand, float ts){}
    }

    public abstract class HandBehaviourState
    {
        //virtual to share some behaviour that can be added to or overridden completely.
        public virtual void OnCollisionEnter(Collision2D collision, BossHand hand)
        {
            
        }
        public virtual void OnCollisionExit(Collision2D collision, BossHand hand)
        {

        }
        //Exit isnt really required so im just leaving it empty in here to avoid unnecessary lines above.
        public virtual void Exit(BossHand hand){}
        public abstract void Enter(BossHand hand);

        //OnDamaged only used in one state for "parrying"
        public virtual void OnDamaged(){}

        public abstract void Update(BossHand hand, float speedMultiplier);
    }
}