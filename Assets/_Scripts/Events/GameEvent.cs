using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Events
{
    //Probably not the solution i need for this, but being able to give at least one object should be fine.
    public abstract class GameEvent : MonoBehaviour
    {
        protected event Action<Object> Event;

        public void AddListener(Action<Object> a) => Event += a;
        public void RemoveListener(Action<Object> a) => Event -= a;

        protected void FireEvent(Object obj)
            => Event?.Invoke(obj);
    }
}