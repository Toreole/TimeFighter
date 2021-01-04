using Game.Patterns.States;

namespace Game.Demo.Boss
{
    public class BossIdleState : State<BossController> //is this even required tbh?
    {
        public override void Enter(BossController o)
        {
            
        }

        public override void Exit(BossController o)
        {
        }

        public override void Update(BossController o){}
    }
}