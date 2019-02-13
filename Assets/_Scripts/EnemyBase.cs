using UnityEngine;
using System.Collections;

namespace Game
{
    /// <summary>
    /// The base class for all enemies
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public abstract class EnemyBase : MonoBehaviour
    {
        protected int currentHP = 1;
        protected int facingDirection = 1;
        protected bool active = false;
        protected bool turningAround = false;
        protected Vector2 startPos = Vector2.zero;

        protected Transform Player { get { return GameManager.instance.PlayerTransform; } }
        protected float RelativePlayerX { get { return Player.position.x - transform.position.x; } }
        protected float NormRelativeX { get { return RelativePlayerX / RelativePlayerX; } }

        [SerializeField]
        protected EnemySettings settings;

        /// <summary>
        /// Move based on the movement pattern inside the settings.
        /// </summary>
        protected abstract void Move();
        /// <summary>
        /// attention all gamers, this is basically FixedUpdate, but is only called when the enemy is active! double epic.
        /// </summary>
        protected abstract void UpdateEnemy();

        protected virtual void Start()
        {
            GameManager.OnLevelStart += OnLevelStart;
            GameManager.OnLevelFail  += OnLevelFail;
            currentHP = settings.HP;
            startPos = transform.position;
        }

        protected void FixedUpdate()
        {
            if (!active)
                return;
            //TODO: redo turning around logic stuff. probably a field of vision detection thing?
            if (!turningAround && NormRelativeX == facingDirection)
                StartCoroutine(TurnAround());
            UpdateEnemy();
        }

        protected virtual void OnLevelStart()
        {
            active = true;
        }

        protected virtual void OnLevelFail()
        {
            active = false;
        }

        public virtual void ProcessHit(AttackHitData hitData)
        {
            Damage(hitData.Damage);
        }

        protected void Damage(int amount)
        {
            currentHP -= amount;
            if (currentHP <= 0)
                Die();
        }

        protected virtual void Die()
        {
            //TODO: implement a way to tell the GameManager that this enemy just died. => also pass in the settings.TimeGain info
        }

        /// <summary>
        /// turn towards the player with some amount of delay or something i guess.
        /// </summary>
        protected virtual IEnumerator TurnAround()
        {
            //TODO: all of this shit
            return null;
        }

        /// <summary>
        /// Wander around the scene randomly. But max settings.WanderDistance away from the startPos.
        /// </summary>
        protected virtual void Wander()
        {
            //TODO: This
        }

        /// <summary>
        /// Walk from left to right and right to left, until you hit a wall, or some sort of cliff.
        /// </summary>
        protected virtual void WalkUntilEdge()
        {
            //TODO: This
        }
    }
}