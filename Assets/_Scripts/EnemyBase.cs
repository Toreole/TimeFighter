﻿using UnityEngine;
using System.Collections;

namespace Game
{
    /// <summary>
    /// The base class for all enemies
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class EnemyBase : MonoBehaviour
    {
        protected int currentHP = 1;
        protected int facingDirection = 1;
        protected bool active = false;
        protected bool turningAround = false;
        protected bool playerIsNear = false;
        protected Vector2 startPos = Vector2.zero;
        protected Rigidbody2D body;
        protected bool isGrounded = false;

        protected Transform Player { get { return GameManager.instance.PlayerTransform; } }
        protected float RelativePlayerX { get { return Player.position.x - transform.position.x; } }
        protected int NormRelativeX { get { return (int) (RelativePlayerX / Mathf.Abs(RelativePlayerX)); } }
        protected bool LookingTowardsPlayer { get { return (facingDirection == NormRelativeX); } }

        [SerializeField]
        protected EnemySettings settings;

        /// <summary>
        /// attention all gamers, this is basically FixedUpdate, but is only called when the enemy is active! double epic.
        /// </summary>
        protected abstract void UpdateEnemy();

        /// <summary>
        /// One time initial update
        /// </summary>
        protected virtual void Start()
        {
            GameManager.OnLevelStart += OnLevelStart;
            GameManager.OnLevelFail  += OnLevelFail;

            startPos = transform.position;
            body = GetComponent<Rigidbody2D>();
        }

        protected void FixedUpdate()
        {
            if (!active)
                return;
            if (!turningAround && !LookingTowardsPlayer && playerIsNear)
            {
                StartCoroutine(TurnAround());
            }
            CheckGrounded();
            UpdateEnemy();
            Move();
        }

        /// <summary>
        /// Do this when the level is started or re-started.
        /// </summary>
        protected virtual void OnLevelStart()
        {
            active = true;
            if (settings.Movement == MovementPattern.ShortDistance)
                StartCoroutine(WanderAround());

            transform.position = startPos;
            currentHP = settings.HP;
            body.WakeUp();
            foreach (var col in GetComponentsInChildren<Collider2D>())
            {
                col.enabled = true;
            }
        }

        protected virtual void OnLevelFail()
        {
            active = false; 
            //reset enemy? idk lol
        }

        /// <summary>
        /// Move based on the movement pattern inside the settings.
        /// </summary>
        protected virtual void Move()
        {
            switch(settings.Movement)
            {
                case MovementPattern.ShortDistance:
                    WanderCheck();
                    break;
                case MovementPattern.EdgeToEdge:
                    WalkUntilEdge();
                    break;
                default:
                    break;
            }
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
            active = false;
            GameManager.instance.RegisterDead(this);
            body.Sleep();
            foreach( var col in GetComponentsInChildren<Collider2D>() )
            {
                col.enabled = false;
            }
        }

        /// <summary>
        /// turn towards the player with some amount of delay or something i guess.
        /// </summary>
        protected virtual IEnumerator TurnAround()
        {
            //start turning after some delay
            turningAround = true;
            yield return new WaitForSeconds(settings.TurnSpeed);
            //smooth the actual turn.
            int turnEnd = 0 - facingDirection;
            float oldX = transform.localScale.x;
            float newScaleX = transform.localScale.x;

            //2 plus 3 is 5.5. weird maths!
            for(float t = 0f; t < 0.2f; t += Time.deltaTime)
            {
                newScaleX = oldX + ((t * turnEnd) / 0.2f) * 2f;
                transform.localScale = new Vector3(newScaleX, 1f, 1f);
                yield return null;
            }

            facingDirection = turnEnd;
            transform.localScale = new Vector3(turnEnd, 1f, 1f);
            turningAround = false;
        }

        //just turn around on the spot.
        protected virtual IEnumerator TurnAroundImmediate()
        {
            turningAround = true;
            int turnEnd = 0 - facingDirection;
            float oldX = transform.localScale.x;
            float newScaleX = transform.localScale.x;

            //2 plus 3 is 5.5. weird maths!
            for (float t = 0f; t < 0.15f; t += Time.deltaTime)
            {
                newScaleX = oldX + ((t * turnEnd) / 0.15f) * 2f;
                transform.localScale = new Vector3(newScaleX, 1f, 1f);
                yield return null;
            }

            facingDirection = turnEnd;
            transform.localScale = new Vector3(turnEnd, 1f, 1f);
            turningAround = false;
        }

        /// <summary>
        /// Make sure not to walk away from the spawn too far.
        /// </summary>
        protected void WanderCheck()
        {
            if (turningAround || playerIsNear || settings.WanderDistance <= 0.1f)
                return;
            
            if(((Vector2) transform.position - startPos ).magnitude >= settings.WanderDistance)
            {
                var xDir = startPos.x - transform.position.x;
                xDir /= Mathf.Abs(xDir);
                if ((int)xDir != facingDirection)
                //Move towards startPos
                {
                    StartCoroutine(TurnAroundImmediate());
                }
            }
           
        }

        protected virtual IEnumerator WanderAround()
        {
            while (active)
            {
                if (playerIsNear || !isGrounded)
                    yield return new WaitForSeconds(0.2f);
                else
                {
                    //else just go into a random direction / turn around when needed.
                    var rand = Random.Range(0, 100);
                    if (rand < 2)
                    {
                        //Turn around when player is not near
                        if (!playerIsNear)
                        {
                            StartCoroutine(TurnAroundImmediate());
                            yield return new WaitForSeconds(0.3f);
                        }
                    }
                    else if (rand < 8)
                    {
                        body.velocity = new Vector2(0, body.velocity.y);
                        yield return new WaitForSeconds(0.5f);
                    }
                    //Stop
                    else
                    {
                        //Walk into the direction youre facing
                        body.velocity = new Vector2(settings.MovementSpeed * facingDirection, body.velocity.y);
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
        }

        /// <summary>
        /// Walk from left to right and right to left, until you hit a wall, or some sort of cliff.
        /// </summary>
            //TODO: This is working pretty consistently and good, but maybe move the wall detection into a OnCollision thing? hmmmm
        protected virtual void WalkUntilEdge()
        {
            if (turningAround || playerIsNear || !isGrounded)
                return;
            //walk into the direction this entity is facing
            if(Physics2D.Raycast(transform.position, new Vector2(facingDirection, 0), 0.6f))
            {
                //Detect a wall
                StartCoroutine(TurnAroundImmediate());
                body.velocity = Vector2.zero;
                //Debug.Log("Theres a wall in my way");
            }
            else if(!Physics2D.Raycast((Vector2)transform.position + new Vector2(facingDirection * 0.6f, 0), Vector2.down, 0.6f))
            {
                //Theres a cliff here or something thats not ground
                StartCoroutine(TurnAroundImmediate());
                body.velocity = Vector2.zero;
                //Debug.Log("Theres a cliff in my way");
            }
            else
            {   //normal behaviour.
                body.velocity = new Vector2(facingDirection * settings.MovementSpeed, body.velocity.y);
            }
        }

        /// <summary>
        /// Player detection radius thing
        /// </summary>
        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
                playerIsNear = true;
        }

        protected virtual void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
                playerIsNear = false;
        }

        /// <summary>
        /// Checks if this Enemy is standing on the ground.
        /// </summary>
        protected void CheckGrounded()
        {
            isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1);
        }
    }
}