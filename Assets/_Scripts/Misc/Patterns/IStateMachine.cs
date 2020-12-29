using UnityEngine;

namespace Game.Patterns.States
{
    //a very generalized statemachine interface
    public interface IStateMachine
    {
        void TransitionToState(IState s);
    }

    //a simple outline of a statemachine to avoid having to re-write this same code over and over
    //StateMachine<T> where T : StateMachine<T> looks super scuffed but it is required for the states to work correctly.
    //this way new StateMachines like a boss are done via -> public class Boss : StateMachine<Boss>
    public class StateMachine<T> : MonoBehaviour, IStateMachine where T : StateMachine<T>
    {
        State<T> currentState;
        public void TransitionToState(IState s)
        {
            currentState = s as State<T>;
        }
    }

}