using UnityEngine;
using Game;

namespace Game.Controller
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Active Player Control")]
        [SerializeField]
        protected bool active = false;

        [Header("Camera Stuff")]
        [SerializeField]
        protected new Camera camera = null;
        protected int facingDirection = 1;
        protected Vector3 directionToMouse = Vector3.zero;

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
        protected float movementSpeed = 1.0f;
        [SerializeField, Tooltip("how fast should the player change momentum while mid-air")]
        protected float airControlStrength = 0.25f;

        protected Vector3 startPos = Vector3.zero;
        protected bool  isGrounded    = false;
        protected EntityState state   = EntityState.Idle;

        [Header("Combat Stats")]
        [SerializeField, Tooltip("The attack damage of normal attacks")]
        protected int attackDamage = 1;
        [SerializeField, Tooltip("The melee attack range")]
        protected float attackRange = 0.6f;
        [SerializeField, Tooltip("The time between attacks")]
        protected float attackCooldown = 0.75f;

        protected bool canAttack = true;
        protected internal float health;
        protected internal float currentHealth;

        public bool IsDead { get { return currentHealth <= 0f; } }

        //Input
        protected float xMove = 0f;
        protected int yMove = 0;
        protected bool jump;
        protected bool attack;

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
            //setup events
            GameManager.OnLevelStart    += OnLevelStart;
            GameManager.OnLevelFail     += OnLevelFail;
            GameManager.OnLevelComplete += OnLevelComplete;
        }

        private void OnLevelStart()
        {
            active = true;
        }

        private void OnLevelFail()
        {
            active = false;
            state  = EntityState.Dead;
        }

        private void OnLevelComplete()
        {
            active = false;
        }

        /// <summary>
        /// Input & mouse dependent stuff
        /// </summary>
        private void Update()
        {
            if (!active)
                return;
            FaceMouse();
            GetInput();
            UpdateState();
        }

        /// <summary>
        /// Makes the player face towards the mouse
        /// </summary>
        private void FaceMouse()
        {
            var myX = transform.position.x;
            var mousePos = camera.ScreenToWorldPoint(Input.mousePosition);
            directionToMouse = (mousePos - transform.position).normalized;
            int newFacingDirection = (mousePos.x > myX) ? 1 : -1;
            if (newFacingDirection == facingDirection)
                return;
            facingDirection = newFacingDirection;
            transform.localScale = new Vector3(facingDirection, 1.0f, 1.0f);
        }

        /// <summary>
        /// Gets relevant input
        /// </summary>
        private void GetInput()
        {
            xMove = Input.GetAxis("Horizontal");
            yMove = (int) Input.GetAxisRaw("Vertical");
            jump =  Input.GetButton("Jump");
            attack = Input.GetButtonDown("LeftClick") || attack; //Maybe this should stay true until the action is performed
        }
        
        /// <summary>
        /// Update the PlayerState
        /// </summary>
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

        /// <summary>
        /// Use the input to move the player character.
        /// </summary>
        private void Move()
        {
            if (isGrounded)
            {
                if (jump)
                    Jump();

                var velocity = xMove * movementSpeed * (Vector2)ground.right;
                velocity.y = body.velocity.y;
                body.velocity = velocity;
            }
            else
            {
                var airControl = xMove * movementSpeed * airControlStrength * Vector2.right;
                body.AddForce(airControl);
            }
        }

        /// <summary>
        /// physics fuck yeah. my brain hurts now.
        /// The player ALWAYS jumps jumpHeight units high no matter what.
        /// </summary>
        private void Jump()
        {
            var jumpDir = ((Vector2)ground.up + Vector2.up).normalized;
            float g = 9.81f * body.gravityScale;
            float v0 = Mathf.Sqrt((jumpHeight) * (2 * g));
            var velocity = body.velocity + jumpDir * v0;
            body.velocity = velocity;
            jump = false;
        }

        private void Attack()
        {
            canAttack = false;
            attack = false;
        }

        public void ProcessHit(AttackHitData hitData)
        {
            currentHealth -= hitData.Damage;
        }
        public void ProcessHit(AttackHitData hitData, bool isOnlyDamage)
        {
            if (isOnlyDamage)
                currentHealth -= hitData.Damage;
            else
                ProcessHit(hitData);
        }

        internal void ResetEntity()
        {
            transform.position = startPos;
            body.velocity = Vector2.zero;
        }
    }
}
