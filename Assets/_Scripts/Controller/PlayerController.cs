using UnityEngine;
using UnityEngine.Experimental.Input;

namespace Controller
{
    public class PlayerController : MonoBehaviour
    {
        private float _horizontalDir;
        [SerializeField] private float moveSpeed = 3f;
        
        private Rigidbody2D _rigid;

        private void Start()
        {
            _rigid = GetComponent<Rigidbody2D>();
            _rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private void Update()
        {
            _horizontalDir = Input.GetAxis("Horizontal");
        }

        private void FixedUpdate()
        {
            var moveDir = Vector2.right * _horizontalDir;
            _rigid.velocity = moveDir * moveSpeed;
        }
    }
}
