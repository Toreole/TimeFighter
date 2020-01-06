using UnityEngine;
using System.Collections;
using Luminosity.IO;

using static Game.Util;

namespace Game.Controller
{
    public class PlayerController : PlayerComponent
    {
        //Serialized, not to be changed during runtime.
        //[SerializeField]
        //protected PlayerStateBehaviour defaultState;
        [Header("Player Settings")]
        [SerializeField]
        protected Animator animator;
        [SerializeField]
        protected float baseSpeed = 3.5f;
        [SerializeField]
        protected float acceleration = 2.5f;
        [SerializeField, Range(-100f, 0f)] //TODO: roll on ender ground state. roll with large x speed but low y?
        protected float rollFallThreshold = -5f, fallDamageThreshold = -16f;
        [SerializeField]
        protected float maxStamina = 100, staminaRegen = 20f;
        [SerializeField] //TODO: everything around dashing in the state behaviours
        protected float dashDistance = 4f, dashCost = 25f, dashSpeed = 9f;
        [SerializeField]
        protected float maxSteepAngle = 37.5f, slipThreshold = 0.25f;
        [SerializeField]
        protected float jumpHeight = 2.5f, airJumpHeight = 1.75f;
        [SerializeField]
        protected LayerMask groundMask;
        [SerializeField]
        protected float halfHeight = 0.5f, halfWidth = 0.5f;
        [SerializeField]
        protected float groundedTolerance = 0.1f;
        [SerializeField]
        protected int airJumps = 1;
        [SerializeField, Header("Rendering")]
        protected new SpriteRenderer renderer;
        
        //public settings properties.
        public float BaseSpeed => baseSpeed;
        public float BaseSpeedSqr { get; protected set; }
        public float JumpHeight => jumpHeight;
        public float AirJumpHeight => airJumpHeight;
        public float Acceleration => acceleration;
        public float MaxSteepAngle => maxSteepAngle;
        public float SlipThreshold => slipThreshold;
        public int AvailableAirJumps 
        { get => availableAirJumps; set => availableAirJumps = Mathf.Clamp(value, 0, airJumps); }
        public bool CanAirJump 
        { get => availableAirJumps > 0; set { availableAirJumps = value ? airJumps : 0; } }
        public float Stamina 
        { get => stamina; set => stamina = Mathf.Clamp(value, 0f, maxStamina); }
        public float StaminaRegen 
        { get => staminaRegen; protected set => staminaRegen = value; }
        public float DashDistance 
        { get => dashDistance; }
        public float DashCost 
        { get => dashCost; }
        public float DashSpeed 
        { get => dashSpeed; }
        public float RollFallThreshold => rollFallThreshold;
        public float FallDamageThreshold => fallDamageThreshold;

        public bool FlipX { get => renderer.flipX; set => renderer.flipX = value; }

        //Active State Controls
        PlayerStateBehaviour activeState;
        bool overrideStateControls = false;

        //Input Buffer
        Vector2 movementInput, movementRaw;
        bool jumpPressed, jumpHold;
        bool specialA, specialB;
        bool attackA, attackB;
        public Vector2 MoveInput => movementInput;
        public Vector2 MoveInputRaw => movementRaw;

        //other variables
        protected bool isGrounded = true;
        protected bool stickToGround = true;
        protected bool ignorePlayerInput = false;
        protected int availableAirJumps;
        protected float stamina;

        protected ContactPoint2D[] contactPoints = new ContactPoint2D[16];
        protected ContactFilter2D filter;

        //other properties
        public bool IsGrounded {
            get => isGrounded;
            set
            {
                if (!IsGrounded && value)
                    OnEnterGround?.Invoke();
                else if (IsGrounded && !value)
                    GroundLeaveOnNextFrame();
                isGrounded = value;
            }
        }
        protected Vector2 groundNormal = Vector2.up;
        public Vector2 GroundNormal => groundNormal;
        public float GroundFriction 
        { get; protected set; } = 0.4f;
        public bool  StickToGround 
        { get => stickToGround; set => stickToGround = value; }
        public bool  IgnorePlayerInput 
        { get => ignorePlayerInput; set { ignorePlayerInput = value; } }
        public Vector2 LastVel 
        { get; protected set; }

        //State callback events.
        #region events
        public delegate void ControllerCallback();

        public event ControllerCallback OnPressJump;
        public event ControllerCallback OnHoldJump;

        public event ControllerCallback OnSpecialA;
        public event ControllerCallback OnSpecialB;

        public event ControllerCallback OnAttackA;
        public event ControllerCallback OnAttackB;

        public event ControllerCallback OnTakeDamage;

        public event ControllerCallback OnEnterGround;
        public event ControllerCallback OnLeaveGround;
        #endregion

        /// <summary>
        /// Initial Setup
        /// </summary>
        private void Start()
        {
            availableAirJumps = airJumps;
            BaseSpeedSqr = baseSpeed * baseSpeed;
            stamina = maxStamina;

            filter = new ContactFilter2D
            {
                layerMask = groundMask,
                useLayerMask = true
            };

            activeState = new GroundedPlayerState
            {
                controller = this
            };
            activeState.OnEnterState();
        }

        /// <summary> 
        /// Update Loop
        /// </summary>
        private void Update()
        {
            CheckGround();
            FetchInput();
            RunCallbacks();
            //Debug.Log("Using Controller: " + InputManager.PreferController); //! only testing 
        }
        /// <summary>
        /// Fixed update callbacks 
        /// </summary>  
        private void FixedUpdate()
        {
            RunFixedCallbacks();
            LastVel = Body.velocity;
        }

        /// <summary>
        /// Get the input for this frame.
        /// </summary>
        void FetchInput()
        { 
            if(ignorePlayerInput)
            {
                movementInput = Vector2.zero;
                jumpHold = false;
                jumpPressed = false;
                return;
            }
            movementInput.x = InputManager.GetAxis("Horizontal");
            movementInput.y = InputManager.GetAxis("Vertical");
            movementRaw.x   = InputManager.GetAxisRaw("Horizontal");
            movementRaw.y   = InputManager.GetAxisRaw("Vertical");
            jumpPressed     = InputManager.GetButtonDown("Jump");
            jumpHold        = InputManager.GetButton("Jump");
        }

        /// <summary>
        /// Checks for ground in a more or less arbitrary way.
        /// </summary>
        protected void CheckGround()
        {
            //RayCastGroundA();
            CheckContactsForGround();
        }
        
        /// <summary>
        /// Checks for ground using the rigidbody contacts, or a raycast to handle slopes
        /// </summary>
        //TODO: searching the contacts should also include checking for a wall i guess.
        protected void CheckContactsForGround()
        {
            int contacts = Body.GetContacts(contactPoints);
            if (contacts == 0)
            {
                IsGrounded = DoGroundStick();
                return;
            }
            Vector2 normals = Vector2.zero;
            float frictions = 0f;
            for (int i = 0; i < contacts && i < 16; i++)
            {
                var contact = contactPoints[i];
                if (contact.normal.y > 0.4)
                {
                    normals += contact.normal;
                    frictions += contact.collider.friction;
                }
            }
            if (normals.Equals(Vector2.zero))
            {
                //Debug.Log("no normals");
                IsGrounded = false;
                return;
            }
            //WOULD BE FUCKING COOL IF UNITY COULD JUST FUCKING COMPILE
            GroundFriction = frictions / contacts;

            normals.Normalize();
            groundNormal = normals;
            IsGrounded = true;
        }

        /// <summary>
        /// Additional check for ground, keeps the player connected to terrain on slope starts
        /// </summary>
        /// <returns></returns>
        protected bool DoGroundStick()
        {
            if (!StickToGround)
            {
                //Debug.Log("Dont stick");
                return false;
            }
            //Debug.DrawRay(Body.position, Vector2.down * (halfHeight + groundedTolerance), Color.red);
            float xOffset = Body.velocity.x * Time.deltaTime;
            Vector2 rayOrigin = Body.position + new Vector2(xOffset, 0f);
            
            //TODO its not 100% accurate at times.
            RaycastHit2D hit2D;
            if (hit2D = Physics2D.CircleCast(rayOrigin, halfHeight, Vector2.down, groundedTolerance, groundMask))
            {
                float oldVel = Body.velocity.magnitude;

                var pos = Body.position;
                //If there is a big enough change in surface angle, adjust the velocity of the player to walk along it
                if (Vector2.Dot(groundNormal, hit2D.normal) < 0.95)
                {
                    float alpha = Vector2.SignedAngle(groundNormal, hit2D.normal);
                    Body.velocity = RotateVector2D(Body.velocity, alpha);
                }
                pos.y = hit2D.centroid.y;
                Body.position = pos;
            }
            return hit2D;
        }

        ///The first solution for finding ground i tried out.
        protected void RayCastGroundA()
        {
#if UNITY_EDITOR
            Debug.DrawRay(Body.position, Vector2.down * (halfHeight + groundedTolerance), Color.red);
#endif
            //TODO: test out with multi raycast again 
            Vector2 rayOrigin = Body.position;
            RaycastHit2D hit2D;
            if (hit2D = Physics2D.Raycast(rayOrigin, Vector2.down, halfHeight + groundedTolerance, groundMask))
            {
                GroundFriction = hit2D.collider.friction;
                groundNormal = hit2D.normal;
                IsGrounded = true;
                //If necessary, "stick" to the ground.
                if (stickToGround)
                {
                    Vector2 newPos = Body.position;
                    newPos.y = hit2D.point.y + halfHeight;
                    Body.position = newPos;
                }
                return;
            }

            //If that failed, do a box cast
            Vector2 boxCenter = Body.position - Vector2.up * halfHeight;
            Vector2 boxSize = new Vector2(halfWidth * 0.6f, groundedTolerance);
            if (Physics2D.OverlapBox(boxCenter, boxSize, 0f, groundMask))
            {
                //? maybe stick to the ground using the data in here? but how tho?
                IsGrounded = true;
                return;
            }
            IsGrounded = false;
        }
        /// <summary>
        /// Delays the groundLeave callback until the next frame and checks if it is still valid at that point.
        /// </summary>
        protected void GroundLeaveOnNextFrame()
        {
            StartCoroutine(DelayCheck());
            IEnumerator DelayCheck()
            {
                yield return null;
                if (!IsGrounded)
                    OnLeaveGround?.Invoke();
            }
        }
        /// <summary>
        /// Run the callbacks based on input n such i guess.
        /// </summary>
        protected void RunCallbacks()
        {
            if (jumpPressed)
                OnPressJump?.Invoke();
            if (jumpHold)
                OnHoldJump?.Invoke();
        }

        protected void RunFixedCallbacks()
        {
            //can just be done here since its only using this input in here. - Fixes the issue with stopping after roll
            movementInput.x = InputManager.GetAxis("Horizontal");
            movementInput.y = InputManager.GetAxis("Vertical");
            activeState.FixedStep(movementInput, Time.deltaTime);
        }

        //TODO: This should be the new way to switch states. no enums,
        //TODO: no weird storing. the PlayerStateBehaviours dont create a lot of garbage anyway
        public void SwitchToState<T>() where T : PlayerStateBehaviour, new()
        {
            activeState.OnExitState();
            activeState = new T();
            activeState.controller = this;
            activeState.OnEnterState();
        }

        public void SetAnimTrigger(string name)
        {
            animator.SetTrigger(name);
        }
        public void SetAnimFloat(string name, float value)
        { 
            animator.SetFloat(name, value);
        }
        public void SetAnimBool(string name, bool value)
            => animator.SetBool(name, value);

        //public void SetActiveInput(bool active)
           // => ignorePlayerInput = !active;
        public void ToggleActiveInput()
            => ignorePlayerInput = !ignorePlayerInput;
        public void SetActiveInput(string active)
        { 
            ignorePlayerInput = !bool.Parse(active); //TODO: fix the animations true/false messup
            //Debug.Log(ignorePlayerInput.ToString()); 
        }

        //TODO: find use cases for these.
        private void OnTriggerEnter2D(Collider2D collision)
        {
            
        }

        private void OnTriggerStay(Collider other)
        {
            
        }
        
        public enum FloatAnimParam
        {
            XVelocity, YVelocity
        }

        #region OLD_ControllerSystem
        //[Header("Renderer")]
        //[SerializeField]
        //protected new SpriteRenderer renderer;
        //[Header("Camera Stuff")]
        //[SerializeField]
        //protected new Camera camera = null;
        //protected int facingDirection = 1;
        //protected Vector2 directionToMouse = Vector2.zero;

        //[Header("Physics And Movement")]
        //[SerializeField, Tooltip("The empty transform child, will be automatically set in Start()")]
        //protected Transform ground;
        //[SerializeField, Tooltip("The collision layer used to check for ground")]
        //protected LayerMask groundLayer;
        //[SerializeField, Tooltip("Settings for movement and that")]
        //protected PlayerControllerSettings controllerSettings = PlayerControllerSettings.DefaultSettings;
        //#region setting_properties
        //    float PlayerWidth  => controllerSettings.playerWidth;
        //    float Acceleration => controllerSettings.acceleration;
        //    float PlayerHeight => controllerSettings.playerHeight;
        //    float JumpHeight   => controllerSettings.jumpHeight;
        //    float TargetSpeed  => controllerSettings.targetSpeed;
        //    float WallJumpStrength   => controllerSettings.wallJumpStrength;
        //    float AirControlStrength => controllerSettings.airControlStrength;
        //    int   MaxDashCharges => controllerSettings.dashCharges;
        //    float DashCooldown => controllerSettings.dashCooldown;
        //    float DashLength => controllerSettings.dashLength;
        //    float MaxAdhereDistance => controllerSettings.maxAdhereDist;
        //    float LedgeClimbBoost => controllerSettings.ledgeClimbBoost;
        //#endregion

        ////This could depend on different weapons?
        //[Header("Combat Stats")]

        //[SerializeField, Tooltip("The attack damage of normal attacks")]
        //protected float attackDamage = 1;
        //[SerializeField, Tooltip("The time between attacks")]
        //protected float attackCooldown = 0.75f;

        //[Header("UI")]
        //[SerializeField]
        //protected PlayerUIManager uIManager;

        //protected bool canAttack = true;
        ////public bool IsDead { get { return currentHealth <= 0f; } }

        ////Input
        //protected float xMove = 0f;
        //protected float yMove = 0;
        //protected bool jump;
        //protected bool attack;
        //protected bool shouldThrow;
        //protected float mouseScroll;
        //protected bool actionPersist;
        //protected bool frameAction;
        //protected bool dash;

        ////Actions
        //protected int selectedAction;
        //protected float actionOffset;
        //protected int dashCharges;
        //protected bool dashing = false;
        //protected bool isRecharging = false;
        //protected float remainDashCd = 0f;

        //public float RelativeDashCD => remainDashCd / DashCooldown;
        //public int AvailableDashes => dashCharges;

        ////other runtime variables
        //protected Vector3 startPos = Vector3.zero;
        //protected bool isGrounded = false;
        //protected EntityState state = EntityState.Idle;
        //protected bool isOnWall  = false;
        //protected Vector2 wallNormal = Vector2.right;
        //protected bool isOnLedge = false;
        //protected bool doLedgeCheck = true;
        //protected Vector2 ledgePosition = Vector2.zero;

        ////public override bool IsGrounded { get { return isGrounded; } }

        //private const float raycastError = 0.05f;
        //private const float g = 9.81f;

        //public void OnStart()
        //{
        //    print("playerController onStart");
        //}
        /// <summary>
        /// Start! bruh unity gae
        /// </summary>
        //protected void Start()
        //{
        //    //IsPlayer = true;
        //    startPos = transform.position;
        //    dashCharges = MaxDashCharges;

        //    if (camera == null)
        //        camera = FindObjectOfType<Camera>();
        //    if (ground == null)
        //        ground = transform.GetChild(0);
        //    //if (body == null)
        //    //    body = GetComponent<Rigidbody2D>();
        //    if (uIManager == null)
        //        uIManager = FindObjectOfType<PlayerUIManager>();
        //    //setup events
        //    //LevelManager.OnLevelStart    += OnLevelStart;
        //    //LevelManager.OnLevelFail     += OnLevelFail;
        //    //LevelManager.OnLevelComplete += OnLevelComplete;
        //}

        //protected override void OnLevelStart()
        //{
        //    Debug.Log("Player Level Start");
        //    active = true;
        //}

        //protected override void OnLevelFail()
        //{
        //    Debug.Log("Player level Fail");
        //    active = false;
        //    state  = EntityState.Dead;
        //}

        //protected override void OnLevelComplete()
        //{
        //    Debug.Log("Player level Complete");
        //    active = false;
        //}
        /// <summary>
        /// Input & mouse dependent stuff
        /// </summary>
        //TODO: make a good system for figuring out which way to look. probably velocity and wall check.
        //private void Update()
        //{
        //    //if (!active)
        //        return;
        //    //FaceMouse();
        //    GetInput();
        //    UpdateState();
        //    PerformActions();
        //}

        /// <summary>
        /// Basically everything that should run after Update
        /// </summary>
        //TODO: Send data to animator
        //private void LateUpdate()
        //{
        //    //if (!active)
        //        return;
        //    //Debug.DrawLine(transform.position, transform.position + (Vector3)(directionToMouse * (attackRange + PlayerWidth /2f)), Color.blue);
        //}

        /// <summary>
        /// Gets relevant input
        /// </summary>
        //TODO: check for more input if needed
        //private void GetInput()
        //{
        //    xMove  = Input.GetAxis("Horizontal");
        //    yMove  = Input.GetAxis("Vertical");
        //    jump   = (Input.GetButtonDown("Jump") || jump) && (isGrounded || isOnWall || isOnLedge); //include grounded check lmao idk, kind redundant but it works
        //    attack = Input.GetButtonDown("LeftClick") || attack; //Maybe this should stay true until the action is performed
        //    shouldThrow   = Input.GetButtonDown("MiddleClick") || shouldThrow;
        //    actionPersist = Input.GetButton("RightClick");
        //    frameAction   = Input.GetButtonDown("RightClick");
        //    dash = Input.GetButtonDown("Dash");

        //    //Swap actions
        //    //TODO: have all actions displayed at once, animate the swapping on the UI
        //    //if(Input.GetButtonDown("ActionSwap") && !actions[selectedAction].IsPerforming)
        //    //{
        //    //    actions[selectedAction].IsSelected = false;
        //    //    selectedAction += (int)Input.GetAxisRaw("ActionSwap");
        //    //    //!this is nicer code now yay
        //    //    selectedAction = (selectedAction + actions.Count) % actions.Count;
        //    //    uIManager.SetAction(actions[selectedAction]);
        //    //    actions[selectedAction].IsSelected = true;
        //    //}

        //    //mouse position
        //    var mousePos = (Vector2)camera.ScreenToWorldPoint(Input.mousePosition);
        //    directionToMouse = (mousePos - (Vector2)transform.position).normalized;
        //}

        //private void OnTriggerEnter2D(Collider2D collision)
        //{
        //    print("lmao");
        //}
        //TODO: rework attacking
        //private void PerformActions()
        //{
        //    if (dash && dashCharges > 0 && !dashing)
        //    {
        //        dashCharges--;
        //        dash = false;
        //        StartCoroutine(DoDash());
        //        return;
        //    }
        //    //if (attack && canAttack)
        //    //    Attack();

        //    //var act = actions[selectedAction];
        //    //act.TargetDirection = directionToMouse;
        //    //if (frameAction && act.CanPerform)
        //    //{
        //    //    frameAction = false;
        //    //    act.PerformAction();
        //    //}
        //    //act.ShouldPerform = actionPersist;
        //}

        /// <summary>
        /// Update the PlayerState
        /// </summary>
        //TODO: constant WIP, 1. Add Attack state
        //private void UpdateState()
        //{
        //    if (isGrounded)
        //    {
        //        if (xMove != 0 && !jump)
        //            state = EntityState.Run;
        //        else if (jump)
        //            state = EntityState.Jump;
        //        else 
        //            state = EntityState.Idle;
        //    }
        //    else
        //    {
        //        //if(body.velocity.y > 0)
        //        //{
        //        //    state = EntityState.Jump;
        //        //}
        //        //else if (body.velocity.y < 0)
        //        //{
        //        //    state = EntityState.Fall;
        //        //}
        //    }
        //    //bruh
        //}

        /// <summary>
        /// YEAH, SCIENCE, BITCH!
        /// </summary>
        //private void FixedUpdate()
        //{
        //    //if (!active)
        //    //    return;
        //    CheckGrounded();
        //    CheckWall();
        //    CheckLedge();
        //    Move();
        //}

        /// <summary>
        /// Does what the name says lol
        /// </summary>
        //protected void CheckGrounded()
        //{
        //    //if(body.velocity.y > 2)
        //    //{
        //    //    isGrounded = false;
        //    //    return;
        //    //}
        //    RaycastHit2D hit;
        //    var raycastPos = transform.position;
        //    var rayLength = PlayerHeight / 1.5f + raycastError;

        //    Debug.DrawRay(raycastPos, Vector3.down * rayLength, Color.red);
        //    if (hit = Physics2D.Raycast(raycastPos, Vector2.down, rayLength, groundLayer))
        //    {
        //        SetGround(hit);
        //    }
        //    else
        //    {
        //        raycastPos += Vector3.right * PlayerWidth / 3f;
        //        Debug.DrawRay(raycastPos, Vector3.down * rayLength, Color.red);
        //        if (hit = Physics2D.Raycast(raycastPos, Vector2.down, rayLength, groundLayer))
        //        {
        //            SetGround(hit);
        //        }
        //        else
        //        {
        //            raycastPos -= Vector3.right * PlayerWidth / 3f * 2f;
        //            Debug.DrawRay(raycastPos, Vector3.down * rayLength, Color.red);
        //            if (hit = Physics2D.Raycast(raycastPos, Vector2.down, rayLength, groundLayer))
        //            {
        //                SetGround(hit);
        //            }
        //            else
        //                isGrounded = false;
        //        }
        //    }
        //}
        //protected void SetGround(RaycastHit2D hit)
        //{
        //    Quaternion rot = Quaternion.LookRotation(Vector3.forward, hit.normal);
        //    ground.rotation = rot;
        //    isGrounded = true;
        //    var nPoint = hit.point + Vector2.up * PlayerHeight / 2f;
        //    var dist = Vector2.Distance(nPoint, transform.position);
        //    //stick to the ground instead of flying off
        //    if (dist < MaxAdhereDistance && dist > 0.05f)
        //    {
        //        //body.position = nPoint;
        //    }
        //}

        ///// <summary>
        ///// Check for a wall for walljumps.
        ///// </summary>
        //protected void CheckWall()
        //{
        //    if (isGrounded)
        //    {
        //        isOnWall = false;
        //        return;
        //    }
        //    var dist = PlayerWidth / 2f + raycastError;
        //    Debug.DrawRay(transform.position, Vector3.right * dist, Color.blue);
        //    Debug.DrawRay(transform.position, -Vector3.right * dist, Color.blue);
        //    RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, dist, groundLayer);
        //    if (hit)
        //    {
        //        //bruh
        //        wallNormal = hit.normal;
        //        isOnWall = true;
        //    }
        //    else if(hit = Physics2D.Raycast(transform.position, -Vector2.right, dist, groundLayer))
        //    {
        //        wallNormal = hit.normal;
        //        isOnWall = true;
        //    }
        //    else
        //    {
        //        isOnWall = false;
        //    }
        //}

        /// <summary>
        /// check for a ledge to hang onto. presumably only in the direction the player is facing. 
        /// </summary>
        //protected void CheckLedge()
        //{
        //    if (isGrounded || !doLedgeCheck)
        //        return;
        //    var xDir = (renderer.flipX) ? -1f: 1f;
        //    Vector2 topCornerOffset = new Vector2(PlayerWidth / 2f * xDir, PlayerHeight / 2f);
        //    Vector2 rayOffset = Vector2.right * (PlayerWidth / 4f * xDir) + Vector2.up * (PlayerHeight / 4f);
        //    //Vector2 rayOrigin = this.Position + rayOffset + topCornerOffset;
        //    float rayDistance = PlayerHeight / 4f;

        //    Debug.DrawRay(rayOrigin, Vector3.down * rayDistance, Color.red);
        //    RaycastHit2D hit;
        //    if(hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayDistance, groundLayer))
        //    {
        //        isOnLedge = true;
        //        ledgePosition = hit.point;
        //        //Try to move closer to the ledge: 
        //        //1. towards "wall"
        //        //2. allign top of the player with detected edge.
        //    }
        //    else
        //    {
        //        isOnLedge = false;
        //    }
        //}

        //! I fixed the movement partially. Further Improve this later on.
        /// <summary>
        /// Use the input to move the player character.
        /// </summary>
        //private void Move()
        //{
        //    //adjust the curent "look direction"
        //    renderer.flipX = (Mathf.Abs(body.velocity.x) < 0.1f) ? renderer.flipX : body.velocity.x < 0;
        //    LookDirection = Vector2.right * (renderer.flipX ? 1 : -1);
        //    if (dashing)
        //        return;

        //    //Ledge hang behaviour
        //    if (isOnLedge)
        //    {
        //        LedgeBehaviour();
        //        return;
        //    }
        //    //normal behaviour
        //    body.gravityScale = (isOnWall && body.velocity.y <= 0f) ? 0.3f : 1f;

        //    //TODO: this part of the code is pretty dang ugly, fix that please!
        //    if (isGrounded)
        //    {
        //        //Prevent slopes from interfering with movement.
        //        body.AddForce(-GetGroundForce());
        //        if (!Mathf.Approximately(xMove, 0f))
        //        {
        //            //move left/right
        //            var vel = body.velocity;
        //            var stepAcc = Acceleration * (Vector2)ground.right * Time.fixedDeltaTime * xMove;
        //            var nextVel = vel + stepAcc;
        //            if (nextVel.magnitude > TargetSpeed && nextVel.magnitude > vel.magnitude)
        //                nextVel = vel - stepAcc;
        //            body.velocity = nextVel;
        //        }
        //    }
        //    else
        //    {
        //        //air movement
        //        var airControl = xMove * Acceleration * AirControlStrength * Vector2.right;
        //        body.velocity += airControl * Time.fixedDeltaTime;
        //    }

        //    //jump last.
        //    if (jump)
        //        Jump();
        //}

        /// <summary>
        /// How the player should behave when hanging onto a ledge
        /// </summary>
        //protected void LedgeBehaviour()
        //{
        //    body.gravityScale = 0f;
        //    if (body.velocity.y <= 0.1f && body.velocity.magnitude < 2f)
        //        body.velocity = Vector2.zero;
        //    if (jump)
        //        ClimbLedge();
        //    if (yMove < 0)
        //    {
        //        doLedgeCheck = false;
        //        isOnLedge = false;
        //        Delay(this, () => { doLedgeCheck = true; }, 0.5f);
        //    }
        //    DrawCross(ledgePosition, 0.2f);
        //}

        /// <summary>
        /// Do a dash lol
        /// </summary>
        //protected IEnumerator DoDash()
        //{
        //    //if (actions[selectedAction].IsPerforming)
        //    //    actions[selectedAction].CancelAction();
        //    dashing = true;
        //    isInvincible = true;
        //    var dashTime = 0.2f;
        //    var newVel = directionToMouse * (DashLength / dashTime);
        //    body.velocity = newVel;
        //    body.gravityScale = 0f;
        //    yield return new WaitForSeconds(dashTime);
        //    body.gravityScale = 1f;
        //    body.velocity = newVel / 2f;
        //    dashing = false;
        //    isInvincible = false;

        //    if (!isRecharging)
        //        StartCoroutine(RechargeDash());
        //}

        /// <summary>
        /// recharge dashes if needed
        /// </summary>
        //protected IEnumerator RechargeDash()
        //{
        //    if (isRecharging)
        //        yield break;
        //    isRecharging = true;
        //    while( dashCharges < MaxDashCharges )
        //    {
        //        for(remainDashCd = DashCooldown; remainDashCd > 0f; remainDashCd -= Time.deltaTime)
        //        {
        //            yield return null;
        //        }
        //        dashCharges++;
        //    }
        //    isRecharging = false;
        //}

        /// <summary>
        /// The resulting force of the (sloped) ground and gravity, that accelerates the body towards the lower ground.
        /// </summary>
        //protected Vector2 GetGroundForce()
        //{
        //    if (ground.up.y > 0.95)
        //        return Vector2.zero;
        //    var Fg = body.gravityScale * body.mass * Physics2D.gravity;
        //    var groundNormal = ground.up;
        //    var alpha = Vector2.Angle(groundNormal, -Fg);
        //    var sinAlpha = Mathf.Sin(alpha * Mathf.Deg2Rad);
        //        //Debug.Log(alpha + "-- sin: " + sinAlpha);
        //    var groundOffsetDirection = (groundNormal.x >= 0) ? ground.right: -ground.right;
        //    var Fh = groundOffsetDirection * sinAlpha * Fg.magnitude;
        //    return Fh;
        //}

        /// <summary>
        /// physics fuck yeah. my brain hurts now.
        /// The player ALWAYS jumps jumpHeight units high no matter what.
        /// </summary>
        //! this is like the only thing that works nicely.
        //private void Jump()
        //{
        //    jump = false;
        //    var jumpDir = isOnWall? (wallNormal + Vector2.up).normalized : ((Vector2)ground.up + Vector2.up).normalized;
        //    float v0 = Mathf.Sqrt((JumpHeight) * (2 * g) * (isOnWall? WallJumpStrength : 1f));
        //    var velocity = jumpDir * v0;
        //        velocity.x += body.velocity.x; //Test to see if this fixes some weird issues
        //    body.velocity = velocity;
        //}

        /// <summary>
        /// Climb the ledge youre holding onto 
        /// </summary>
        //private void ClimbLedge()
        //{
        //    var finalPos = ledgePosition + Vector2.up * PlayerHeight / 2f;
        //    var tempPos = new Vector2(body.position.x, finalPos.y);
        //    var delta = Normalized(finalPos.x - tempPos.x);
        //    body.MovePosition(tempPos);

        //    DelayPhysicsFrame(this, () => 
        //    {
        //        body.MovePosition(finalPos);
        //        jump = false;
        //        if(Mathf.Abs(xMove) > 0.1f)
        //            body.velocity = Vector2.right * delta * LedgeClimbBoost;
        //    });
        //}

        //TODO: process knockback and that kind of stuff.
        /// <summary>
        /// Process a hit.
        /// </summary>
        /// <param name="hitData"></param>
        //public override void ProcessHit(AttackHitData hitData)
        //{
        //    if(!isInvincible)
        //        currentHealth -= hitData.Damage;
        //}
        //public override void ProcessHit(AttackHitData hitData, bool isOnlyDamage)
        //{
        //    if (isOnlyDamage && !isInvincible)
        //        currentHealth -= hitData.Damage;
        //    else
        //        ProcessHit(hitData);
        //}

        /// <summary>
        /// Reset the Player to his start values.
        /// </summary>
        //internal override void ResetEntity()
        //{
        //    transform.position = startPos;
        //    body.velocity = Vector2.zero;
        //}


        /// <summary>
        /// Makes the player face towards the mouse
        /// </summary>
        //[System.Obsolete("This doesnt make sense in the long run. really. Use X-Velocity and wall-detection a factor in look direction.")]
        //private void FaceMouse()
        //{
        //    var myX = transform.position.x;
        //    var mousePos = (Vector2)camera.ScreenToWorldPoint(Input.mousePosition);
        //    directionToMouse = (mousePos - (Vector2)transform.position).normalized;
        //    int newFacingDirection = NormalizeInt(mousePos.x - myX);
        //    if (newFacingDirection == facingDirection)
        //        return;
        //    facingDirection = newFacingDirection;
        //    transform.localScale = new Vector3(facingDirection, 1.0f, 1.0f);
        //}
        //[System.Serializable]
        //public struct PlayerControllerSettings
        //{
        //    [SerializeField, Tooltip("height of the players hitbox")]
        //    internal float playerHeight;
        //    [SerializeField, Tooltip("width of the players hitbox")]
        //    internal float playerWidth;
        //    [SerializeField, Tooltip("The fixed height the player can jump (in Units).")]
        //    internal float jumpHeight;
        //    [SerializeField, Tooltip("how fast the player usually moves on the ground.")]
        //    internal float targetSpeed;
        //    [SerializeField, Tooltip("how fast the speed of the player changes while on ground.")]
        //    internal float acceleration;
        //    [SerializeField, Tooltip("how fast should the player change momentum while mid-air")]
        //    internal float airControlStrength;
        //    [SerializeField, Tooltip("strength of wall jumps")]
        //    internal float wallJumpStrength;
        //    [SerializeField, Tooltip("length of the dash")]
        //    internal float dashLength;
        //    [SerializeField, Tooltip("dash cooldown")]
        //    internal float dashCooldown;
        //    [SerializeField, Tooltip("amount of max charges")]
        //    internal int dashCharges;
        //    [SerializeField, Tooltip("The max distance to stick to the ground")]
        //    internal float maxAdhereDist;
        //    [SerializeField, Tooltip("The speed the player gains after climbing a ledge")]
        //    internal float ledgeClimbBoost;

        //    public static PlayerControllerSettings DefaultSettings 
        //        => new PlayerControllerSettings()
        //        {
        //            playerHeight = 0.8f,
        //            playerWidth = 0.8f,
        //            jumpHeight = 1.0f,
        //            targetSpeed = 1.0f,
        //            acceleration = 2.0f,
        //            airControlStrength = 0.25f,
        //            wallJumpStrength = 0.75f,
        //            dashLength = 3.5f,
        //            dashCooldown = 2.5f,
        //            dashCharges = 2,
        //            maxAdhereDist = 0.1f,
        //            ledgeClimbBoost = 4f
        //        };

        //}
        #endregion
    }
}