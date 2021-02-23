using UnityEngine;

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
            if (target.IsGrounded)
                groundedY = Mathf.Min(target.Position.y, groundedY);
            targetPosition.y = groundedY + 7.0f;
            var body = hand.Body;
            float timeOffset = (Time.time - startTime) / 7f;
            //accelerate a little bit over time. //-- Unclamped is funky, maybe ill use it lmao.
            float speed = hand.trackSpeed + timeOffset * 3f;
            body.MovePosition(Vector2.MoveTowards(body.position, targetPosition, speed * Time.deltaTime));
            if (Vector2.SqrMagnitude(body.position - targetPosition) < 0.05f) //if its about there.
            {
                hand.TransitionToState(new HandWaitQueue(0.3f, new HandSlamState()));
            }
        }
    }
}