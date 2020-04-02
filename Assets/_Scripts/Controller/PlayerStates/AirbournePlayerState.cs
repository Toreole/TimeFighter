using UnityEngine;
using Game;

namespace Game.Controller
{
    public class AirbournePlayerState : PlayerStateBehaviour
    {
        public override void FixedStep(Vector2 input, float deltaTime)
        {
            Vector2 lastVel = controller.LastVel;
            //Update the facing direction.
            controller.FlipX = lastVel.x > 0 ? false : lastVel.x < 0 ? true : controller.FlipX;
            //check for entering wall.
            if(controller.IsTouchingWall && controller.JumpBeingHeld && lastVel.y <= 0)
            {
                controller.SwitchToState<WallPlayerState>();
            }
        }

        public override void OnEnterState()
        {
            controller.OnEnterGround += EnterGround;
            controller.OnPressJump += Jump;
            //controller.OnEnterWall += EnterWall;
        }

        public override void OnExitState()
        {
            controller.OnEnterGround -= EnterGround;
            controller.OnPressJump -= Jump;
            //controller.OnEnterWall -= EnterWall;
        }

        void EnterGround(LandingType landing)
        {
            controller.CanAirJump = true;
            HandleFall(landing);
            //Debug.Log(controller.lastVerticalVel); <-- this actually seems to get the correct one
            controller.SwitchToState<GroundedPlayerState>();
        }

        //this is pretty janky
        //void EnterWall(GroundData wallData, bool jumpHeld)
        //{
        //    //high velocity on y (greater than 0) should not cause the player to enter the climbing state.
        //    if (wallData.HasFlag(GroundFlags.Climbable) && jumpHeld)
        //    {
        //        controller.SwitchToState(new WallPlayerState());
        //    }
        //}

        /// <summary>
        /// For mid-air jumps
        /// </summary>
        void Jump()
        {
            //obviously only if you can jump
            if(controller.CanAirJump)
            {
                controller.AvailableAirJumps--;
                float x = controller.MoveInputRaw.x;
                x = Mathf.Abs(x) < 0.02f ? 0 : Util.Normalized(x);
                //this determines the direction of the jump. -1 = left; 0 = unchanged; 1 = right;
                float xVel = Body.velocity.x;
                Vector2 tVelocity = Vector2.zero;
                //directional jumps should keep a certain base speed, neutral jumps go straight up
                var minXspeed = x * controller.BaseSpeed * 0.7f;
                if (x > 0f ) //positive
                    tVelocity.x = Mathf.Max(xVel, minXspeed);
                else if (x < 0f) //negative
                    tVelocity.x = Mathf.Min(tVelocity.x, minXspeed);
                else //neutral
                    tVelocity.x = 0;
                //simple copy pasta from groundedstate jump lol
                tVelocity.y = Mathf.Sqrt(controller.AirJumpHeight * Util.g2);
                Body.velocity = tVelocity;
            }
        }

        /// <summary>
        /// A dash performed in mid-air.
        /// </summary>
        void Dash()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Handles the landing after a fall.
        /// </summary>
        //Air state needs to handle fall before the groundbehaviour enters.
        void HandleFall(LandingType landing)
        {
            Vector2 lastVel = controller.LastVel;

            if (landing == LandingType.Roll) //prioritize y velocity
                Body.velocity = GetGroundRight() * Mathf.Max(controller.BaseSpeed, Mathf.Abs(lastVel.x)) * Util.Normalized(lastVel.x);
            else if (landing == LandingType.HardLanding)
            {
                //TODO: take fall damage in here aswell.
                Body.velocity = Vector2.zero; //Land and dont do movement anymore.
            }
        }
    }
}