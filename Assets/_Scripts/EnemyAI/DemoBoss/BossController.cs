using Game.Events;
using UnityEngine;
using Game.Patterns.States;
using NaughtyAttributes;
using Game.Serialization;
using UnityEngine.UI;
using Game.UI;

namespace Game.Demo.Boss
{
    public class BossController : StateMachine<BossController>, IDamageable, ISerialized //IDamagable implementation inside of here? needs collision in that case...
    {
        [SerializeField]
        private new string name; //new string because unity cant do shit and has named a property "name". who does that? properties are PascalCase aaaaaaaaaaaa
        [SerializeField]
        protected GameEvent startEvent;

        public Entity Target { get; private set; }
        public float PercentageHealth => health /maxHealth;

        private float health;
        [SerializeField]
        private float maxHealth;
        [SerializeField]
        private float intermissionHealthThreshold = 0.3f;
        public float IntermissionHealthThreshold => intermissionHealthThreshold;

        //for single depth push-down automata. specific for this boss rn.
        State<BossController> stateBuffer;

        [SerializeField]
        protected float damageStunTime = 3.5f;

        public float DamageStunTime => damageStunTime;

        //UI:
        private Slider healthBar;

#region Attacks

        [SerializeField]
        private BossHand[] hands; //Fists have their own behaviour more or less. Should be animated.
        //fists have two attacks: one where they charge straight at the player as a fist fr fr (hulk go smash!!!!) so they can go diagonally
        //and one attack where they open the palm and go straight down, they move above the player and then slam down real fast. (insta-kill)
        public BossHand[] Hands => hands;
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
        private float baseAttackSpeed = 1; //the rate at which attack cooldowns go down (multiplier for deltatime).
        private float attackSpeed = 1;
        [SerializeField]
        private float enrageSpeedBuff = 1.3f; //30% faster.
        [SerializeField]
        private float enrageInterval = 25f; //every [interval] seconds, the enrageSpeedBuff gets applied in P2
        [SerializeField]
        private float handSlamSpeed, handPunchSpeed, handTrackSpeed;

        public float HandSmoothing => handSmoothing;
        public float HandSpeed => handSpeed;
        public float GlobalAttackTimer {get; set;} = 4f;

        public float SlamAttackTimer {get; set;} = 7f;
        public float PunchAttackTimer {get; set;} = 10f;
        public float AttackSpeed {get => attackSpeed; 
            set 
            { 
                attackSpeed = value;
                foreach(var h in hands)//update the speed that the hands should attack with.
                    h.SetSpeeds(handTrackSpeed, handSlamSpeed, handPunchSpeed, attackSpeed);
            }
        }
        public float EnrageSpeedBuff => enrageSpeedBuff;
        public float EnrageInterval => enrageInterval;


        public bool CanAttack(out BossHand hand)
        {
            if(GlobalAttackTimer <= 0f)
            foreach(var h in Hands)
                if(h.IsReady)
                {
                    hand = h;
                    return true;
                }
            hand = null;
            return false;
        }

#endregion

        // Start is called before the first frame update
        void Start()
        {
            health = maxHealth;
            attackSpeed = baseAttackSpeed;
            currentState = new BossIdleState(); //just assign default value in here. Idle state does absolutely nothing in this case.
            foreach(var hand in hands)
                hand.SetSpeeds(handTrackSpeed, handSlamSpeed, handPunchSpeed, attackSpeed);
            startEvent.AddListener(StartBossEncounter);
        }

        // Update is called once per frame
        void Update()
        {
            currentState.Update(this);
        }

        private void StartBossEncounter(Object ent)
        {
            Debug.Log("Boss Started");
            Target = ent as Entity;
            //Show healthbar in persistent UI
            healthBar = PersistentUI.GetBossHealthAndSetupDisplay(this.name, maxHealth, health);
            //optional: startup animation.
            TransitionToState(new BossPhaseOne());
        }

        public void ResetBoss()
        {
            health = maxHealth;
            attackSpeed = baseAttackSpeed;
            GlobalAttackTimer = globalAttackCooldown;
            SlamAttackTimer   = slamAttackCooldown;
            PunchAttackTimer  = punchAttackCooldown;

            foreach(var hand in hands)
                hand.ResetHand();
            //throw new System.NotImplementedException();
        }

        ///<summary>Moves the Hands while under the control of the boss controller.</summary>
        public void MoveHands()
        {
            foreach(var hand in Hands)
            {
                var transform = hand.transform;
                //Attacking and Waiting are handled by the Hand itself.
                //In here we only need to return it to the preferred position and make it ready.
                if(hand.ActivityStatus == HandState.Returning)
                {
                    //Try smoothdamping the position of the hand.
                    transform.position = Vector2.SmoothDamp(transform.position, hand.preferredPosition, ref hand.returnVelocity, handSmoothing / attackSpeed, handSpeed, Time.deltaTime);
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
            GlobalAttackTimer = Mathf.Max(0, GlobalAttackTimer - amount);
            PunchAttackTimer = Mathf.Max(0, PunchAttackTimer - amount);
            SlamAttackTimer = Mathf.Max(0, SlamAttackTimer - amount);
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
            health -= amount;
            healthBar.value = health;
            if(health <= 0)
            {
                //TODO: Let the boss die.
            }
        }

//Serialization for this boss is something to worry about at a much later time.
#region Serialization

        public class BossData : ObjectData 
        {

        }
        public void Deserialize(ObjectData data)
        {
            var bossData = data as BossData;
            throw new System.NotImplementedException();
        }

        public ObjectData Serialize()
        {
            return new BossData();
            throw new System.NotImplementedException();
        }
        [SerializeField, HideInInspector]
        private string objectID;
        public string ObjectID { get => objectID; set => objectID = value; }
#endregion

#region Debugging
        
        //Show some info on the GUI
        private void OnGUI() 
        {
            if(!DebugUtil.ShowDebugGUI || health <= 0) //if the debugGUI is disabled, or the boss is dead, return.
                return;
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            stringBuilder.AppendLine($"<b>Active Boss: {this.name}</b>");
            stringBuilder.AppendLine($"ObjectID: {this.objectID}");
            stringBuilder.AppendLine($"Target: {this.Target?.name ?? "none"}");
            stringBuilder.AppendLine($"State: {currentState.GetType().Name}");
            stringBuilder.AppendLine($"AttackSpeed: {this.attackSpeed.ToString("0.00")}");
            stringBuilder.AppendLine($"GlobalAttack: {GlobalAttackTimer.ToString("0.00")}");
            stringBuilder.AppendLine($"SlamAttack: {SlamAttackTimer.ToString("0.00")}");
            stringBuilder.AppendLine("<b>Hands:</b>");
            foreach(var hand in hands)
            {
                stringBuilder.AppendLine($"  {hand.name}:");
               stringBuilder.AppendLine($"  Status: {hand.ActivityStatus.ToString()}");
            }
        
            GUI.skin.box.alignment = TextAnchor.UpperLeft;
            GUI.skin.box.richText = true;
            GUILayout.Box(stringBuilder.ToString());
            if(GUILayout.Button("Damage"))
                this.Damage(10f);

        }
#endregion
    }
}