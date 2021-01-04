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
                Debug.Log("Boss Target has died.");
                TransitionToState(o, new BossIdleState());
                o.ResetBoss();
                return;
            }
            o.MoveHands();
            o.FollowTarget();
            o.TickDownAttackTimers(Time.deltaTime * o.AttackSpeed);

            //can we attack? out gives the first available hand.
            if(o.CanAttack(out BossHand attackingHand))
            {
                var targetPos = o.Target.Position;
                //o.PunchAttackTimer; o.SlamAttackTimer; the timers for these attacks.
                //How to determine which attack should happen?
                //depend on where the target is relative to the hand? below on Y => slam?
                //just pick the first available attack? like with an if/elseif/elseif
                if(o.SlamAttackTimer <= 0)
                {
                    o.SlamAttackTimer = 7f;//TODO: NEED BETTER WAY OF RESETTING THIS!!!!!!
                    o.GlobalAttackTimer = 4f;
                    attackingHand.Slam(o.Target);
                }
            }
        }
        //Combat states need to tick down the cooldown on attacks and trigger attacks.

    }
}