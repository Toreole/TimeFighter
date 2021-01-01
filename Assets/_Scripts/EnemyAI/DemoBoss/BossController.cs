using Game.Events;
using UnityEngine;
using Game.Patterns.States;
using NaughtyAttributes;
using Game.Serialization;

namespace Game.Demo.Boss
{
    public class BossController : StateMachine<BossController>, IDamageable //IDamagable implementation inside of here? needs collision in that case...
    {
        [SerializeField]
        protected GameEvent startEvent;

        public Entity Target { get; private set; }
        public float PercentageHealth => health /maxHealth;

        private float health;
        private float maxHealth;

        //for single depth push-down automata. specific for this boss rn.
        State<BossController> stateBuffer;

        [SerializeField]
        protected float damageStunTime = 3.5f;

        public float DamageStunTime => damageStunTime;

#region Attacks

        [SerializeField]
        private BossFist[] fistsToAttackWith; //Fists have their own behaviour more or less. Should be animated.
        //fists have two attacks: one where they charge straight at the player as a fist fr fr (hulk go smash!!!!) so they can go diagonally
        //and one attack where they open the palm and go straight down, they move above the player and then slam down real fast. (insta-kill)
        public BossFist[] Hands => fistsToAttackWith;
        [SerializeField] //smoothdamp time and max speed for returning.
        protected float handSmoothing = 2f, handSpeed = 5;

        [SerializeField] //The minimum time between individual attacks.
        private float globalAttackCooldown = 4f;
        [SerializeField]
        private float slamAttackCooldown = 7f; //the palm slams down every cd.
        [SerializeField]
        private float punchAttackCooldown = 10f; //the time between fists go out to punch
        [SerializeField]
        private float mountedHandAttackDelay = 5f; //if the player is mounting one of the hands for more than this time, attack the player on the hand.

        [SerializeField]
        private float attackSpeed = 1; //the rate at which attack cooldowns go down (multiplier for deltatime).

        public float HandSmoothing => handSmoothing;
        public float HandSpeed => handSpeed;
        public float GlobalAttackTimer {get; set;} = 4f;

        public float SlamAttackTimer {get; set;} = 7f;
        public float PunchAttackTimer {get; set;} = 10f;

        public bool CanAttack(out BossHand hand)
        {
            if(GlobalAttackTimer <= 0f)
            foreach(var h in Hands)
                if(h.ActivityStatus == HandState.Ready )
                {
                    hand = h.instance;
                    return true;
                }
            hand = null;
            return false;
        }

#endregion

        // Start is called before the first frame update
        void Start()
        {
            currentState = new BossIdleState(); //just assign default value in here. Idle state does absolutely nothing in this case.
            startEvent.AddListener(StartBossEncounter);
        }

        // Update is called once per frame
        void Update()
        {
            currentState.Update(this);
        }

        private void StartBossEncounter()
        {
            //Show healthbar in persistent UI
            //optional: startup animation.
            TransitionToState(new BossPhaseOne());
        }

        public void ResetBoss()
        {
            throw new System.NotImplementedException();
        }

        ///<summary>Moves the Hands while under the control of the boss controller.</summary>
        public void MoveHands()
        {
            foreach(var hand in Hands)
            {
                var transform = hand.instance.transform;
                //Attacking and Waiting are handled by the Hand itself.
                //In here we only need to return it to the preferred position and make it ready.
                if(hand.ActivityStatus == HandState.Returning)
                {
                    //Try smoothdamping the position of the hand.
                    transform.position = Vector2.SmoothDamp(transform.position, hand.preferredPosition, ref hand.returnVelocity, handSmoothing, handSpeed, Time.deltaTime);
                    //if its approximately at the position it should be at, mark it as ready.
                    if((transform.position - hand.preferredPosition).sqrMagnitude < 0.01f)
                        hand.ActivityStatus = HandState.Ready;
                }
                else if(hand.ActivityStatus == HandState.Ready) //while the hand is ready, just keep it at the position.
                    transform.position = hand.preferredPosition;
            }
        }

        ///<summary>Follows the target on the horizontal axis if it's too far away.</summary>
        public void FollowTarget()
        {
            float offset = Target.Position.x - transform.position.x;
            if(Mathf.Abs(offset) > 5)
            {
                transform.position += new Vector3(1, 0) * Util.Normalized(offset) * Time.deltaTime;
            }
        }

        ///<summary>Removes [amount] seconds off the attack timers</summary>
        public void TickDownAttackTimers(float amount)
        {
            GlobalAttackTimer -= amount;
            PunchAttackTimer -= amount;
            SlamAttackTimer -= amount;
        }

        ///<summary>Transition to a given state</summary>
        public override void TransitionToState(State<BossController> s)
        {
            currentState.Exit(this);
            currentState = s;
            currentState.Enter(this);
        }

        ///<summary>Enters a temporary override state, the current state is buffered.</summary>
        public void PushOverrideState(State<BossController> state)
        {
            //technically we dont leave the current state so theres no need to call Exit(this)
            stateBuffer = currentState;
            currentState = state;
            state.Enter(this);
        }   

        ///<summary>Returns from the override state to the buffered state.</summary>
        public void PopOverrideState()
        {
            currentState.Exit(this);
            currentState = stateBuffer;
            //not required: currentState = null;
        }

        public void Damage(float amount)
        {
            throw new System.NotImplementedException();
        }

//Serialization for this boss is something to worry about at a much later time.
#region Serialization

        public class BossData : ObjectData 
        {

        }
        public override void Deserialize(ObjectData data)
        {
            var bossData = data as BossData;
            throw new System.NotImplementedException();
        }

        public override ObjectData Serialize()
        {
            return new BossData();
            throw new System.NotImplementedException();
        }
        #endregion

        [System.Serializable]
        public class BossFist //All of these variables could easily be on the BossHand script aswell????????
        {
            public BossHand instance;
            [SerializeField]
            private Transform locale;
            public HandState ActivityStatus {get; set;}
            public bool IsReady => ActivityStatus == HandState.Ready;
            
            [System.NonSerialized] internal Vector2 returnVelocity = Vector2.zero;

            public Vector3 preferredPosition => locale.position;
        }
    }
}