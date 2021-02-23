using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Controller.PlayerStates;

namespace Game.Controller
{
    /// <summary>
    /// The Player as a whole. The main component. 
    /// </summary>
    public class Player : Entity
    {
        public const string Tag = "Player";
        [Header("Player Fields")]
        [SerializeField]
        protected PlayerController controller;

        public override bool IsGrounded => controller.IsGrounded;

        /// <summary>
        /// Setup all player components
        /// </summary>
        private void Awake()
        {
            foreach (var pc in GetComponents<PlayerComponent>())
                pc.SetPlayer(this);
        }

        public override void Damage(float amount)
        {
            currentHealth -= amount;
            //TODO: the rest 
            //--DEBUG ONLY:
            //Simply restart the scene when the player dies. dont care.
            if(currentHealth <= 0)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);    
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
             
        }

        //TODO: make this better.
        public override void Stun(float time, bool cancelMovement)
        {
            if(cancelMovement)
                body.velocity = Vector2.zero;
            if(controller)
            {
                controller.SwitchToState(new PlayerStunnedState(time));
            }
        }
    }
}