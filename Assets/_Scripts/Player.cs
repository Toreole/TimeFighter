using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.Controller;

namespace Game
{
    /// <summary>
    /// The Player as a whole. The main component. 
    /// </summary>
    public class Player : Entity
    {
        [Header("Player Fields")]
        [SerializeField]
        protected PlayerController controller;

        public override void Damage(float amount)
        {
            throw new System.NotImplementedException();
        }
    }
}