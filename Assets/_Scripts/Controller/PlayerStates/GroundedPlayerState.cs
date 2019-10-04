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
            force.y = Mathf.Sqrt(controller.JumpHeight * g2);
            force.x = Body.velocity.x;
            Body.velocity = force;
            //Body.AddForce(force, ForceMode2D.Impulse);
        }

        /// <summary>
        /// Movement for the base ground
        /// </summary>
        /// <param name="input">the inputs on x and y</param>
        /// <param name="deltaTime"></param>
        private void Move(Vector2 input, float deltaTime)
        {
            if (!controller.IsGrounded)
                return;
            Vector2 normal = controller.GroundNormal;
            float groundAngle = Vector2.SignedAngle(normal, Vector2.up);
            float absAngle = Mathf.Abs(groundAngle);
            //Test against the max angle to walk.
            //if the friction is low on this piece of ground, slide, otherwise stay
            if (absAngle >= controller.MaxSteepAngle && controller.GroundFriction < 0.3f)
            {
                return;
            }

            //TODO: Accelerate
            //float sqrVelocity = Body.velocity.sqrMagnitude; 
            //float percentSqrVelocity = sqrVelocity / controller.BaseSpeedSqr;

            float acc = input.x * controller.BaseSpeed;

            Vector2 force = Vector2.right * acc;
            force.y = Body.velocity.y;
            Body.velocity = force;

            //Add a force to prevent sliding off hills for no reason. 
            //Testing if the angle is something that matters 
            if (absAngle <= 4f)
                return;

            //TODO: wtf it doesnt work again | i get stuck on corners n shit all the time, jumping is still fucking retarded
            //float alpha = Vector2.Angle(normal, -Fg);
            float sinAlpha = Mathf.Sin(absAngle * Mathf.Deg2Rad);
            
            Vector2 right = RotateVector2D(Vector2.right, - groundAngle);
            right.Normalize();
            Debug.DrawLine(Body.position, Body.position + right);
            
            var groundOffsetDirection = (normal.x >= 0) ? -right : right;

            float absFg = Body.gravityScale * Body.mass * g; //hmm 
            Vector2 Fh = groundOffsetDirection * sinAlpha * -absFg;

            Body.AddForce(Fh, ForceMode2D.Force);
        }

        private void OnLeaveGround()
        {
            Debug.Log("Left Ground");
            //controller.SwitchToState<AirbournePlayerState>();
        }
    }
}
