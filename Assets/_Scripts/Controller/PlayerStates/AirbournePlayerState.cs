using UnityEngine;
using Game;

namespace Game.Controller
{
    public class AirbournePlayerState : PlayerStateBehaviour
    {
        public override void FixedStep(Vector2 input, float deltaTime)
        {
            //throw new System.NotImplementedException();
        }

        public override void OnEnterState()
        {
            controller.OnEnterGround += Yeet;
        }

        void Yeet()
        {
            controller.SwitchToState<GroundedPlayerState>();
        }

        public override void OnExitState()
        {
            controller.OnEnterGround -= Yeet;
        }
    }
}
