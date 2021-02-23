using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using Game.Controller.PlayerStates;
using Game.Interactions;

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
        protected float maxSteepAngle = 37.5f;//, slipThreshold = 0.25f;
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
        [SerializeField]
        protected float airControl = 1f;
        [SerializeField]
        protected float terminalVelocityY = -50f;
        [SerializeField]
        protected GrapHookController hookController;
        [Header("Input")]
        [SerializeField]
        protected InputActionReference movementAction;
        [SerializeField]
        protected InputActionReference jumpAction, specialAction;
        [SerializeField]
        protected InputActionReference dashAction;
        [SerializeField]
        protected InputActionReference interactAction;
        [SerializeField, Header("Rendering")]
        protected new SpriteRenderer renderer;

        //public settings properties.
        public float BaseSpeed => baseSpeed;
        public float BaseSpeedSqr { get; protected set; }
        public float JumpHeight => jumpHeight;
        public float AirJumpHeight => airJumpHeight;
        public float Acceleration => acceleration;
        public float MaxSteepAngle => maxSteepAngle;
        public float JumpForce { get; private set; } //the force the player jumps with.

        //public float SlipThreshold => slipThreshold; // !! deprecated in favor of ground flags.
        public int AvailableAirJumps 
        { get => availableAirJumps; set => availableAirJumps = Mathf.Clamp(value, 0, airJumps); }
        public bool CanAirJump 
        { get => availableAirJumps > 0; set { availableAirJumps = value ? airJumps : 0; } }
        public float AirControl => airControl;
        public float TerminalVelocityY => terminalVelocityY;
        public float Stamina 
        { get => stamina; set => stamina = Mathf.Clamp(value, 0f, maxStamina); }
        public float StaminaRegen 
        { get => staminaRegen; private set => staminaRegen = value; }
        public float DashDistance 
        { get => dashDistance; }
        public float DashCost 
        { get => dashCost; }
        public float DashSpeed 
        { get => dashSpeed; }
        public float RollFallThreshold => rollFallThreshold;
        public float FallDamageThreshold => fallDamageThreshold;
        public float HalfWidth => halfWidth; //i need this for climbing.

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

        public bool JumpBeingHeld => jumpHold;

        //other variables
        protected bool isGrounded = true;
        protected bool stickToGround = true;
        protected bool ignorePlayerInput = false;
        protected int availableAirJumps;
        protected float stamina;
        protected bool isTouchingWall = false;
        /// <summary>
        /// Info about the ground material and its connected sounds.
        /// </summary>
        protected GroundData currentGround = null;
        protected WallInfo currentWall = new WallInfo();

        protected ContactPoint2D[] contactPoints = new ContactPoint2D[16];
        protected ContactFilter2D filter;

        //other properties
        //grounded property, handles event callback
        public bool IsGrounded {
            get => isGrounded;
            set
            {
                if (!IsGrounded && value)
                {
                    OnEnterGround?.Invoke(HandleLanding());
                    Debug.Log("Entered Ground.");
                }
                else if (IsGrounded && !value)
                    GroundLeaveOnNextFrame();
                isGrounded = value;
            }
        }

        //wall touch property, ?handles event callback?
        public bool IsTouchingWall
        {
            get => isTouchingWall;
            set
            {
                //if (!isTouchingWall && value)
                //    OnEnterWall?.Invoke(currentWall, jumpHold);
                isTouchingWall = value;
            }
        }

        protected Vector2 groundNormal = Vector2.up; 
        public Vector2 GroundNormal => groundNormal;
        public GroundData CurrentGround => currentGround;
        public WallInfo CurrentWall => currentWall;
        //public float GroundFriction 
        //{ get; protected set; } = 0.4f;
        public bool  StickToGround 
        { get => stickToGround; set => stickToGround = value; }
        public bool  IgnorePlayerInput 
        { get => ignorePlayerInput; set { ignorePlayerInput = value; } }
        public Vector2 LastVel 
        { get; protected set; }

        //State callback events.
        #region events

        public event System.Action OnPressJump;
        public event System.Action OnHoldJump;

        public event System.Action OnSpecialA;
        public event System.Action OnSpecialB;

        public event System.Action OnAttackA;
        public event System.Action OnAttackB;

        public event System.Action OnTakeDamage;

        public event System.Action OnDash;

        /// <summary>
        /// the bool dictates whether the player performs a roll
        /// </summary>
        public event System.Action<LandingType> OnEnterGround;
        public event System.Action OnLeaveGround;

        ///<summary>DEBUG ONLY!!!!</summary>
        private string activeStateType;

        /// <summary>
        /// Params: GroundData: Wall Information
        /// bool: is the jump button held down?
        /// </summary>
        //public event System.Action<GroundData, bool> OnEnterWall;
        #endregion

        /// <summary>
        /// Initial Setup
        /// </summary>
        private void Start()
        {
            availableAirJumps = airJumps;
            BaseSpeedSqr = baseSpeed * baseSpeed;
            stamina = maxStamina;
            
            //calculate the standard jump force (needs to be recalculated in case the gravity changes.
            JumpForce = Mathf.Sqrt(jumpHeight * Util.g2);

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

            if(interactAction.action != null)
                interactAction.action.performed += OnInteract;
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
            if(ignorePlayerInput) //if ignored, reset to defaults.
            {
                movementInput = Vector2.zero;
                jumpHold = false;
                jumpPressed = false;
                return;
            }
            if (movementAction.action != null) //moving
            {
                movementInput = movementAction.action.ReadValue<Vector2>();
                movementRaw.x = Normalized(movementInput.x);
                movementRaw.y = Normalized(movementInput.y);
            }
            if (jumpAction.action != null) //jumping  //TODO all these .action.triggered can be replaced by using the action.performed event
            {
                jumpPressed = jumpAction.action.triggered;
                jumpHold = jumpAction.action.phase == InputActionPhase.Started;
            }
            if(specialAction.action != null) // special action
            {
                if (specialAction.action.triggered)
                    OnSpecialA?.Invoke();
            }
            if (dashAction.action != null)
            {
                if (dashAction.action.triggered)
                    OnDash?.Invoke();
            }
        }

        LandingType HandleLanding()
        {
            if(LastVel.y <= FallDamageThreshold)
            {
                //TODO: take damage
                return LandingType.HardLanding;
            }
            if(LastVel.y <= RollFallThreshold || Mathf.Abs(LastVel.x) > BaseSpeed)
            {
                if (Mathf.Abs(LastVel.x) < 0.1f)
                    return LandingType.HardLanding;
                return LandingType.Roll;
            }
            return LandingType.LightLanding;
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
        //! THIS IS WEIRD I GUESS
        protected void CheckContactsForGround()
        {
            int contacts = Body.GetContacts(contactPoints);
            if (contacts == 0)
            {
                IsGrounded = DoGroundStick();
                return;
            }
            Vector2 normals = Vector2.zero;
            //float frictions = 0f;
            for (int i = 0; i < contacts && i < 16; i++)
            {
                var contact = contactPoints[i];
                if (contact.normal.y > 0.4)
                {
                    normals += contact.normal;
                    //frictions += contact.collider.friction;
                }
            }
            if (normals.Equals(Vector2.zero))
            {
                //Debug.Log("no normals");
                IsGrounded = false;
                return;
            }
            //Try getting the grounddata, if it doesnt exist just stick with the current one.
            currentGround = contactPoints[0].collider.GetComponent<GroundData>() ?? currentGround;

            //if the platform contact point is too far up, and im going up, this shouldnt be ground.
            if (currentGround.HasFlag(GroundFlags.Platform) && LastVel.y > 0.1f)
            {
                //False positive ground check.
                //Debug.Log("False positive for ground via contacts");
                IsGrounded = false;
                return;
            }
            //? remove this in favor of grounddata.
            //GroundFriction = frictions / contacts;

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
            float xOffset = Body.velocity.x * Time.deltaTime;
            Vector2 rayOrigin = Body.position + new Vector2(xOffset, 0f);

            //TODO its not 100% accurate at times.
            RaycastHit2D hit2D;
            //! EDIT: Changed CircleCast for standard Ray
            if (hit2D = Physics2D.Raycast(rayOrigin, Vector2.down, halfHeight + groundedTolerance, groundMask))
            {
                if (hit2D.collider.usedByEffector)
                    return false; //probably not a good idea but this might be a temporary fix for platforms.

                float oldVel = Body.velocity.magnitude;

                //re-set the velocity to be along the ground.
                float alpha = Vector2.SignedAngle(Vector2.up, hit2D.normal);
                if (Mathf.Abs(alpha) > maxSteepAngle)
                {
                    print("TOO STEEP");
                    return false;
                }
                //make the velocity parallel to the ground.
                float currSpeed = Body.velocity.magnitude;
                Vector2 velocity = new Vector2(Util.Normalized(Body.velocity.x) * currSpeed, 0);
                velocity = RotateVector2D(velocity, alpha);
                Debug.DrawLine(Body.position, Body.position + velocity, Color.green, 5f);
                Body.velocity = velocity;
                
                //get the ground data right here.
                currentGround = hit2D.collider.GetComponent<GroundData>() ?? currentGround;
                
                //set the position.
                Vector2 pos = Body.position;
                pos.y = hit2D.point.y + halfHeight; //edit: add halfheight instead of setting this to the centroid?
                Body.position = pos;
                groundNormal = hit2D.normal;
                return true;
            }
            Debug.DrawRay(rayOrigin, Vector2.down * (halfHeight + groundedTolerance), Color.blue, 10f);
            return false;
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
            //movementInput.x = InputManager.GetAxis("Horizontal");
            //movementInput.y = InputManager.GetAxis("Vertical");
            CheckForWall();
            activeState.FixedStep(movementInput, Time.deltaTime);
        }

        /// <summary>
        /// Checks for a wall on the "forward" direction.
        /// Slopes with an incline of less than 10° are considered a wall (Abs(normal.x) >= 0.985f)
        /// </summary>
        void CheckForWall()
        {
            Vector2 dir = FlipX ? Vector2.left : Vector2.right;
            if (!PerformWallCheck(dir))
                PerformWallCheck(-dir);

            bool PerformWallCheck(Vector2 direction)
            {
                float length = halfWidth + 0.12f; //standard procedure.
                RaycastHit2D hit;
#if UNITY_EDITOR
                Debug.DrawRay(Body.position, direction, Color.cyan);
#endif
                //check for a collider
                if (hit = Physics2D.Raycast(Body.position, direction, length, groundMask))
                {
                    //verify angle limit.
                    Vector2 normal = hit.normal;
                    Vector2 side = new Vector2(Util.Normalized(normal.x), 0f);
                    if (Vector2.SignedAngle(normal, side).InRange(-10f, 10f))
                    {
                        currentWall.materialInfo = hit.collider.GetComponent<GroundData>() ?? currentWall.materialInfo;
                        currentWall.normal = hit.normal;
                        currentWall.point = hit.point;
                        Vector2 tangent = Vector3.Cross(currentWall.normal, Vector3.forward);
                        currentWall.upTangent = tangent.y < 0 ? -tangent : tangent;
                        isTouchingWall = true; //use property, this can call the event
                        return true;
                    }
                }
                isTouchingWall = false; //directly write to the field, there is no exit wall event.
                return false;
            }
        }

        /// <summary>
        /// Switch to another State using a generic method
        /// </summary>
        /// <typeparam name="T">state type</typeparam>
        public void SwitchToState<T>() where T : PlayerStateBehaviour, new()
        {
            activeState.OnExitState();
            activeState = new T();
            activeState.controller = this;
            activeState.OnEnterState();
            activeStateType = activeState.GetType().Name;
        }
        /// <summary>
        /// Switch to a given state. Required when a state needs extra info passed in from previous state.
        /// </summary>
        public void SwitchToState(PlayerStateBehaviour state)
        {
            activeState.OnExitState();
            activeState = state;
            activeState.controller = this;
            state.OnEnterState();
            activeStateType = activeState.GetType().Name;
        }

        //public void SetActiveInput(bool active)
           // => ignorePlayerInput = !active;
        public void ToggleActiveInput()
            => ignorePlayerInput = !ignorePlayerInput;
        public void SetActiveInput(string active)
        { 
            ignorePlayerInput = !bool.Parse(active); //TODO: fix the animations true/false messup
            //Debug.Log(ignorePlayerInput.ToString()); 
        }

        /// <summary>
        /// A safe way of checking for Ground flags (handles null)
        /// </summary>
        public bool GroundHasFlag(GroundFlags flag)
        {
            return (currentGround) ? currentGround.HasFlag(flag) : false;
        }
        /// <summary>
        /// A safe way of checking for Wall flags (handles null)
        /// </summary>
        public bool WallHasFlag(GroundFlags flag)
        {
            return currentWall.materialInfo ? currentWall.materialInfo.HasFlag(flag) : false;
        }
        
        public void StartHook()
        {
            //print("start hook");
            if (hookController)
                hookController.Throw();
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            //Try to interact.
            if(IsGrounded && Body.velocity.x < 0.4f)
            {
                Interactable.Current?.Interact(player);
                //buffer the current state / go into a no-control state until the interaction is complete (animation?)
            }
        }
    }

    //EDIT: class instead of struct.
    [System.Serializable]
    public class WallInfo
    {
        public GroundData materialInfo;
        public Vector2 point;
        public Vector2 normal;
        public Vector2 upTangent;
    }
}