using System.Collections;
using UnityEngine;

using static Game.Util;

namespace Game.Controller
{
    public class GroundedPlayerState : PlayerStateBehaviour
    {
        public override void FixedStep(Vector2 input, float deltaTime)
        {
            controller.Stamina += deltaTime * controller.StaminaRegen;
            if (controller.IgnorePlayerInput)
                return; //it shouldnt move when the player input is ignored anyway.
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
        }
        /// <summary>
        /// Pseudo deconstruct
        /// </summary>
        public override void OnExitState()
        {
            controller.OnPressJump -= Jump;
            controller.OnLeaveGround -= OnLeaveGround;
            controller.StickToGround = false;
        }
        
        /// <summary>
        /// Old jump.
        /// </summary> 
        private void Jump()
        {
            //TODO: only be able to jump when slipping (slope) and a certain speed is reached ???
            if (controller.CurrentGround.HasFlag(GroundFlags.Slippery))
                if(Body.velocity.magnitude < 4f)
                    return; 
            controller.StickToGround = false;
            var force = Vector2.zero; //technically not a force, but a velocity.
            force.y = controller.JumpForce; 
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
            Vector2 right = GetGroundRight(out float groundAngle);
            float absAngle = Mathf.Abs(groundAngle);
            //Test against the max angle to walk.
            //if the friction is low on this piece of ground, slide, otherwise stay
            if (absAngle >= controller.MaxSteepAngle || controller.GroundHasFlag(GroundFlags.Slippery))
            {
                //"sliding"
                return;
            }

            //baseline for the following calculations
            Vector2 velocity = Body.velocity;
            //flip the renderer on the Y axis (mirror) if youre going left
            controller.FlipX = Mathf.Abs(velocity.x) < 0.01? controller.FlipX : velocity.x < 0;

            AddGroundCounterForce();

            //Check for raw input, we want to stop moving when the axis is no longer actively used.
            if (Mathf.Approximately(controller.MoveInputRaw.x, 0f))
            {
                //no horizontal input here.
                velocity.x = Mathf.Lerp(velocity.x, 0f, 0.55f*deltaTime);
                Body.velocity = velocity;
                return;
            }

            DoMovement();

            void DoMovement()
            {
                //The acceleration for this frame
                Vector2 acceleration = right * (input.x * controller.Acceleration * deltaTime);
                velocity += acceleration;
                float xInAbsSpeed = Mathf.Abs(input.x) * controller.BaseSpeed;
                //Clamp x velocity, y should not matter that much
                velocity.x = Mathf.Clamp(velocity.x, -xInAbsSpeed, xInAbsSpeed);
                Body.velocity = velocity;
            }
            
            void AddGroundCounterForce()
            {
                //Only add a ground counter force if the surface is sloped.
                if (absAngle <= 4f)
                    return;

                //! Absolute sinAlpha solves the issue with the force not being in the correct direction
                float sinAlpha = Mathf.Abs(Mathf.Sin(groundAngle * Mathf.Deg2Rad));

                //normalizing the right vector shouldnt be necessary as a normalized vector stays normalized when rotated.
                //right.Normalize();

                var groundOffsetDirection = (controller.GroundNormal.x >= 0) ? -right : right;

                float absFg = Body.gravityScale * Body.mass * g; //hmm 

                Vector2 Fh = groundOffsetDirection * sinAlpha * absFg;

                Body.AddForce(Fh, ForceMode2D.Force);
            }
        }
        
        private void OnLeaveGround()
        {
            Debug.DrawLine(Body.position, Body.position + Vector2.up, Color.red, duration: 200f);
            Debug.Log("Left Ground"); 
            controller.StickToGround = false;
            controller.SwitchToState<AirbournePlayerState>();
        }

        /// <summary>
        /// A dash performed on ground.
        /// </summary>
        void Dash()
        {
            throw new System.NotImplementedException();
        }
    }
}