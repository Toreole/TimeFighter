using UnityEngine;
using Game.Patterns.States;

namespace Game.Demo.Boss
{
    public class BossIntermissionState : State<BossController>
    {

        float enterTime = 0f;

        public override void Enter(BossController o)
        {
            //o.Invincible = true; //Should the boss be invincible during the intermission?
            enterTime = Time.time;
            o.SetAnimationPhase(2);
            o.IsInvincible = true;
        }

        public override void Exit(BossController o)
        {
            o.IsInvincible = false;
        }

        public override void Update(BossController o)
        {
            if(Time.time - enterTime >= 5f) //5 second intermission
                TransitionToState(o, new BossPhaseTwo());
        }
    }
}