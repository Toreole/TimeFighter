using UnityEngine;
using System.Collections;

namespace Game
{
    /// <summary>
    /// The base class for all enemies
    /// </summary>
    [System.Obsolete]
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class EnemyBase : Entity
    {
        [SerializeField]
        protected float halfHeight = 0.5f;
        
        protected int facingDirection = 1;
        protected bool turningAround = false;
        protected bool playerIsNear = false;
        protected Vector2 startPos = Vector2.zero;
        protected bool isGrounded = false;

        protected Transform Player { get { return LevelManager.instance.PlayerTransform; } }
        protected float RelativePlayerX { get { return Player.position.x - transform.position.x; } }
        protected int NormRelativeX { get { return (int) (RelativePlayerX / Mathf.Abs(RelativePlayerX)); } }
        protected bool LookingTowardsPlayer { get { return (facingDirection == NormRelativeX); } }

        [SerializeField]
        protected internal EnemySettings settings;

        public EnemySettings Settings { get { return settings; } }
        
        /// <summary>
        /// attention all gamers, this is basically FixedUpdate, but is only called when the enemy is active! double epic.
        /// </summary>
        [System.Obsolete("Ughh i dont really need this i guess.")]
        protected abstract void UpdateEnemy();

        /// <summary>
        /// One time initial update
        /// </summary>
        protected override void Start()
        {
            LevelManager.OnLevelStart += OnLevelStart;
            LevelManager.OnLevelFail  += OnLevelFail;

            startPos = transform.position;
            body = GetComponent<Rigidbody2D>();
        }

        protected virtual void FixedUpdate()
        {
            if (!active)
                return;
            if (!turningAround && !LookingTowardsPlayer && playerIsNear)
            {
                StartCoroutine(TurnAround());
            }
            CheckGrounded();
            //UpdateEnemy();
            Move();
        }

        /// <summary>
        /// Do this when the level is started or re-started.
        /// </summary>
        protected override void OnLevelStart()
        {
            active = true;
            if (settings.Movement == MovementPattern.ShortDistance)
                StartCoroutine(WanderAround());

            transform.position = startPos;
            currentHealth = settings.HP;
            body.WakeUp();
            foreach (var col in GetComponentsInChildren<Collider2D>())
            {
                col.enabled = true;
            }
        }

        protected override void OnLevelFail()
        {
            active = false;
            //reset enemy? idk lol
        }

        protected override void OnLevelComplete()
        {
            active = false;
        }

        /// <summary>
        /// Move based on the movement pattern inside the settings.
        /// </summary>
        protected virtual void Move()
        {
            if (!isGrounded)
                return;
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

        public override void ProcessHit(AttackHitData hitData)
        {
            currentHealth -= hitData.Damage;
            if (currentHealth <= 0)
                Die();
        }

        public override void ProcessHit(AttackHitData hitData, bool onlyDamage)
        {
            if (onlyDamage)
            {
                currentHealth -= hitData.Damage;
                if (currentHealth <= 0)
                    Die();
                return;
            }
            ProcessHit(hitData);
        }

        protected virtual void Die()
        {
            active = false;
            LevelManager.instance.RegisterDead(this);
            body.Sleep();
            foreach( var col in GetComponentsInChildren<Collider2D>() )
            {
                col.enabled = false;
            }
            Debug.Log(name + " died");
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
            if (turningAround || playerIsNear)
                return;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(facingDirection, 0), 0.6f, 1);
            //walk into the direction this entity is facing
            if ( hit )
            {
                //Detect a wall
                StartCoroutine(TurnAroundImmediate());
                body.velocity = Vector2.zero;
                //Debug.Log("Theres a wall in my way");
            }
            else
            {
                hit = Physics2D.Raycast((Vector2)transform.position + new Vector2(facingDirection * 0.6f, 0), Vector2.down, halfHeight + 0.1f, 1);
                if ( !hit )
                {
                    //Theres a cliff here or something thats not ground
                    StartCoroutine(TurnAroundImmediate());
                    body.velocity = Vector2.zero;
                    //Debug.Log("Theres a cliff in my way");
                }
                else
                {   //normal behaviour.
                    if(hit.collider.CompareTag("Environment"))
                        body.velocity = new Vector2(facingDirection * settings.MovementSpeed, body.velocity.y);
                }
            }
        }

        protected virtual void OnCollisionEnter2D(Collision2D col)
        {
            //TODO this does not account for new collisions with the same collider. Also make better tags for environment
            if (settings.Movement != MovementPattern.EdgeToEdge || col.collider.CompareTag("Player"))
                return;
            var xNormal = Mathf.Abs(col.contacts[0].normal.x);
            if (xNormal > 0.8f && !turningAround)
            {
                StartCoroutine(TurnAroundImmediate());
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
            isGrounded = Physics2D.Raycast(transform.position, Vector2.down, halfHeight + 0.05f);
        }

        /// <summary>
        /// Reset this entity to the defaults
        /// </summary>
        internal override void ResetEntity()
        {
            StopAllCoroutines();
            transform.position = startPos;
            transform.localScale = Vector3.one;
            active = false;
            currentHealth = settings.HP;
            facingDirection = 1;
            active = false;
            turningAround = false;
            playerIsNear = false;
            body.velocity = Vector2.zero;
        }
    }
}