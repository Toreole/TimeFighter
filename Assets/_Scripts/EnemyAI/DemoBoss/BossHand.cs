using UnityEngine;

namespace Game.Demo.Boss
{
    ///<summary>Outsourcing some movement from the BossController. Takes some commands from the BC.</summary>
    public class BossHand : MonoBehaviour
    {
        [SerializeField] //obviously required for animations to work.
        protected Animator animator;
        [SerializeField]
        protected Rigidbody2D body; //rigidbody movement? probably right?
        [SerializeField]
        private Transform locale;


        private Vector3 startingPosition;

        //more like a flag and less of a state. this is not a statemachine, its just describing the latest activity of the hand, 
        //which is determined by the BossController, it's states, and collisions with objects in the world.
        public HandState ActivityStatus {get; set;} = HandState.Returning;
        public bool IsReady => ActivityStatus == HandState.Ready;
        
        [System.NonSerialized] internal Vector2 returnVelocity = Vector2.zero;

        public Vector3 preferredPosition => locale.position;

        private void Awake() 
        {
            startingPosition = transform.position;
        }

        public void ResetHand()
        {
            transform.position = startingPosition;
            ActivityStatus = HandState.Returning;
        }

    }

    public enum HandState 
    {
        Undefined = 0,
        Ready = 1,
        Attacking = 2,
        Waiting = 3,
        Returning = 4,
        MountedByPlayer = 5,
        Disabled = 6
    }
}