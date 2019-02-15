using UnityEngine;

namespace Game.Controller
{
    public class StateManager : MonoBehaviour
    {
        [Header("Inputs")] public float horizontal;
        public float moveAmount;
        public bool jumpInput;

        [Header("Stats")] public float moveSpeed = 3.5f;
        public Vector2 jumpForce;

        [HideInInspector] public Animator anim;
        [HideInInspector] public Rigidbody2D rigid;

        [HideInInspector] public float delta;

        public void Init()
        {
            //anim = GetComponent<Animator>();
            //anim.applyRootMotion = false;

            rigid = GetComponent<Rigidbody2D>();
            rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
            rigid.drag = 4;
        }

        public void Tick(float d)
        {
            delta = d;
        }

        public void FixedTick(float d)
        {
            delta = d;

            rigid.drag = (moveAmount > 0) ? 0 : 4;
            rigid.velocity = new Vector2(horizontal * moveSpeed, rigid.velocity.y);

            if (jumpInput)
                rigid.AddForce(jumpForce, ForceMode2D.Impulse);
        }
    }
}
