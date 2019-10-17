using UnityEngine;
using System.Collections;
using Game.Controller;

namespace Game
{
    public class SlomoTestTrigger : MonoBehaviour
    {
        public float slowedTime = 0.3f;
        public float zoomSize = 4f;
        PlayerController player;
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.CompareTag("Player"))
            {
                //TODO: dynamic zoom in
                player = collision.gameObject.GetComponentInParent<PlayerController>();
                player.OnPressJump += OnJump;
                Time.timeScale = slowedTime;
                CameraController.DynamicZoom(zoomSize);
            }
        }

        void OnJump()
        {
            player.OnPressJump -= OnJump;
            //Debug.Log("Time is normal again");
            Time.timeScale = 1;
            CameraController.DynamicZoom(CameraController.DefaultSize); //now back out yall
        }
    }
}