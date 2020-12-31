using UnityEngine;
using NaughtyAttributes;

namespace Game.Events
{
    public class TriggerEvent : GameEvent
    {
        [SerializeField, Tag]
        protected string requiredTag;

        private void Start() 
        {
            GameEvent x = this;
            x.AddListener(() => Debug.Log("Hello World."));
        }

        private void OnTriggerEnter2D(Collider2D other) 
        {
            if(string.IsNullOrEmpty(requiredTag) || other.CompareTag(requiredTag))
                FireEvent();
        }
    }
}