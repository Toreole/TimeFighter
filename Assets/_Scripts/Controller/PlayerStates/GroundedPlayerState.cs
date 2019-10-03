using System.Collections;
using UnityEngine;

using static Game.Util;

namespace Game.Controller
{
    public class GroundedPlayerState : PlayerStateBehaviour
    {
        public override void FixedStep(Vector2 input, float deltaTime)
        {
            Move(input, deltaTime);
        }

        /// <summary>
        /// Set up the state
        /// </summary>
        public override void OnEnterState()
        {
            controller.OnPressJump += Jump;
            controller.OnLeaveGround += OnLeaveGround;
            controller.OnEnterGround += EditorOnlyTest;
            controller.StickToGround = true;
        }
        public override void OnExitState()
        {
            controller.OnPressJump -= Jump;
            controller.OnLeaveGround -= OnLeaveGround;
            controller.OnEnterGround -= EditorOnlyTest;
        }
        void EditorOnlyTest()
        {
            controller.StickToGround = true;
        }

        /// <summary>
        /// Old jump.
        /// </summary> 
        private void Jump()
        {
            if (!controller.IsGrounded)
                return;

            controller.StickToGround = false;
            var force = Vector2.zero;
            
            force.y = Mathf.Sqrt(controller.JumpHeight * g2) * Body.mass;
            
            Body.AddForce(force, ForceMode2D.Impulse);
        }

        //TODO: proper movement from left to right and so on.
        private void Move(Vector2 input, float deltaTime)
        {
            if (!controller.IsGrounded)
                return;
            //float sqrVelocity = Body.velocity.sqrMagnitude; 
            //float percentSqrVelocity = sqrVelocity / controller.BaseSpeedSqr;

            float acc = input.x * controller.BaseSpeed;

            Vector2 force = Vector2.right * acc;
            force.y = Body.velocity.y;
            Body.velocity = force;
        }

        private void OnLeaveGround()
        {
            Debug.Log("Left Ground");
            //controller.SwitchToState<AirbournePlayerState>();
        }
    }
}
