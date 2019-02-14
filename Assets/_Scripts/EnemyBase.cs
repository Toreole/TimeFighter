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
        protected bool playerIsNear = false;
        protected Vector2 startPos = Vector2.zero;
        protected Rigidbody2D body;
        protected bool isGrounded = false;

        protected Transform Player { get { return GameManager.instance.PlayerTransform; } }
        protected float RelativePlayerX { get { return Player.position.x - transform.position.x; } }
        protected int NormRelativeX { get { return (int) (RelativePlayerX / RelativePlayerX); } }
        protected bool LookingTowardsPlayer { get { return (facingDirection == NormRelativeX); } }

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
            body = GetComponent<Rigidbody2D>();
            //StartCoroutine(TurnAround()); <- only testing lol
        }

        protected void FixedUpdate()
        {
            if (!active)
                return;
            if (!turningAround && !LookingTowardsPlayer && playerIsNear)
            {
                Debug.Log(facingDirection + " --- " + NormRelativeX);
                StartCoroutine(TurnAround());
            }
            CheckGrounded();
            UpdateEnemy();
        }

        protected virtual void OnLevelStart()
        {
            active = true;
            if (settings.Movement == MovementPattern.ShortDistance)
                StartCoroutine(WanderAround());
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
            active = false;
            GameManager.instance.RegisterDead(this);
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
            //TODO: Fix this piece of shit. => its constantly spinning. check whether im looking in the correct direction first.
            if (turningAround)
                return;

            if(((Vector2) transform.position - startPos ).magnitude >= settings.WanderDistance)
            {
                //Move towards startPos
                StartCoroutine(TurnAroundImmediate());
            }
           
        }

        protected virtual IEnumerator WanderAround()
        {
            while (active)
            {
                if (playerIsNear)
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
                            Debug.Log("reeeeeee");
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
        protected virtual void WalkUntilEdge()
        {
            if (turningAround)
                return;
            //TODO: This
            //walk into the direction this entity is facing
        }

        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
                playerIsNear = true;
        }

        protected virtual void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
                playerIsNear = true;
        }

        protected void CheckGrounded()
        {
            isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1);
        }
    }
}