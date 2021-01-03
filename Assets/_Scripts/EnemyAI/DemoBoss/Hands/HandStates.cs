using UnityEngine;
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
            
        }

        //Move above the player.
        public override void Update(BossHand hand)
        {
            
        }
    }

    ///<summary>Slams down until static collision is hit. Kills all vulnerable entities on the way.</summary>
    public class HandSlamState : HandBehaviourState
    {
        public override void Enter(BossHand o)
        {
            o.ActivityStatus = HandState.Attacking;
            o.SetActiveCollision(true);
        }

        public override void OnCollisionEnter(Collision2D collision, BossHand hand)
        {
            //checking whether we collided with an entity. Entities usually have colliders in the first child so based on that we're checking a parent.
            Entity entity = collision.transform.GetComponentInParent<Entity>();
            if(entity)
            {
                if(entity.IsInvincible)
                {
                    hand.IgnoreCollisionWith(entity.Collider);
                    return;
                }
            }
        }

        public override void OnCollisionExit(Collision2D collision, BossHand hand)
        {
            throw new System.NotImplementedException();
        }

        public override void Update(BossHand o)
        {
            throw new System.NotImplementedException();
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

        public override void Update(BossHand o)
        {
            throw new System.NotImplementedException();
        }
    }

    public class HandWaitQueue : HandBehaviourState
    {
        HandBehaviourState nextState;
        float enterTime = 0f;

        public HandWaitQueue(HandBehaviourState next)
        {
            nextState = next;
        }

        public override void Enter(BossHand o)
        {
            o.ActivityStatus = HandState.Waiting;
            enterTime = Time.time;
        }

        public override void Update(BossHand o)
        {
            throw new System.NotImplementedException();
        }
    }

    public class HandNoControlState : HandBehaviourState
    {
        public override void Enter(BossHand hand)
        {
            hand.ActivityStatus = HandState.Returning;
        }

        public override void Update(BossHand hand){}
    }

    public abstract class HandBehaviourState
    {
        //virtual to share some behaviour that can be added to or overridden completely.
        public virtual void OnCollisionEnter(Collision2D collision, BossHand hand)
        {
            if(collision.gameObject.CompareTag(hand.playerTag))
            {
                //check whether on the side or above.
            }
        }
        public virtual void OnCollisionExit(Collision2D collision, BossHand hand)
        {

        }
        //Exit isnt really required so im just leaving it empty in here to avoid unnecessary lines above.
        public virtual void Exit(BossHand hand){}
        public abstract void Enter(BossHand hand);

        //OnDamaged only used in one state for "parrying"
        public virtual void OnDamaged(){}

        public abstract void Update(BossHand hand);
    }
}