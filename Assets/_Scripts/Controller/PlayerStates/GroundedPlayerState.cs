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
            controller.SetAnimBool("Grounded", true);
            controller.OnPressJump += Jump;
            controller.OnLeaveGround += OnLeaveGround;
            controller.StickToGround = true;
            HandleFall();
            //Debug.Log(controller.lastVerticalVel);
            //TODO: enter state -> roll / damage 
        }
        /// <summary>
        /// Pseudo deconstruct
        /// </summary>
        public override void OnExitState()
        {
            controller.OnPressJump -= Jump;
            controller.OnLeaveGround -= OnLeaveGround;
        }

        /// <summary>
        /// Handles the landing after a fall.
        /// </summary>
        void HandleFall()
        {
            Vector2 lastVel = controller.LastVel;
            if (Mathf.Abs(lastVel.x) > 0.75f) //only if there is some x velocity going on already
            {
                if (lastVel.y <= controller.FallDamageThreshold)
                {
                    //Take damage.
                    //TODO: damage system
                }
                else if (lastVel.y <= controller.RollFallThreshold) //prioritize y velocity
                {
                    //TODO: roll
                    //Debug.Log("Ground Roll");
                    controller.SetAnimTrigger("Grounded_Roll");
                    Body.velocity = GetGroundRight() * Mathf.Max(controller.BaseSpeed, Mathf.Abs(lastVel.x)) * Util.Normalized(controller.LastVel.x);
                }
                else if(Mathf.Abs(lastVel.x) > controller.BaseSpeed) //low y vel, but x vel larger than the basespeed
                {
                    controller.SetAnimTrigger("Grounded_Roll");
                    Body.velocity = GetGroundRight() * Mathf.Max(controller.BaseSpeed, Mathf.Abs(lastVel.x)) * Util.Normalized(controller.LastVel.x);
                }
            }
            else //no relevant x velocity
            {
                //Debug.Log("Grounded Land");
                controller.SetAnimTrigger("Grounded_Land"); //TODO: grounded land doesnt reenable controls??
                Body.velocity = Vector2.zero; //Land and dont do movement anymore.
            }
        }

        /// <summary>
        /// Old jump.
        /// </summary> 
        private void Jump()
        {
            //TODO: only be able to jump when slipping (slope) and a certain speed is reached ???
            if (controller.GroundFriction <= controller.SlipThreshold)
                if(Body.velocity.magnitude < 4f)
                    return; 
            controller.StickToGround = false;
            var force = Vector2.zero;
            force.y = Mathf.Sqrt(controller.JumpHeight * Util.g2);
            force.x = Body.velocity.x;
            Body.velocity = force;
            //Body.AddForce(force, ForceMode2D.Impulse);
        }

        Vector2 GetGroundRight()
        {
            float groundAngle = Vector2.SignedAngle(controller.GroundNormal, Vector2.up);
            Vector2 right = Util.RotateVector2D(Vector2.right, -groundAngle);
            return right;
        }
        Vector2 GetGroundRight(out float angle)
        {
            angle = Vector2.SignedAngle(controller.GroundNormal, Vector2.up);
            Vector2 right = Util.RotateVector2D(Vector2.right, -angle);
            return right;
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
            if (absAngle >= controller.MaxSteepAngle || controller.GroundFriction < 0.3f)
            {
                //"sliding"
                return;
            }

            //baseline for the following calculations
            Vector2 velocity = Body.velocity;
            //send x velocity to the animator
            controller.SetAnimFloat("XVelocity", Mathf.Abs(velocity.x)); //TODO: THIS IS VERY HACKY I DONT LIKE IT. 
            //flip the renderer on the Y axis (mirror) if youre going left
            controller.FlipX = Mathf.Approximately(velocity.x, 0)? controller.FlipX : velocity.x < 0;

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