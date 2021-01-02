using UnityEngine;
using NaughtyAttributes;

namespace Game.Events
{
    //this event is triggered by entities, what kind of entity is not certain, but the tag can filter out a few.
    public class TriggerEvent : GameEvent
    {
        [SerializeField, Tag]
        protected string requiredTag;
        [SerializeField]
        protected bool oneTimeOnly = false;

        private void OnTriggerEnter2D(Collider2D other) 
        {
            Debug.Log(other.tag);
            if(other.CompareTag(requiredTag))
            {
                FireEvent(other.GetComponentInParent<Entity>());
                if(oneTimeOnly)
                    gameObject.SetActive(false);
            }
        }
    }
}