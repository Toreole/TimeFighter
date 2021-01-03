using UnityEngine;

namespace Game.Demo.Boss
    {
    public class BossPhaseOne : BossCombatState
    {
        public override void Enter(BossController o)
        {
            
        }

        public override void Exit(BossController o)
        {
            
        }

        public override void Update(BossController o)
        {
            //when at the set threshold for the second phase to trigger.
            if(o.PercentageHealth <= o.IntermissionHealthThreshold)
            {
                TransitionToState(o, new BossIntermissionState());
                return;
            }

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
                    attackingHand.Slam(o.Target);
                }
            }

            base.Update(o);
        }
    }
}