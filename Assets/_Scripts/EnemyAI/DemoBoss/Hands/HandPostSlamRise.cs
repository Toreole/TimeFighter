using UnityEngine;

namespace Game.Demo.Boss
{
    ///<summary>Rises up for some time / distance after hitting the ground, then returns to idle</summary>
    public class HandPostSlamRise : HandBehaviourState
    {
        Vector2 targetPosition;
        public override void Enter(BossHand hand)
        {
            targetPosition = hand.Body.position + new Vector2(0f, 7f);
        }

        public override void Update(BossHand hand, float speedMultiplier)
        {
            hand.Body.MovePosition(hand.Body.position + new Vector2(0, hand.slamSpeed / 2f * Time.deltaTime));
            if (hand.Body.position.y >= targetPosition.y) //Lose control once the hand has raised high enough.
                hand.TransitionToState(BossHand.NoControlState);
        }
    }
}