using System.Collections;
using UnityEngine;

namespace Game.Controller
{
    public class GroundedPlayerState : PlayerStateBehaviour
    {
        public override void FixedStep(Vector2 input, float deltaTime)
        {
            Move(input, deltaTime);
        }

        public override void OnEnterState()
        {
            controller.OnPressJump += Jump;
        }
        public override void OnExitState()
        {
            controller.OnPressJump -= Jump;
        }

        private void Jump()
        {
            controller.Body.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        }

        private void Move(Vector2 input, float deltaTime)
        {
            var force = Vector2.right * input.x * controller.BaseSpeed;
            controller.Body.AddForce(force);
        }

    }
}
