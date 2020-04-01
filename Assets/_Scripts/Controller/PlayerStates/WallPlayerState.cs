using UnityEngine;

namespace Game.Controller
{
    public class WallPlayerState : PlayerStateBehaviour
    {
        public override void FixedStep(Vector2 input, float deltaTime)
        {
            //notes: movement is limited to up/down along the wall.
            //down: depending on friction / slippery?
            //lastvel.y needs to be slowed down to +-0 at the beginning?
        }

        public override void OnEnterState()
        {
            controller.OnPressJump += Jump;
            Body.velocity = Vector2.zero;
            Body.gravityScale = 0f;
            Debug.Log("Wall State Enter");
        }

        public override void OnExitState()
        {
            controller.OnPressJump -= Jump;
            Body.gravityScale = 1f;
        }

        //leave the wall with a jump
        //TODO: multiple ways (see Trello)
        void Jump()
        {
            //uhh how do i rotate text in ms paint?
            //the X Flip should coincide with the direction the player is facing, so X in this case should be the opposite of that.
            Vector2 direction = new Vector2(controller.FlipX ? 1 : -1, 1).normalized;
            //Apply the jump "force" (velocity).
            Body.velocity = direction * controller.JumpForce;
            //turn around
            controller.FlipX = !controller.FlipX;
            //switch to airbourne controls.
            controller.SwitchToState<AirbournePlayerState>();
        }
    }
}
