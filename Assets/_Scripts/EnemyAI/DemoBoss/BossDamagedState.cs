using UnityEngine;
using Game.Patterns.States;

namespace Game.Demo.Boss
{
    public class BossDamagedState : State<BossController>
    {

        float enterTime = -100f;

        public override void Enter(BossController o)
        {
            enterTime = Time.time;
        }

        //No exit for this needed rn.
        public override void Exit(BossController o) { }

        public override void Update(BossController o)
        {
            if(Time.time - enterTime > o.DamageStunTime)
                o.PopOverrideState();
        }
    }
}