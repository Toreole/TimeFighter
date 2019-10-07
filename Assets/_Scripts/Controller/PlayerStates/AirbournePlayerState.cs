using UnityEngine;
using Game;

namespace Game.Controller
{
    public class AirbournePlayerState : PlayerStateBehaviour
    {
        public override void FixedStep(Vector2 input, float deltaTime)
        {
            //TODO: think about some usecases for this, atm its just dead lol.
        }

        public override void OnEnterState()
        {
            controller.OnEnterGround += EnterGround;
            controller.OnPressJump += Jump;
        }

        public override void OnExitState()
        {
            controller.OnEnterGround -= EnterGround;
            controller.OnPressJump -= Jump;
        }

        void EnterGround()
        {
            controller.CanAirJump = true;
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
    }
}