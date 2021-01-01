using UnityEngine;

namespace Game.Demo.Boss
    {
    public class BossPhaseOne : BossCombatState
    {
        public override void Enter(BossController o)
        {
            throw new System.NotImplementedException();
        }

        public override void Exit(BossController o)
        {
            throw new System.NotImplementedException();
        }

        public override void Update(BossController o)
        {
            //when hitting 30% health
            if(o.PercentageHealth <= 0.3f)
            {
                TransitionToState(o, new BossIntermissionState());
                return;
            }
            base.Update(o);
        }
    }
}