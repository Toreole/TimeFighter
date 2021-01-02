using System.Collections;
using UnityEngine;

namespace Game.Demo.Boss
{
    ///<summary>Outsourcing some movement from the BossController. Takes some commands from the BC.</summary>
    public class BossHand : MonoBehaviour, IDamageable
    {
        [SerializeField] //obviously required for animations to work.
        protected Animator animator;
        [SerializeField]
        protected Rigidbody2D body; //rigidbody movement? probably right?
        [SerializeField]
        private Transform locale;

        private Vector3 startingPosition;
        private float trackSpeed, slamSpeed, punchSpeed;

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

        public void SetSpeeds(float trackSpeed, float slamSpeed, float punchSpeed, float multiplier = 1.0f)
        {
            this.trackSpeed = trackSpeed * multiplier;
            this.slamSpeed = slamSpeed * multiplier;
            this.punchSpeed = punchSpeed * multiplier;
        }

        public void ResetHand()
        {
            transform.position = startingPosition;
            ActivityStatus = HandState.Returning;
        }

        //perform a slam attack on the target.
        //1. move above the target
        //2. fall down
        //3. wait until solid terrain was found (collision)
        //!!!! ignore collision with invincible entities (i-frame dodges)
        //4. kill all entities between the hand and the collision
        //5. short delay
        //6. mark hand for returning.
        public void Slam(Entity target)
        {
            ActivityStatus = HandState.Attacking;
            //setup in here
            StartCoroutine(DoSlam(target));
        }

        private IEnumerator DoSlam(Entity target)
        {
            yield return null;
        }

        //try to punch the target.
        //1. move in a straight line
        //2. taking damage stops movement (-> skip to 5.)
        //3. knock back any and all entities hit (damage them)
        //4. move until terrain is hit
        //5. short delay
        //6. mark hand for returning.
        public void Punch(Entity target)
        {
            throw new System.NotImplementedException();
        }

        //From IDamagable, used by punches
        public void Damage(float amount)
        {
            throw new System.NotImplementedException();
        }

        //collision enter MIGHT be used, but checking the rigidbody contacts could suffice.
        //collision is relevant for both punch and slam so idk. Maybe also for detecing whether the player is mounted while this hand is waiting/returning?
        private void OnCollisionEnter2D(Collision2D other)
        {
            throw new System.NotImplementedException();
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