using UnityEngine;
using System.Collections.Generic;
using Game;

namespace Game.Testing
{
    public class EntityBouncePad : MonoBehaviour
    {
        public float speed;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            var ent = collision.collider.GetComponentInParent<Entity>();
            if(ent)
            {
                ent.Body.velocity += (Vector2)transform.up * speed;
                print("bounce");
            }
        }
    }
}
