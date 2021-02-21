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

            //Checks if both hands can attack at the same time. this takes priority.
            if(o.CanAttackBothHands() && o.ClapAttackTimer <= 0f)
            {
                o.ClapAttackTimer = o.ClapCooldown;
                var hands = o.Hands;
                float maxX = float.MinValue;
                for(int i = 0; i < hands.Length; i++)
                {
                    //body position instead of transform position because the builtin transform is wack.
                    float x = hands[i].Body.position.x;
                    maxX = Mathf.Max(maxX, x);
                }
                //do the same loop, but this time give the order to attack.
                for(int i = 0; i < o.Hands.Length; i++)
                {
                    float x = hands[i].Body.position.x;
                    //the one at maximumX should come from the right side.
                    hands[i].Clap(o.Target, maxX == x ? 1f : -1f);
                }
            }

            //Can the boss attack with a single-handed attack?
            if(o.CanAttack(out BossHand attackingHand))
            {
                var targetPos = o.Target.Position;
                //o.PunchAttackTimer; o.SlamAttackTimer; the timers for these attacks.
                //How to determine which attack should happen?
                //depend on where the target is relative to the hand? below on Y => slam?
                //just pick the first available attack? like with an if/elseif/elseif
                if(o.SlamAttackTimer <= 0f)
                {
                    o.SlamAttackTimer = o.SlamCooldown;
                    o.GlobalAttackTimer = o.GlobalAttackCooldown;
                    attackingHand.Slam(o.Target);
                }
            }
        }
        //Combat states need to tick down the cooldown on attacks and trigger attacks.

    }
}