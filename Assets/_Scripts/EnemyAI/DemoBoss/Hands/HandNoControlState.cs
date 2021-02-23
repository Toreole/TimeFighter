namespace Game.Demo.Boss
{
    public class HandNoControlState : HandBehaviourState
    {
        public override void Enter(BossHand hand)
        {
            hand.ActivityStatus = HandState.Returning;
            //hand.SetActiveCollision(false);
        }

        public override void Update(BossHand hand, float ts) { }
    }
}