using UnityEngine;
using System.Collections;
using Game.Controller;

namespace Game
{
    public class TestTrigger : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            //aight ignoring player input seems to work with this. very nice.
            if (collision.CompareTag("Player"))
                collision.transform.GetComponentInParent<PlayerController>().IgnorePlayerInput = true;
            //Collision_Area, aka. the object that actually has the collider2D attached is the one that gets passed in here.
            //this is interesting because the rigidbody component transform is the one that is the basis for all calculations
            Debug.Log(collision.name);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
                collision.transform.GetComponentInParent<PlayerController>().IgnorePlayerInput = false;
        }
    }
}