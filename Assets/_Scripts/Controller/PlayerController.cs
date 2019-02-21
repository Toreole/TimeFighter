using UnityEngine;
using UnityEngine.Experimental.Input;

namespace Game.Controller
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
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
        [SerializeField, Tooltip("The fixed height the player can jump (in Units).")]
        protected float jumpHeight = 1.0f;
        [SerializeField, Tooltip("how fast the player usually moves on the ground.")]
        protected float movementSpeed = 1.0f;
        [SerializeField, Tooltip("how fast should the player change momentum while mid-air")]
        protected float airControlStrength = 0.25f;
        protected bool  isGrounded    = false;

        //[Header("Input")]
        //[SerializeField]
        //protected PlayerInput playerInput;
        protected float xMove = 0f;
        protected int yMove = 0;
        protected bool jump;

        /// <summary>
        /// Start!
        /// </summary>
        private void Start()
        {
            if (camera == null)
                camera = FindObjectOfType<Camera>();
            if (ground == null)
                ground = transform.GetChild(0);
            if (body == null)
                body = GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// Input & mouse dependent stuff
        /// </summary>
        private void Update()
        {
            FaceMouse();
            GetInput();
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
            yMove = (int) (Input.GetAxis("Vertical") + 0.5f);
            jump =  Input.GetButton("Jump");
        }

        /// <summary>
        /// YEAH, SCIENCE, BITCH!
        /// </summary>
        private void FixedUpdate()
        {
            CheckGrounded();
            Move();
        }

        /// <summary>
        /// Does what the name says lol
        /// </summary>
        private void CheckGrounded()
        {
            RaycastHit2D hit;
            if(hit = Physics2D.Raycast(transform.position, Vector2.down, playerHeight / 2f + 0.05f, 1))
            {
                Quaternion rot = Quaternion.LookRotation(Vector3.forward, hit.normal);
                ground.rotation = rot;
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }
        }
        /// <summary>
        /// Use the input to move the player character.
        /// </summary>
        private void Move()
        {
            if (!isGrounded)
                return;
            if (jump)
                Jump();

            var velocity = xMove * movementSpeed * (Vector2)ground.right;
            velocity.y = body.velocity.y;
            body.velocity = velocity;
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
    }
}
