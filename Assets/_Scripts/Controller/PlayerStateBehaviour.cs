using System;
using System.Collections.Generic;
using UnityEngine;
using Game;

namespace Game.Controller
{
    /// <summary>
    /// The base class for behaviour in the statemachine controller of the player.
    /// </summary>
    public abstract class PlayerStateBehaviour
    {
        public PlayerController controller;
        protected Rigidbody2D Body => controller.Body;
        /// <summary>
        /// Basically FixedUpdate(), but it already knows horizontal and vertical input (AD/WS)
        /// </summary>
        /// <param name="input"></param>
        public abstract void FixedStep(Vector2 input, float deltaTime);

        /// <summary>
        /// Callback thats fired when entering this state
        /// </summary>
        public abstract void OnEnterState();
        public abstract void OnExitState();
    }
}