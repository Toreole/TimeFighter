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
            float MaxAdhereDistance => controllerSettings.maxAdhereDist;
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
        protected bool isOnWall  = false;
        protected Vector2 wallNormal = Vector2.right;
        protected bool isOnLedge = false;
        protected bool doLedgeCheck = true;
        protected Vector2 ledgePosition = Vector2.zero;

        private const float raycastError = 0.05f;
        private const float g = 9.81f;

        /// <summary>
        /// Start! bruh unity gae
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
            jump   = (Input.GetButtonDown("Jump") || jump) && (isGrounded || isOnWall || isOnLedge); //include grounded check lmao idk, kind redundant but it works
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
                //!this is nicer code now yay
                selectedAction = (selectedAction + actions.Count) % actions.Count;
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
            //if (attack && canAttack)
            //    Attack();

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
            //bruh
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
            CheckWall();
            CheckLedge();
            Move();
        }

        #region PhysicsAndMovement
        /// <summary>
        /// Does what the name says lol
        /// </summary>
        protected void CheckGrounded()
        {
            if(body.velocity.y > 2)
            {
                isGrounded = false;
                return;
            }
            RaycastHit2D hit;
            var raycastPos = transform.position;
            var rayLength = PlayerHeight / 1.5f + raycastError;

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
        protected void SetGround(RaycastHit2D hit)
        {
            Quaternion rot = Quaternion.LookRotation(Vector3.forward, hit.normal);
            ground.rotation = rot;
            isGrounded = true;
            var nPoint = hit.point + Vector2.up * PlayerHeight / 2f;
            var dist = Vector2.Distance(nPoint, transform.position);
            //stick to the ground instead of flying off
            if (dist < MaxAdhereDistance && dist > 0.05f)
            {
                body.position = nPoint;
            }
        }

        /// <summary>
        /// Check for a wall for walljumps.
        /// </summary>
        protected void CheckWall()
        {
            if (isGrounded)
            {
                isOnWall = false;
                return;
            }
            var dist = PlayerWidth / 2f + raycastError;
            Debug.DrawRay(transform.position, Vector3.right * dist, Color.blue);
            Debug.DrawRay(transform.position, -Vector3.right * dist, Color.blue);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, dist, groundLayer);
            if (hit)
            {
                //bruh
                wallNormal = hit.normal;
                isOnWall = true;
            }
            else if(hit = Physics2D.Raycast(transform.position, -Vector2.right, dist, groundLayer))
            {
                wallNormal = hit.normal;
                isOnWall = true;
            }
            else
            {
                isOnWall = false;
            }
        }

        /// <summary>
        /// check for a ledge to hang onto. presumably only in the direction the player is facing.
        /// </summary>
        protected void CheckLedge()
        {
            if (isGrounded || !doLedgeCheck)
                return;
            Vector2 topCornerOffset = new Vector2(PlayerWidth / 2f * transform.localScale.x, PlayerHeight / 2f);
            Vector2 rayOffset = Vector2.right * (PlayerWidth / 4f * transform.localScale.x) + Vector2.up * (PlayerHeight / 4f);
            Vector2 rayOrigin = this.Position + rayOffset + topCornerOffset;
            float rayDistance = PlayerHeight / 4f;

            Debug.DrawRay(rayOrigin, Vector3.down * rayDistance, Color.red);
            RaycastHit2D hit;
            if(hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayDistance, groundLayer))
            {
                isOnLedge = true;
                ledgePosition = hit.point;
                //Try to move closer to the ledge: 
                //1. towards "wall"
                //2. allign top of the player with detected edge.
            }
            else
            {
                isOnLedge = false;
            }
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

            //Ledge hang behaviour
            if (isOnLedge)
            {
                LedgeBehaviour();
                return;
            }
            //normal behaviour
            body.gravityScale = (isOnWall && body.velocity.y <= 0f) ? 0.3f : 1f;

            //TODO: this part of the code is pretty dang ugly, fix that please!
            if (isGrounded)
            {
                //Prevent slopes from interfering with movement.
                body.AddForce(-GetGroundForce());
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

            //jump last.
            if (jump)
                Jump();
        }
        
        /// <summary>
        /// How the player should behave when hanging onto a ledge
        /// </summary>
        protected void LedgeBehaviour()
        {
            body.gravityScale = 0f;
            if (body.velocity.y <= 0.1f && body.velocity.magnitude < 2f)
                body.velocity = Vector2.zero;
            if (jump)
                ClimbLedge();
            if (yMove < 0)
            {
                doLedgeCheck = false;
                isOnLedge = false;
                Delay(this, () => { doLedgeCheck = true; }, 0.5f);
            }
            DrawCross(ledgePosition, 0.2f);
        }

        /// <summary>
        /// Do a dash lol
        /// </summary>
        protected IEnumerator DoDash()
        {
            if (actions[selectedAction].IsPerforming)
                actions[selectedAction].CancelAction();
            dashing = true;
            isInvincible = true;
            var dashTime = 0.2f;
            var newVel = directionToMouse * (DashLength / dashTime);
            body.velocity = newVel;
            body.gravityScale = 0f;
            yield return new WaitForSeconds(dashTime);
            body.gravityScale = 1f;
            body.velocity = newVel / 2f;
            dashing = false;
            isInvincible = false;

            if (!isRecharging)
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
            jump = false;
            var jumpDir = isOnWall? (wallNormal + Vector2.up).normalized : ((Vector2)ground.up + Vector2.up).normalized;
            float v0 = Mathf.Sqrt((JumpHeight) * (2 * g) * (isOnWall? WallJumpStrength : 1f));
            var velocity = jumpDir * v0;
                velocity.x += body.velocity.x; //Test to see if this fixes some weird issues
            body.velocity = velocity;
        }

        /// <summary>
        /// Climb the ledge youre holding onto 
        /// </summary>
        //TODO: This could be nicer, but it works. (2 physics frames long climb)
        private void ClimbLedge()
        {
            var finalPos = ledgePosition + Vector2.up * PlayerHeight / 2f;
            var tempPos = new Vector2(body.position.x, finalPos.y);
            body.MovePosition(tempPos);

            DelayPhysicsFrame(this, () => 
            {
                body.MovePosition(finalPos);
                jump = false;
            });
            //float v0 = Mathf.Sqrt((JumpHeight) * (2 * g));
            //body.velocity = new Vector2(0, v0);
        }
        #endregion

        #region CombatCode

        //TODO: process knockback and that kind of stuff.
        /// <summary>
        /// Process a hit.
        /// </summary>
        /// <param name="hitData"></param>
        public override void ProcessHit(AttackHitData hitData)
        {
            if(!isInvincible)
                currentHealth -= hitData.Damage;
        }
        public override void ProcessHit(AttackHitData hitData, bool isOnlyDamage)
        {
            if (isOnlyDamage && !isInvincible)
                currentHealth -= hitData.Damage;
            else
                ProcessHit(hitData);
        }

        //TODO: Add Throwable Items
        //TODO: also make this an action instead of this lol.
        /// <summary>
        /// Throw the equipped thing
        /// </summary>
        private void Throw()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Temporary Attack Code
        /// </summary>
        //TODO: This is absolute dogshit. Make it play an attack animation and work with trigger colliders for hit detection.
        private void Attack()
        {
            throw new NotImplementedException();
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
        [SerializeField, Tooltip("The max distance to stick to the ground")]
        internal float maxAdhereDist;

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
                maxAdhereDist = 0.1f
            };
        
    }
}