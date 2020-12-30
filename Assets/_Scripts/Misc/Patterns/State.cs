using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Patterns.States
{
    ///A generic approach to the state pattern.
    public abstract class State<T> where T : StateMachine<T>
    {
        ///<summary>Enter => true. Transitioning to another state => false. Transition Priority!</summary>
        protected bool activeControl = false; 

        ///<summary>
        ///Update method for all states required.
        ///Using void instead of IState return for more direct control without breaking execution order.
        ///</summary>
        public abstract void Update(T o); //Note: Perhaps for Boss AI something along the lines of an IEnumerator Update could be preferable.

        public abstract void Enter(T o);
        public abstract void Exit(T o);

        ///<summary>Shorthand for transitioning the StateMachine to the target State without accidentally transitioning twice in a update.</summary>
        protected void TransitionToState(T o, State<T> other)
        {
            if(activeControl)
                o.TransitionToState(other);
            activeControl = false;
        }

    }
}