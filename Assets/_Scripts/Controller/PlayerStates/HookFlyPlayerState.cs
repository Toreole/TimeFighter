using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.Controller.PlayerStates
{
    /// <summary>
    /// The class that actually controls the movement of the player while hooked.
    /// </summary>
    public class HookFlyPlayerState : PlayerStateBehaviour
    {
        Vector2 targetPosition;
        SpriteRenderer hook;
        float currentDist = 0;

        public override void FixedStep(Vector2 ignore, float deltaTime)
        {
            //!input is probably ignored here. Jumping and taking damage could interrupt the hook?
            //1. Move towards the hook destination.
            float distance = deltaTime * controller.BaseSpeed * 3f; //yikes, hardcoded numbers
            currentDist -= distance;
            //movement direction.
            Vector2 direction = (targetPosition - Body.position);
            direction.Normalize();
            Body.velocity = controller.BaseSpeed * 4f * direction;
            //1.5 adjust the sprite.
            hook.size = new Vector2(1, currentDist);

            //2. enter wall if required.
            if (controller.IsTouchingWall)
                controller.SwitchToState<WallPlayerState>();
            else if (currentDist < 0)
                controller.SwitchToState<AirbournePlayerState>();
        }

        //constructor requires the target position of the grapple hit.
        public HookFlyPlayerState(Vector2 target, SpriteRenderer rend)
        {
            targetPosition = target;
            hook = rend;
            currentDist = hook.size.y;
        }
        
        public override void OnEnterState()
        {
            controller.OnEnterGround += OnEnterGround;
            Body.gravityScale = 0f;
            //? Body.isKinematic = true; //i dont know about this one chief?
        }

        public override void OnExitState()
        {
            controller.OnEnterGround += OnEnterGround;
            GameObject.Destroy(hook.gameObject);
            Body.gravityScale = 1f;
        }

        void OnEnterGround(LandingType ignore)
        {
            controller.SwitchToState<GroundedPlayerState>();
        }
    }
}
