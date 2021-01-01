using Game.Serialization;

namespace Game.Patterns.States
{
    //a simple outline of a statemachine to avoid having to re-write this same code over and over
    //StateMachine<T> where T : StateMachine<T> looks super scuffed but it is required for the states to work correctly.
    //this way new StateMachines like a boss are done via -> public class Boss : StateMachine<Boss>
    //this could easily be an interface aswell, but every StateMachine controller object is complex enough to warrant being serialized.
    public abstract class StateMachine<T> : SerializedMonoBehaviour where T : StateMachine<T> 
    {
        protected State<T> currentState;
        public abstract void TransitionToState(State<T> s);
    }

}