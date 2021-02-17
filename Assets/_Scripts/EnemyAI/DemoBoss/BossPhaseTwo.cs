using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Patterns.States;

namespace Game.Demo.Boss
{
    public class BossPhaseTwo : BossCombatState
    {
        float lastBuff = 0f;

        public override void Enter(BossController o)
        {
            lastBuff = Time.time;
            o.AttackSpeed *= o.EnrageSpeedBuff;
            o.SetAnimationPhase(2);
        }

        public override void Exit(BossController o)
        {
            
        }

        public override void Update(BossController o)
        {
            if(Time.time - lastBuff >= o.EnrageInterval)
            {
                lastBuff = Time.time;
                o.AttackSpeed *= o.EnrageSpeedBuff;
            }
            base.Update(o);
        }
    }
}