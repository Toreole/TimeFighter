using Game;
using Game.Controller;
using UnityEngine;

namespace Game.Controller.PlayerStates
{
    public class PlayerStunnedState : PlayerStateBehaviour
    {
        float startTime;
        float stunTime;

        public override void FixedStep(Vector2 input, float deltaTime)
        {
            var lastVelocity = Body.velocity;
            lastVelocity.x = Mathf.MoveTowards(lastVelocity.x, 0, Mathf.Abs(lastVelocity.x) * deltaTime);
            Body.velocity = lastVelocity;
            if(Time.time - startTime >= stunTime)
            {
                if(controller.IsGrounded)
                    controller.SwitchToState<GroundedPlayerState>();
                else if (controller.IsTouchingWall)
                    controller.SwitchToState<WallPlayerState>();
                else 
                    controller.SwitchToState<AirbournePlayerState>();
            }
        }

        public PlayerStunnedState(float t)
        {
            stunTime = t;
        }

        public override void OnEnterState()
        {
            startTime = Time.time;
        }

        public override void OnExitState()
        {
            
        }
    }
}