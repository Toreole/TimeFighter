using UnityEngine;
using System.Collections;
using Game.Controller;

namespace Game.Controller.PlayerStates
{
    public class DashingPlayerState : PlayerStateBehaviour
    {
        bool wasMidAir;
        Vector2 dashDirection;
        float dashDistance;
        Vector3 origin;
        Vector2 originalVelocity;

        public override void FixedStep(Vector2 input, float deltaTime)
        {
            //moves the dashing player forward
            Body.velocity = dashDirection * controller.DashSpeed;

            if(controller.IsTouchingWall)
                controller.SwitchToState<WallPlayerState>();

            if (Vector3.Distance(origin, Body.position) >= dashDistance)
            {
                //Body.velocity = originalVelocity;
                Body.velocity *= 0.5f;
                if (controller.IsTouchingWall && wasMidAir)
                {
                    controller.SwitchToState<WallPlayerState>();
                }
                else if (controller.IsGrounded)
                {
                    controller.SwitchToState<GroundedPlayerState>();
                }
                else
                    controller.SwitchToState<AirbournePlayerState>();
            }
        }

        void GroundCancel(LandingType landing)
        {
            controller.SwitchToState<GroundedPlayerState>();
        }

        public override void OnEnterState()
        {
            dashDistance = controller.DashDistance * (wasMidAir ? 0.5f : 1f);
            originalVelocity = Body.velocity;
            origin = Body.position;
            Body.gravityScale = 0f;

            controller.OnEnterGround += GroundCancel;
        }

        public override void OnExitState()
        {
            Body.gravityScale = 1f;
            controller.OnEnterGround -= GroundCancel;
        }

        public DashingPlayerState(Vector2 direction, bool wasInMidAirBefore)
        {
            dashDirection = direction;
            wasMidAir = wasInMidAirBefore;
        }
    }
}
