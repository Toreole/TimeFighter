using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Patterns.States;

namespace Game.Demo.Boss
{
    public abstract class BossCombatState : State<BossController>
    {
        public override void Update(BossController o)
        {
            if(o.Target.IsDead) //Every state while in combat should check whether the target(player) has died. Then reset the encounter.
            {
                TransitionToState(o, new BossIdleState());
                o.ResetBoss();
                return;
            }
            o.MoveHands();
            o.FollowTarget();
            o.TickDownAttackTimers(Time.deltaTime * o.AttackSpeed);
        }
        //Combat states need to tick down the cooldown on attacks and trigger attacks.

    }
}