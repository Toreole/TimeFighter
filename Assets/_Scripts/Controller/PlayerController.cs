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
        [Header("Camera Stuff")]
        [SerializeField]
        protected new Camera camera = null;
        protected int facingDirection = 1;
        protected Vector2 directionToMouse = Vector2.zero;
        
        [Header("Physics And Movement")]
        [SerializeField, Tooltip("The empty transform child, will be automatically set in Start()")]
        protected Transform ground;
        [SerializeField, Tooltip("The collision layer used to check for ground")]
        protected LayerMask groundLayer;
        [SerializeField, Tooltip("Settings for movement and that")]
        protected PlayerControllerSettings controllerSettings = PlayerControllerSettings.DefaultSettings;
        #region setting_properties
            float PlayerWidth  => controllerSettings.playerWidth;
            float Acceleration => controllerSettings.acceleration;
            float PlayerHeight => controllerSettings.playerHeight;
            float JumpHeight   => controllerSettings.jumpHeight;
            float TargetSpeed  => controllerSettings.targetSpeed;
            float WallJumpStrength   => controllerSettings.wallJumpStrength;
            float AirControlStrength => controllerSettings.airControlStrength;
            int   MaxDashCharges => controllerSettings.dashCharges;
            float DashCooldown => controllerSettings.dashCooldown;
            float DashLength => controllerSettings.dashLength;
        #endregion

        //This could depend on different weapons?
        [Header("Combat Stats")]
        [SerializeField, Tooltip("Throw controller")]
        protected ThrowController throwController;
        [SerializeField, Tooltip("The attack damage of normal attacks")]
        protected float attackDamage = 1;
        [SerializeField, Tooltip("The melee attack range")]
        protected float attackRange = 0.6f;
        [SerializeField, Tooltip("The time between attacks")]
        protected float attackCooldown = 0.75f;

        [Header("UI")]
        [SerializeField]
        protected PlayerUIManager uIManager;

        protected bool canAttack = true;
        protected internal float maxHealth; //TODO: make this actual health.
        public bool IsDead { get { return currentHealth <= 0f; } }

        //Input
        protected float xMove = 0f;
        protected float yMove = 0;
        protected bool jump;
        protected bool attack;
        protected bool shouldThrow;
        protected float mouseScroll;
        protected bool actionPersist;
        protected bool frameAction;
        protected bool dash;

        //Actions
        protected int selectedAction;
        protected float actionOffset;
        protected int dashCharges;
        protected bool dashing = false;
        protected bool isRecharging = false;
        protected float remainDashCd = 0f;

        public float RelativeDashCD => remainDashCd / DashCooldown;
        public int AvailableDashes => dashCharges;

        //other runtime variables
        protected Vector3 startPos = Vector3.zero;
        protected bool isGrounded = false;
        protected EntityState state = EntityState.Idle;
        
        /// <summary>
        /// Start!
        /// </summary>
        protected override void Start()
        {
            base.Start();
            IsPlayer = true;
            startPos = transform.position;
            dashCharges = MaxDashCharges;

            if (camera == null)
                camera = FindObjectOfType<Camera>();
            if (ground == null)
                ground = transform.GetChild(0);
            if (body == null)
                body = GetComponent<Rigidbody2D>();
            if (uIManager == null)
                uIManager = FindObjectOfType<PlayerUIManager>();
            actions = new List<BaseAction>(GetComponents<GenericHook>());
            if (actions.Count > 0)
                uIManager.SetAction(actions[0]);
            //setup events
            LevelManager.OnLevelStart    += OnLevelStart;
            LevelManager.OnLevelFail     += OnLevelFail;
            LevelManager.OnLevelComplete += OnLevelComplete;
        }

        #region LevelEvents
        protected override void OnLevelStart()
        {
            Debug.Log("Player Level Start");
            active = true;
        }

        protected override void OnLevelFail()
        {
            Debug.Log("Player level Fail");
            active = false;
            state  = EntityState.Dead;
        }

        protected override void OnLevelComplete()
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
            //FaceMouse();
            GetInput();
            UpdateState();
            PerformActions();
        }

        /// <summary>
        /// Basically everything that should run after Update
        /// </summary>
        //TODO: Send data to animator
        private void LateUpdate()
        {
            if (!active)
                return;
            Debug.DrawLine(transform.position, transform.position + (Vector3)(directionToMouse * (attackRange + PlayerWidth /2f)), Color.blue);
        }

        /// <summary>
        /// Gets relevant input
        /// </summary>
        //TODO: check for more input if needed
        private void GetInput()
        {
            xMove  = Input.GetAxis("Horizontal");
            yMove  = Input.GetAxis("Vertical");
            jump   = (Input.GetButtonDown("Jump") || jump) && isGrounded; //include grounded check lmao idk, kind redundant but it works
            attack = Input.GetButtonDown("LeftClick") || attack; //Maybe this should stay true until the action is performed
            shouldThrow   = Input.GetButtonDown("MiddleClick") || shouldThrow;
            actionPersist = Input.GetButton("RightClick");
            frameAction   = Input.GetButtonDown("RightClick");
            dash = Input.GetButtonDown("Dash");

            //Swap actions
            //TODO: have all actions displayed at once, animate the swapping on the UI
            if(Input.GetButtonDown("ActionSwap") && !actions[selectedAction].IsPerforming)
            {
                selectedAction += (int)Input.GetAxisRaw("ActionSwap");
                selectedAction = (selectedAction < 0) ? actions.Count - 1: (selectedAction >= actions.Count)? 0 : selectedAction;
                uIManager.SetAction(actions[selectedAction]);
            }

            //mouse position
            var mousePos = (Vector2)camera.ScreenToWorldPoint(Input.mousePosition);
            directionToMouse = (mousePos - (Vector2)transform.position).normalized;
        }

        //TODO: rework attacking
        private void PerformActions()
        {
            if (dash && dashCharges > 0 && !dashing)
            {
                dashCharges--;
                dash = false;
                StartCoroutine(DoDash());
                return;
            }
            if (attack && canAttack)
                Attack();

            var act = actions[selectedAction];
            if (frameAction && act.CanPerform)
            {
                act.TargetDirection = directionToMouse;
                frameAction = false;
                act.PerformAction();
            }
            act.ShouldPerform = actionPersist;
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
            var rayLength = PlayerHeight / 2f + 0.05f;
            Debug.DrawRay(raycastPos, Vector3.down * rayLength, Color.red);
            if (hit = Physics2D.Raycast(raycastPos, Vector2.down, rayLength, groundLayer))
            {
                SetGround(hit);
            }
            else
            {
                raycastPos += Vector3.right * PlayerWidth / 3f;
                Debug.DrawRay(raycastPos, Vector3.down * rayLength, Color.red);
                if (hit = Physics2D.Raycast(raycastPos, Vector2.down, rayLength, groundLayer))
                {
                    SetGround(hit);
                }
                else
                {
                    raycastPos -= Vector3.right * PlayerWidth / 3f * 2f;
                    Debug.DrawRay(raycastPos, Vector3.down * rayLength, Color.red);
                    if (hit = Physics2D.Raycast(raycastPos, Vector2.down, rayLength, groundLayer))
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

        //! I fixed the movement partially. Further Improve this later on.
        /// <summary>
        /// Use the input to move the player character.
        /// </summary>
        private void Move()
        {
            //adjust the curent "look direction"
            var xS = (Mathf.Abs(body.velocity.x) < 0.1f)? transform.localScale.x : Normalized(body.velocity.x);
                transform.localScale = new Vector3(xS, 1f, 1f);
            if (dashing)
                return;
            if (isGrounded)
            {
                //Prevent slopes from interfering with movement.
                body.AddForce(-GetGroundForce());
                if (jump)
                    Jump();
                if (!Mathf.Approximately(xMove, 0f))
                {
                    //move left/right
                    var vel = body.velocity;
                    var stepAcc = Acceleration * (Vector2)ground.right * Time.fixedDeltaTime * xMove;
                    var nextVel = vel + stepAcc;
                    if (nextVel.magnitude > TargetSpeed && nextVel.magnitude > vel.magnitude)
                        nextVel = vel - stepAcc;
                    body.velocity = nextVel;
                }
            }
            else
            {
                //air movement
                var airControl = xMove * Acceleration * AirControlStrength * Vector2.right;
                body.velocity += airControl * Time.fixedDeltaTime;
            }
        }

        /// <summary>
        /// Do a dash lol
        /// </summary>
        protected IEnumerator DoDash()
        {
            if (actions[selectedAction].IsPerforming)
                actions[selectedAction].CancelAction();
            dashing = true;
            var dashTime = 0.2f;
            var oldVelocity = body.velocity;
            var newVel = directionToMouse * (DashLength / dashTime);
            body.velocity = newVel;
            var oldGravity = body.gravityScale;
            body.gravityScale = 0f;
            yield return new WaitForSeconds(dashTime);
            body.gravityScale = oldGravity;
            body.velocity = newVel / 2f;
            dashing = false;

            if(!isRecharging)
                StartCoroutine(RechargeDash());
        }
        
        /// <summary>
        /// recharge dashes if needed
        /// </summary>
        protected IEnumerator RechargeDash()
        {
            if (isRecharging)
                yield break;
            isRecharging = true;
            while( dashCharges < MaxDashCharges )
            {
                for(remainDashCd = DashCooldown; remainDashCd > 0f; remainDashCd -= Time.deltaTime)
                {
                    yield return null;
                }
                dashCharges++;
            }
            isRecharging = false;
        }

        /// <summary>
        /// The resulting force of the (sloped) ground and gravity, that accelerates the body towards the lower ground.
        /// </summary>
        protected Vector2 GetGroundForce()
        {
            if (ground.up.y > 0.95)
                return Vector2.zero;
            var Fg = body.gravityScale * body.mass * Physics2D.gravity;
            var groundNormal = ground.up;
            var alpha = Vector2.Angle(groundNormal, -Fg);
            var sinAlpha = Mathf.Sin(alpha * Mathf.Deg2Rad);
                //Debug.Log(alpha + "-- sin: " + sinAlpha);
            var groundOffsetDirection = (groundNormal.x >= 0) ? ground.right: -ground.right;
            var Fh = groundOffsetDirection * sinAlpha * Fg.magnitude;
            return Fh;
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
            float v0 = Mathf.Sqrt((JumpHeight) * (2 * g));
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
            float v0 = Mathf.Sqrt((JumpHeight) * (2 * g) * WallJumpStrength);
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

        //TODO: process knockback and that kind of stuff.
        /// <summary>
        /// Process a hit
        /// </summary>
        /// <param name="hitData"></param>
        public override void ProcessHit(AttackHitData hitData)
        {
            currentHealth -= hitData.Damage;
        }
        public override void ProcessHit(AttackHitData hitData, bool isOnlyDamage)
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
        }

        #region ObsoleteCode

        //TODO: Add Throwable Items
        //TODO: also make this an action instead of this lol.
        /// <summary>
        /// Throw the equipped thing
        /// </summary>
        private void Throw()
        {
            //if (throwAmmo <= 0)
            //    return;
            //var velocity = throwable.startVelocity * directionToMouse;
            //var obj = Instantiate(throwable.prefab, transform.position, Quaternion.identity, null);
            //var throwBody = obj.GetComponent<Rigidbody2D>();
            //throwBody.velocity = velocity;
            //throwAmmo--;
            //shouldThrow = false;
            //state = EntityState.Throwing;
        }

        /// <summary>
        /// Temporary Attack Code
        /// </summary>
        //TODO: This is absolute dogshit. Make it play an attack animation and work with trigger colliders for hit detection.
        [System.Obsolete("This is garbage lol")]
        private void Attack()
        {
            canAttack = false;
            attack = false;
            Debug.Log("Attack!");
            RaycastHit2D hit;
            if (hit = Physics2D.Raycast(transform.position, directionToMouse, attackRange + (PlayerWidth / 2)))
            {
                var damageable = hit.transform.GetComponent<IDamageable>();
                if (damageable != null)
                    damageable.ProcessHit(new AttackHitData(attackDamage, hit.point));
            }

            StartCoroutine(ResetAttack());
        }

        //TODO: Make a Slider or some indicator for the attack cooldown (for-yield loop)
        private IEnumerator ResetAttack()
        {
            yield return new WaitForSeconds(attackCooldown);
            canAttack = true;
        }

        /// <summary>
        /// Makes the player face towards the mouse
        /// </summary>
        [System.Obsolete("This doesnt make sense in the long run. really. Use X-Velocity and wall-detection a factor in look direction.")]
        private void FaceMouse()
        {
            var myX = transform.position.x;
            var mousePos = (Vector2)camera.ScreenToWorldPoint(Input.mousePosition);
            directionToMouse = (mousePos - (Vector2)transform.position).normalized;
            int newFacingDirection = NormalizeInt(mousePos.x - myX);
            if (newFacingDirection == facingDirection)
                return;
            facingDirection = newFacingDirection;
            transform.localScale = new Vector3(facingDirection, 1.0f, 1.0f);
        }

        #endregion
    }

    [System.Serializable]
    public struct PlayerControllerSettings
    {
        [SerializeField, Tooltip("height of the players hitbox")]
        internal float playerHeight;
        [SerializeField, Tooltip("width of the players hitbox")]
        internal float playerWidth;
        [SerializeField, Tooltip("The fixed height the player can jump (in Units).")]
        internal float jumpHeight;
        [SerializeField, Tooltip("how fast the player usually moves on the ground.")]
        internal float targetSpeed;
        [SerializeField, Tooltip("how fast the speed of the player changes while on ground.")]
        internal float acceleration;
        [SerializeField, Tooltip("how fast should the player change momentum while mid-air")]
        internal float airControlStrength;
        [SerializeField, Tooltip("strength of wall jumps")]
        internal float wallJumpStrength;
        [SerializeField, Tooltip("length of the dash")]
        internal float dashLength;
        [SerializeField, Tooltip("dash cooldown")]
        internal float dashCooldown;
        [SerializeField, Tooltip("amount of max charges")]
        internal int dashCharges;

        public static PlayerControllerSettings DefaultSettings 
            => new PlayerControllerSettings()
            {
                playerHeight = 0.8f,
                playerWidth = 0.8f,
                jumpHeight = 1.0f,
                targetSpeed = 1.0f,
                acceleration = 2.0f,
                airControlStrength = 0.25f,
                wallJumpStrength = 0.75f,
                dashLength = 3.5f,
                dashCooldown = 2.5f,
                dashCharges = 2,
            };
        
    }
}