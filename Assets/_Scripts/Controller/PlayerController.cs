using UnityEngine;
using Game;
using System;
using System.Collections;
using System.Collections.Generic;

using static Game.Util;

namespace Game.Controller
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : Entity
    {
        [Header("Active Player Control")]
        [SerializeField]
        protected bool active = false;

        [Header("Camera Stuff")]
        [SerializeField]
        protected new Camera camera = null;
        protected int facingDirection = 1;
        protected Vector2 directionToMouse = Vector2.zero;

        [Header("Physics And Movement")]
        [SerializeField, Tooltip("The empty transform child, will be automatically set in Start()")]
        protected Transform ground;
        [SerializeField, Tooltip("The Rigidbody component, will be automatically set in Start()")]
        protected Rigidbody2D body;
        [SerializeField, Tooltip("height of the players hitbox")]
        protected float playerHeight  = 1.0f;
        [SerializeField, Tooltip("width of the players hitbox")]
        protected float playerWidth = 1.0f;
        [SerializeField, Tooltip("The fixed height the player can jump (in Units).")]
        protected float jumpHeight = 1.0f;
        [SerializeField, Tooltip("how fast the player usually moves on the ground.")]
        protected float targetSpeed = 1.0f;
        [SerializeField, Tooltip("how fast the speed of the player changes while on ground.")]
        protected float acceleration = 2.0f;
        [SerializeField, Tooltip("how fast should the player change momentum while mid-air")]
        protected float airControlStrength = 0.25f;
        [SerializeField, Tooltip("strength of wall jumps")]
        protected float wallJumpStrength = 0.75f;
        [SerializeField, Tooltip("the speed of the hook when it flies")]
        protected float hookSpeed = 25.0f;
        [SerializeField, Tooltip("the range of the hook")]
        protected float hookRange = 10.0f;
        [SerializeField, Tooltip("the strength at which the player is propelled towards the hook")]
        protected float hookStrength = 10.0f;
        [SerializeField, Tooltip("The hook object")]
        protected SpriteRenderer hookChain;

        protected Vector3 startPos    = Vector3.zero;
        protected Vector2 hookHit     = Vector2.positiveInfinity;
        protected bool hooking        = false;
        protected bool isGrounded     = false;
        protected EntityState state   = EntityState.Idle;

        //This could depend on different weapons?
        [Header("Combat Stats")]
        [SerializeField, Tooltip("The attack damage of normal attacks")]
        protected float attackDamage = 1;
        [SerializeField, Tooltip("The melee attack range")]
        protected float attackRange = 0.6f;
        [SerializeField, Tooltip("The time between attacks")]
        protected float attackCooldown = 0.75f;
        [SerializeField, Tooltip("The throwable weapon to use")]
        protected internal Throwable throwable;

        protected bool canAttack = true;
        protected int throwAmmo = 0;
        protected internal float health;
        protected internal float currentHealth;
        public bool IsDead { get { return currentHealth <= 0f; } }

        //Input
        protected float xMove = 0f;
        protected int yMove = 0;
        protected bool jump;
        protected bool attack;
        protected bool shouldThrow;
        protected bool shouldHook;

        /// <summary>
        /// Start!
        /// </summary>
        private void Start()
        {
            startPos = transform.position;

            if (camera == null)
                camera = FindObjectOfType<Camera>();
            if (ground == null)
                ground = transform.GetChild(0);
            if (body == null)
                body = GetComponent<Rigidbody2D>();

            if (throwable != null)
                throwAmmo = throwable.startAmount;
            Debug.Log("Player Setup Events");
            //setup events
            LevelManager.OnLevelStart    += OnLevelStart;
            LevelManager.OnLevelFail     += OnLevelFail;
            LevelManager.OnLevelComplete += OnLevelComplete;
        }

        #region LevelEvents
        private void OnLevelStart()
        {
            Debug.Log("Player Level Start");
            active = true;
        }

        private void OnLevelFail()
        {
            Debug.Log("Player level Fail");
            active = false;
            state  = EntityState.Dead;
        }

        private void OnLevelComplete()
        {
            Debug.Log("Player level Complete");
            active = false;
        }
        #endregion

        #region FrameUpdates
        /// <summary>
        /// Input & mouse dependent stuff
        /// </summary>
            //TODO: make a good system for figuring out which way to look. probably velocity and wall check.
        private void Update()
        {
            if (!active)
                return;
            FaceMouse();
            GetInput();
            UpdateState();
            //just for testing
            if (Input.GetKeyDown(KeyCode.Escape))
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        /// <summary>
        /// Basically everything that should run after Update
        /// </summary>
        //TODO: Send data to animator
        private void LateUpdate()
        {
            if (!active)
                return;
            Debug.DrawLine(transform.position, transform.position + (Vector3)(directionToMouse * (attackRange + playerWidth /2f)), Color.blue);
        }

        /// <summary>
        /// Makes the player face towards the mouse
        /// </summary>
        [System.Obsolete("This doesnt make sense in the long run. really. Use X-Velocity and wall-detection a factor in look direction.")]
        private void FaceMouse()
        {
            var myX = transform.position.x;
            var mousePos = (Vector2) camera.ScreenToWorldPoint(Input.mousePosition);
            directionToMouse = (mousePos - (Vector2)transform.position).normalized;
            int newFacingDirection =  NormalizeInt(mousePos.x - myX);
            if (newFacingDirection == facingDirection)
                return;
            facingDirection = newFacingDirection;
            transform.localScale = new Vector3(facingDirection, 1.0f, 1.0f);
        }

        /// <summary>
        /// Gets relevant input
        /// </summary>
        //TODO: check for more input if needed
        private void GetInput()
        {
            xMove = Input.GetAxis("Horizontal");
            yMove = (int) Input.GetAxisRaw("Vertical");
            jump =  (Input.GetButtonDown("Jump") || jump) && isGrounded; //include grounded check lmao idk, kind redundant but it works
            attack = Input.GetButtonDown("LeftClick") || attack; //Maybe this should stay true until the action is performed
            shouldThrow = Input.GetButtonDown("MiddleClick") || shouldThrow;
            shouldHook  = Input.GetButton("RightClick");
        }
        
        /// <summary>
        /// Update the PlayerState
        /// </summary>
        //TODO: constant WIP, 1. Add Attack state
        private void UpdateState()
        {
            if (isGrounded)
            {
                if (xMove != 0 && !jump)
                    state = EntityState.Run;
                else if (jump)
                    state = EntityState.Jump;
                else 
                    state = EntityState.Idle;
            }
            else
            {
                if(body.velocity.y > 0)
                {
                    state = EntityState.Jump;
                }
                else if (body.velocity.y < 0)
                {
                    state = EntityState.Fall;
                }
            }
        }
        #endregion

        /// <summary>
        /// YEAH, SCIENCE, BITCH!
        /// </summary>
        private void FixedUpdate()
        {
            if (!active)
                return;
            CheckGrounded();
            Move();
            PerformActions();
        }

        #region PhysicsAndMovement
        /// <summary>
        /// Does what the name says lol
        /// </summary>
        private void CheckGrounded()
        {
            if(body.velocity.y > 2)
            {
                isGrounded = false;
                return;
            }
            RaycastHit2D hit;
            var raycastPos = transform.position;
            var rayLength = playerHeight / 2f + 0.05f;
            Debug.DrawRay(raycastPos, Vector3.down * rayLength, Color.red);
            if(hit = Physics2D.Raycast(raycastPos, Vector2.down, rayLength, 1))
            {
                SetGround(hit);
            }
            else
            {
                raycastPos += Vector3.right * playerWidth / 3f;
                Debug.DrawRay(raycastPos, Vector3.down * rayLength, Color.red);
                if (hit = Physics2D.Raycast(raycastPos, Vector2.down, rayLength, 1))
                {
                    SetGround(hit);
                }
                else
                {
                    raycastPos -= Vector3.right * playerWidth / 3f * 2f;
                    Debug.DrawRay(raycastPos, Vector3.down * rayLength, Color.red);
                    if (hit = Physics2D.Raycast(raycastPos, Vector2.down, rayLength, 1))
                    {
                        SetGround(hit);
                    }
                    else
                        isGrounded = false;
                }
            }
        }
        private void SetGround(RaycastHit2D hit)
        {
            Quaternion rot = Quaternion.LookRotation(Vector3.forward, hit.normal);
            ground.rotation = rot;
            isGrounded = true;
        }

        //TODO: Better movement handling. Accelerate and decelerate instead of instant movement speed changes
        /// <summary>
        /// Use the input to move the player character.
        /// </summary>
        private void Move()
        {
            if (isGrounded)
            {
                if (jump)
                    Jump();
                if (!Mathf.Approximately(xMove, 0f))
                {
                    //Accelerate in the correct direction
                    var xVelocity  = body.velocity.x;
                    var stepAccAbs = acceleration * Time.fixedDeltaTime * xMove;

                    var nextXVel = xVelocity + stepAccAbs;

                    ////positive movement
                    //if(xMove > 0)
                    //{
                    //    nextXVel = (nextXVel > targetSpeed) ? targetSpeed : nextXVel;
                    //}
                    ////negative movement
                    //else
                    //{
                    //    nextXVel = (nextXVel < -targetSpeed) ? -targetSpeed : nextXVel;
                    //}
                    body.velocity = new Vector2(nextXVel, body.velocity.y);
                    //Old movement logic. instead accelerate and decellerate please.
                    //var velocity = xMove * targetSpeed * (Vector2)ground.right;
                    //velocity.y = body.velocity.y;
                    //body.velocity = velocity;
                }
            }
            else
            {
                var airControl = xMove * acceleration * airControlStrength * Vector2.right;
                body.velocity += airControl * Time.fixedDeltaTime;
            }
        }

        /// <summary>
        /// physics fuck yeah. my brain hurts now.
        /// The player ALWAYS jumps jumpHeight units high no matter what.
        /// </summary>
        //! this is like the only thing that works nicely.
        private void Jump()
        {
            var jumpDir = ((Vector2)ground.up + Vector2.up).normalized;
            float g = 9.81f * body.gravityScale;
            float v0 = Mathf.Sqrt((jumpHeight) * (2 * g));
            var velocity = jumpDir * v0;
                velocity.x += body.velocity.x; //Test to see if this fixes some weird issues
            body.velocity = velocity;
            jump = false;
        }
        //TODO: rework wall jumping (limited times?)
        private void WallJump(Vector2 contactNormal)
        {
            var jumpDir = (contactNormal + Vector2.up).normalized;
            float g = 9.81f * body.gravityScale;
            float v0 = Mathf.Sqrt((jumpHeight) * (2 * g) * wallJumpStrength);
            var velocity = jumpDir * v0;
            body.velocity = velocity;
            jump = false;
        }
        //TODO: WIP wall check for walljump
        private void OnCollisionStay2D(Collision2D collision)
        {
            if (isGrounded)
                return;
            if (collision.collider.CompareTag("Enemy"))
                return;
            var normal = collision.contacts[0].normal;
            if (Mathf.Abs(normal.x) > 0.8)
                if (Input.GetButtonDown("Jump"))
                    WallJump(normal);
        }
        #endregion

        #region CombatCode
        //? improve?
        private void PerformActions()
        {
            if (shouldThrow)
                Throw();
            if (attack && canAttack)
                Attack();
            if (shouldHook && !hooking)
                StartCoroutine(DoHook());
        }

        //TODO: Hooking feels weird. Maybe (Distance) Physics Joints could be an answer?
        private IEnumerator DoHook()
        {
            hooking = true;
            //1. try to find location to hook to
            RaycastHit2D hit;
            if (hit = Physics2D.Raycast(transform.position, directionToMouse, hookRange, 1))
            {
                hookHit = hit.point;
            }
            else
            {
                yield return new WaitForSeconds(0.2f);
                hooking = false;
                yield break; //STOP, THIS VIOLATES THE LAW
            }
            //2. fire hook
            hookChain.gameObject.SetActive(true);
            yield return ShootHook();
            //3. move towards the hookHit position
            //TODO: figure out a better solution to this garbage
            var velocity = Vector2.zero;
            while(shouldHook)
            {
                if ((hookHit - (Vector2)transform.position).magnitude > hookRange)
                {
                    BreakChain();
                    yield break;
                }
                var direction = (hookHit - (Vector2)transform.position).normalized;
                //body.AddForce(direction * hookStrength * body.mass, ForceMode2D.Force);
                velocity += direction * hookStrength * Time.fixedDeltaTime;
                velocity = (velocity.magnitude > hookStrength) ? velocity.normalized * hookStrength : velocity;
                body.velocity = velocity;
                UpdateChain();
                yield return null;
            }
            BreakChain();
        }

        //TODO: this is fucking broken, i dont know how but it just fucks up.
        private IEnumerator ShootHook()
        {
            Vector2 ch = hookHit - (Vector2)transform.position;
            float totalDist = ch.magnitude;
            float reqTime = totalDist / hookSpeed;
            for (float t = 0f; t < 1; t += Time.deltaTime / reqTime)
            {
                //now do the adjusting
                hookChain.transform.position = (Vector2)transform.position + ch * t / 2f;
                hookChain.size = new Vector2(1f, t * totalDist);
                hookChain.transform.rotation = Quaternion.LookRotation(Vector3.forward, -ch.normalized);

                //recalculate this shit every single time ugh
                ch = hookHit - (Vector2)transform.position;
                totalDist = ch.magnitude;
                reqTime = totalDist / hookSpeed;
                yield return null;
            }
        }

        //Update the hooks size and that.
        private void UpdateChain()
        {
            var ch = hookHit - (Vector2)transform.position;
            hookChain.transform.position = Vector2.Lerp(transform.position, hookHit, 0.5f);
            hookChain.size = new Vector2(1f, ch.magnitude );
            hookChain.transform.rotation = Quaternion.LookRotation(Vector3.forward, -ch.normalized);
        }

        private void BreakChain()
        {
            hookHit = Vector2.positiveInfinity;
            hooking = false;
            hookChain.gameObject.SetActive(false);
        }

        /// <summary>
        /// Temporary Attack Code
        /// </summary>
        //TODO: This way of attacking is absolute dogshit. Make it play an attack animation and work with trigger colliders for hit detection.
        private void Attack()
        {
            canAttack = false;
            attack = false;
            Debug.Log("Attack!");
            RaycastHit2D hit;
            if(hit = Physics2D.Raycast(transform.position, directionToMouse, attackRange + (playerWidth/2)))
            {
                if(hit.transform.CompareTag("Enemy"))
                    hit.transform.GetComponent<EnemyBase>().ProcessHit(new AttackHitData(attackDamage, hit.point));
            }

            StartCoroutine(ResetAttack());
        }
        //TODO: Make a Slider or some indicator for the attack cooldown (for-yield loop)
        private IEnumerator ResetAttack()
        {
            yield return new WaitForSeconds(attackCooldown);
            canAttack = true;
        }

        //TODO: Add Throwable Items
        /// <summary>
        /// Throw the equipped thing
        /// </summary>
        private void Throw()
        {
            if (throwAmmo <= 0)
                return;
            var velocity = throwable.startVelocity * directionToMouse;
            var obj = Instantiate(throwable.prefab, transform.position, Quaternion.identity, null);
            var throwBody = obj.GetComponent<Rigidbody2D>();
            throwBody.velocity = velocity;
            throwAmmo--;
            shouldThrow = false;
            state = EntityState.Throwing;
        }

        //TODO: process knockback and that kind of stuff.
        /// <summary>
        /// Process a hit
        /// </summary>
        /// <param name="hitData"></param>
        internal override void ProcessHit(AttackHitData hitData)
        {
            currentHealth -= hitData.Damage;
        }
        internal override void ProcessHit(AttackHitData hitData, bool isOnlyDamage)
        {
            if (isOnlyDamage)
                currentHealth -= hitData.Damage;
            else
                ProcessHit(hitData);
        }

        #endregion

        /// <summary>
        /// Reset the Player to his start values.
        /// </summary>
        internal override void ResetEntity()
        {
            transform.position = startPos;
            body.velocity = Vector2.zero;
            if (throwable != null)
                throwAmmo = throwable.startAmount;
        }
    }
}