using UnityEngine;

namespace Game.Demo.Boss
{
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
            if (Time.time - enterTime >= waitTime / speedMultiplier)
                o.TransitionToState(nextState);
        }
    }

}