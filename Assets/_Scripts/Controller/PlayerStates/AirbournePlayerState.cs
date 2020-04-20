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

            //better falling / drag
            lastVel.x = Mathf.Lerp(lastVel.x, 0, 10 * Time.deltaTime);
            //Debug.DrawLine(Body.position, Body.position + Body.velocity, Color.blue, 10);

            //check for entering wall. moveinput and wall normal should be in opposite direction (move against the wall) to auto-enter the wall state.
            if(controller.IsTouchingWall && controller.WallHasFlag(GroundFlags.Climbable) && controller.MoveInputRaw.x * controller.CurrentWall.normal.x < 0)//old: && controller.JumpBeingHeld && lastVel.y <= 0)
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

        /// <summary>
        /// For mid-air jumps
        /// </summary>
        void Jump()
        {
            //obviously only if you can jump
            if(controller.CanAirJump)
            {
                controller.AvailableAirJumps--;
                float xInput = controller.MoveInputRaw.x;
                xInput = Mathf.Abs(xInput) < 0.02f ? 0 : Util.Normalized(xInput);
                //this determines the direction of the jump. -1 = left; 0 = unchanged; 1 = right;
                float xVel = Body.velocity.x;
                Vector2 tVelocity = Vector2.zero;
                //directional jumps should keep a certain base speed, neutral jumps go straight up
                var minXspeed = xInput * controller.BaseSpeed * 0.7f;
                if (xInput > 0f ) //positive
                    tVelocity.x = Mathf.Max(xVel, minXspeed);
                else if (xInput < 0f) //negative
                    tVelocity.x = Mathf.Min(tVelocity.x, minXspeed);
                else //neutral
                    tVelocity.x = 0;
                //jump
                tVelocity.y = controller.JumpForce;
                Body.velocity = tVelocity;
                return;
            }
            if(controller.IsTouchingWall) //simple wall jumps.
            {
                WallInfo wall = controller.CurrentWall;
                //cant climb - cant walljump.?
                //if (!wall.materialInfo.HasFlag(GroundFlags.Climbable))
                    //return; 
                Vector2 direction = wall.normal.x > 0 ? new Vector2(0.707f, 0.707f)  : new Vector2(-0.707f, 0.707f); //spooky magic numbers.
                //direction.Normalize(); //normalize jump vector. - no longer needed since hardcoded normalized vectors lmao.
                Body.velocity = direction * controller.JumpForce;
                controller.FlipX = direction.x > 0; //adjust sprite direction just in case.
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