using System;
using System.Collections.Generic;
using UnityEngine;
using Game;

namespace Game.Controller
{
    /// <summary>
    /// The base class for behaviour in the statemachine controller of the player.
    /// </summary>
    public abstract class PlayerStateBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Basically Update(), but it already knows horizontal and vertical input (AD/WS)
        /// </summary>
        /// <param name="input"></param>
        public abstract void Step(Vector2 input);
        /// <summary>
        /// What happens when space is pressed? mostly for jumping. Always used in some way
        /// </summary>
        public abstract void OnPressSpace();

        public virtual void OnHoldSpace() { }
        public virtual void OnPressRelease() { }
        /// <summary>
        /// 
        /// </summary>
        public virtual void OnPressShift() { }
        public virtual void OnPressControl() { }
        /// <summary>
        /// Callback thats fired when entering this state
        /// </summary>
        public virtual void OnEnterState() { }
        public virtual void OnExitState() { }
    }
}