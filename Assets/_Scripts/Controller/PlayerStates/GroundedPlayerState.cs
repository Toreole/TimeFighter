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
            controller.StickToGround = true;
            //Debug.Log(controller.lastVerticalVel);
            //TODO: enter state -> roll / damage 
        }
        public override void OnExitState()
        {
            controller.OnPressJump -= Jump;
            controller.OnLeaveGround -= OnLeaveGround;
        }

        /// <summary>
        /// Old jump.
        /// </summary> 
        private void Jump()
        {
            controller.StickToGround = false;
            var force = Vector2.zero;
            force.y = Mathf.Sqrt(controller.JumpHeight * Util.g2);
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
            Vector2 normal = controller.GroundNormal;
            float groundAngle = Vector2.SignedAngle(normal, Vector2.up);
            float absAngle = Mathf.Abs(groundAngle);
            //Test against the max angle to walk.
            //if the friction is low on this piece of ground, slide, otherwise stay
            if (absAngle >= controller.MaxSteepAngle || controller.GroundFriction < 0.3f)
            {
                //"sliding"
                return;
            }
            Vector2 right = Util.RotateVector2D(Vector2.right, -groundAngle);
            Vector2 velocity = Body.velocity;

            Debug.DrawLine(Body.position, Body.position + right, Color.blue);

            //This should be decent enough i guess
            Vector2 acceleration = right * (input.x * controller.Acceleration * deltaTime);
            velocity += acceleration;
            //hmmm i wanted to clamp the overall velocity including y, but it doesnt seem to work with jumping at all
            velocity.x = Mathf.Clamp(velocity.x, -controller.BaseSpeed, controller.BaseSpeed);
            Body.velocity = velocity;
            
            //Only add a ground counter force if the surface is sloped.
            if (absAngle <= 4f)
                return;

            //TODO: the force isnt really strong enough or whatever so it
            float sinAlpha = Mathf.Sin(absAngle * Mathf.Deg2Rad);
            
            right.Normalize();
            
            var groundOffsetDirection = (normal.x >= 0) ? -right : right;

            float absFg = Body.gravityScale * Body.mass * g; //hmm 
            Vector2 Fh = groundOffsetDirection * sinAlpha * -absFg;

            Body.AddForce(Fh, ForceMode2D.Force);
        }
        
        private void OnLeaveGround()
        {
            Debug.Log("Left Ground"); 
            controller.StickToGround = false;
            controller.SwitchToState<AirbournePlayerState>();
        }
    }
}
