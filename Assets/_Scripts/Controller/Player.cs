using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Diagnostics;

namespace Game.Controller
{
    /// <summary>
    /// The Player as a whole. The main component. 
    /// </summary>
    public class Player : Entity
    {
        [Header("Player Fields")]
        [SerializeField]
        protected PlayerController controller;

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
            
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
             
        }
    }
}