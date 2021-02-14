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
        //Vector2 originalVelocity;
        float lastDistance;
        bool dashBuffer = false;

        public override void FixedStep(Vector2 input, float deltaTime)
        {
            //moves the dashing player forward
            Body.velocity = dashDirection * controller.DashSpeed;

            //handle entering a wall.
            if(controller.IsTouchingWall)
                controller.SwitchToState<WallPlayerState>();

            //distance travelled
            float distTravelled = Vector3.Distance(origin, Body.position);
            float delta = distTravelled - lastDistance;
            lastDistance = distTravelled;

            //! Temp fix attempt. should avoid infinite loops of dashing into collision.
            //this ignores the dash buffer.
            if (delta <= Mathf.Epsilon)
            {
                Debug.Log("0");
                if (controller.IsGrounded)
                    controller.SwitchToState<GroundedPlayerState>();
                else if(controller.IsTouchingWall && controller.WallHasFlag(GroundFlags.Climbable))
                    controller.SwitchToState<WallPlayerState>();
                else
                    controller.SwitchToState<AirbournePlayerState>();
                Body.velocity = Vector2.zero;
                return;
            }

            //finish the dash.
            if (distTravelled >= dashDistance)
            {
                //perform the buffered dash next.
                //for this dont switch the state, just reset.
                if (dashBuffer)
                {
                    dashBuffer = false;
                    controller.Stamina -= controller.DashCost;
                    origin = Body.position;
                    dashDirection = input;
                    lastDistance = -0.1f;
                    return;
                }
                //if no dash is buffered, continue with other stuff.
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

        void BufferDash()
        {
            if (controller.Stamina >= controller.DashCost)
                dashBuffer = true;
        }

        void GroundCancel(LandingType landing)
        {
            controller.SwitchToState<GroundedPlayerState>();
        }

        public override void OnEnterState()
        {
            dashDistance = controller.DashDistance * (wasMidAir ? 0.5f : 1f);
            //originalVelocity = Body.velocity;
            origin = Body.position;
            Body.gravityScale = 0f;
            lastDistance = -0.1f;

            controller.OnDash += BufferDash;
            controller.OnEnterGround += GroundCancel;
        }

        public override void OnExitState()
        {
            Body.gravityScale = 1f;
            controller.OnEnterGround -= GroundCancel;
            controller.OnDash += BufferDash;
        }

        public DashingPlayerState(Vector2 direction, bool wasInMidAirBefore)
        {
            dashDirection = direction;
            wasMidAir = wasInMidAirBefore;
        }
    }
}
