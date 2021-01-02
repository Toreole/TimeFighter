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
            throw new System.NotImplementedException();
        }

        public override void Update(BossController o)
        {
            //when at the set threshold for the second phase to trigger.
            if(o.PercentageHealth <= o.IntermissionHealthThreshold)
            {
                TransitionToState(o, new BossIntermissionState());
                return;
            }
            base.Update(o);
        }
    }
}