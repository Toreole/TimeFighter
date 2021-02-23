using UnityEngine;

namespace Game.Demo.Boss
{
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
            if (!ready && sqrDist <= 0.1f)
            {
                handsReady++;
                ready = true;
            }
            else if (ready && sqrDist > 0.1f)
            {
                ready = false;
                handsReady--;
            }
            //Check whether both hands are ready, then go.
            if (handsReady == 2)
            {
                //the hand should move in the opposite direction of where it started.
                o.TransitionToState(new HandClapState(-side));
            }
        }
    }

}