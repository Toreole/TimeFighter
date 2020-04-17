using UnityEngine;

namespace Game.Controller
{
    public class WallPlayerState : PlayerStateBehaviour
    {
        bool completeEnter = false;

        public override void FixedStep(Vector2 input, float deltaTime)
        {
            if (!controller.IsTouchingWall)
                Debug.Log("ledge?"); //TODO: this APPEARS to correctly detect ledges. do some more testing on it and then make player climb up the ledge when pressing up?
            WallInfo wall = controller.CurrentWall;
            if (completeEnter)
            {
                //notes: movement is limited to up/down along the wall.
                //down: depending on friction / slippery?
                //stick to wall -> rotate 

                //default: no input for now
                Body.velocity = Vector2.zero;
                Body.velocity = wall.upTangent * input.y * controller.BaseSpeed * 0.5f;
                //Debug.Log("ree");
            }
            else //enter: slow down
            {
                //lastvel.y needs to be slowed down to +-0 at the beginning?
                Vector2 nextVel = Vector2.MoveTowards(Body.velocity, Vector2.zero, deltaTime * controller.BaseSpeed / 2f);
                if(nextVel.sqrMagnitude < 0.4)
                {
                    nextVel = Vector2.zero;
                    completeEnter = true;
                }
                Body.velocity = nextVel;
            }
            //Body.rotation = Vector2.SignedAngle(controller.CurrentWall.normal)
            //Align the player with the wall. 
            var normal = wall.normal; //temp the normal of the wall.
            Body.rotation = Vector2.SignedAngle((normal.x >= 0 ? Vector2.right : Vector2.left), normal);
            //Move towards the wall
            Vector2 offset = (normal.x >= 0 ? Vector2.right : Vector2.left) * (controller.HalfWidth + 0.02f);
            Body.position = wall.point + offset; 
        }

        public override void OnEnterState()
        {
            controller.OnPressJump += Jump;
            //Body.velocity = Vector2.zero;
            controller.OnEnterGround += EnterGround;
            Body.gravityScale = 0f;

            //make sure that the velocity on X is 0
            var startVelocity = Body.velocity;
            startVelocity.x = 0;
            Body.velocity = startVelocity;
            //only do the "slide when the enter y vel is negative!
            completeEnter = startVelocity.y >= 0;

            controller.FlipX = controller.CurrentWall.normal.x > 0;
            Debug.Log("Wall State Enter");
        }

        public override void OnExitState()
        {
            controller.OnPressJump -= Jump;
            controller.OnEnterGround -= EnterGround;
            Body.gravityScale = 1f;
            Body.rotation = 0f;
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

        void EnterGround(LandingType ignored)
        {
            controller.SwitchToState<GroundedPlayerState>(); //Grounded state overrides this.
        }
    }
}
