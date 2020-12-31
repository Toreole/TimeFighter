using Game.Events;
using UnityEngine;
using Game.Patterns.States;
using NaughtyAttributes;
using Game.Serialization;

namespace Game.Demo.Boss
{
    public class BossController : StateMachine<BossController>//, IDamageable //IDamagable implementation inside of here? needs collision in that case...
    {
        [SerializeField]
        protected GameEvent startEvent; 
        [SerializeField, Tag]
        protected string playerTag;
        [SerializeField]
        protected BossFist[] fists;

        public Entity Target { get; private set; }
        public float PercentageHealth => health /maxHealth;

        private float health;
        private float maxHealth;

#region Attacks

        float timeBetweenAttacks;
        BossFist[] fistsToAttackWith; //Fists have their own behaviour more or less. Should be animated.
        //fists have two attacks: one where they charge straight at the player as a fist fr fr (hulk go smash!!!!) so they can go diagonally
        //and one attack where they open the palm and go straight down, they move above the player and then slam down real fast. (insta-kill)

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
            
        }

        public void ResetBoss()
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
        public class BossFist
        {
            public BossHand instance;
            [SerializeField]
            private Transform locale;
            public HandState activity;

            public Vector3 preferredPosition => locale.position;
        }
    }
}