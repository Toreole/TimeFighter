using System;
using UnityEngine;

namespace Game.Events
{
    public abstract class GameEvent : MonoBehaviour
    {
        protected event Action Event;

        public void AddListener(Action a) => Event += a;
        public void RemoveListener(Action a) => Event -= a;

        protected void FireEvent()
            => Event?.Invoke();
    }
}