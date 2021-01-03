﻿using UnityEngine;
using Game.Patterns.States;

namespace Game.Demo.Boss
{
    ///<summary>Has to be instanced and given a target. Moves above the target and enters the SlamState.</summary>
    public class HandTrackTargetState : HandBehaviourState
    {
        Entity target;

        public HandTrackTargetState(Entity target)
        {
            this.target = target;
        }

        public override void Enter(BossHand hand)
        {
            hand.ActivityStatus = HandState.Attacking;
        }

        //Move above the player.
        public override void Update(BossHand hand, float speedMultiplier)
        {
            Vector2 targetPosition = target.Position + new Vector2(0f, 7f);
            var body = hand.Body;
            body.MovePosition(Vector2.MoveTowards(body.position, targetPosition, hand.trackSpeed * Time.deltaTime));
            if(body.position == targetPosition)
            {
                hand.TransitionToState(new HandWaitQueue(0.3f, new HandSlamState()));
            }
        }
    }

    ///<summary>Slams down until static collision is hit. Kills all vulnerable entities on the way.</summary>
    public class HandSlamState : HandBehaviourState
    {
        System.Collections.Generic.List<Entity> slammedEntities = new System.Collections.Generic.List<Entity>();

        float speed;
        public override void Enter(BossHand o)
        {
            o.ActivityStatus = HandState.Attacking;
            o.SetActiveCollision(true);
            speed = o.slamSpeed;
        }

        public override void OnCollisionEnter(Collision2D collision, BossHand hand)
        {
            //checking whether we collided with an entity. Entities usually have colliders in the first child so based on that we're checking a parent.
            Entity entity = collision.collider.GetComponentInParent<Entity>();
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
                        entity.Stun(1000f);
                        //add the entity to the list of slammed entities that will be killed at the end of the slam.
                        slammedEntities.Add(entity);
                        //now just ignore the collision between these objects for now.
                        hand.IgnoreCollisionWith(collision.collider);
                        //use a joint to move the entity along with this hand.
                        var joint = hand.gameObject.AddComponent<RelativeJoint2D>();
                        joint.connectedBody = entity.Body;
                    }
                }
            } 
            else if(collision.gameObject.isStatic)
            {
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

    public class HandPunchState : HandBehaviourState
    {
        public override void Enter(BossHand o)
        {
            throw new System.NotImplementedException();
        }

        public override void OnCollisionEnter(Collision2D collision, BossHand hand)
        {
            throw new System.NotImplementedException();
        }

        public override void OnCollisionExit(Collision2D collision, BossHand hand)
        {
            throw new System.NotImplementedException();
        }

        public override void Update(BossHand o, float speedMultiplier)
        {
            throw new System.NotImplementedException();
        }
    }

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
            hand.SetActiveCollision(false);
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