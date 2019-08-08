using System;
using System.Collections.Generic;
using UnityEngine;
using Game;

namespace Game.Controller
{
    public abstract class PlayerStateBehaviour : MonoBehaviour
    {
        public abstract void Step(Vector2 input);
        public abstract void OnPressSpace();

        public virtual void OnPressShift() { }
        public virtual void OnPressControl() { }
        public virtual void OnEnterState() { }
        public virtual void OnExitState() { }
    }
}