using UnityEngine;

namespace Game.Controller
{
    public class WallPlayerState : PlayerStateBehaviour
    {
        bool completeEnter = false;

        public override void FixedStep(Vector2 input, float deltaTime)
        {
            WallInfo wall = controller.CurrentWall;
            if (completeEnter)
            {
                DoWallMovement(input, wall);
            }
            else //enter: slow down
            {
                WaitEnterWall(deltaTime);
            }
            //Last step that should always be done.
            AdjustWallOffset(wall);
        }

        /// <summary>
        /// Rotates the player along the wall and adjusts the position.
        /// </summary>
        /// <param name="wall"></param>
        void AdjustWallOffset(WallInfo wall)
        {
            //Align the player with the wall. 
            Vector2 normal = wall.normal; //temp the normal of the wall.
            Body.rotation = Vector2.SignedAngle((normal.x >= 0 ? Vector2.right : Vector2.left), normal);
            //Move towards the wall
            Vector2 offset = (normal.x >= 0 ? Vector2.right : Vector2.left) * (controller.HalfWidth + 0.02f);
            Body.position = wall.point + offset;
        }

        /// <summary>
        /// Wait for the character to slow down before doing anything.
        /// </summary>
        /// <param name="deltaTime"></param>
        void WaitEnterWall(float deltaTime)
        {
            //lastvel.y needs to be slowed down to +-0 at the beginning?
            Vector2 nextVel = Vector2.MoveTowards(Body.velocity, Vector2.zero, deltaTime * controller.BaseSpeed * 2f);
            if (nextVel.sqrMagnitude < 0.4)
            {
                nextVel = Vector2.zero;
                completeEnter = true;
            }
            Body.velocity = nextVel;
        }

        /// <summary>
        /// Move along the wall.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="wall"></param>
        void DoWallMovement(Vector2 input, WallInfo wall)
        {
            //notes: movement is limited to up/down along the wall.
            //down: depending on friction / slippery?
            //stick to wall -> rotate 
            if (!controller.IsTouchingWall)
            {
                if (wall.point.y > Body.position.y)
                {
                    //go airborne and drop down pog.
                    if (input.y < -0.8f)
                    {
                        controller.SwitchToState<AirbournePlayerState>();
                        return;
                    }
                    //Debug.Log("lower ledge");
                }
                else
                {
                    //TODO: climb up the ledge
                }
            } 
            //Update velocity.
            //Body.velocity = Vector2.zero;
            Body.velocity = wall.upTangent * input.y * controller.BaseSpeed * 0.5f;
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
            float vertical = controller.MoveInputRaw.y;
            //uhh how do i rotate text in ms paint?
            //the X Flip should coincide with the direction the player is facing, so X in this case should be the opposite of that.
            Vector2 direction = new Vector2(controller.FlipX ? 1 : -1, vertical);
            direction.Normalize();
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
