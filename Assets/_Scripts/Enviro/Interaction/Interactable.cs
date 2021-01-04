using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Interactions
{
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class Interactable : MonoBehaviour
    {
        public static Interactable Current {get; private set;}
        public abstract void Interact(Game.Controller.Player cause);

        protected virtual void Start() 
        {
            GetComponent<Collider2D>().isTrigger = true;    
        }

        private void OnTriggerEnter2D(Collider2D other) 
        {
            if(other.CompareTag(Game.Controller.Player.Tag))
            {
                Game.UI.PersistentUI.PlaceInteractionAt(transform, Vector3.up);
                Current = this;
            }
        }

        private void OnTriggerExit2D(Collider2D other) 
        {
            if(other.CompareTag(Game.Controller.Player.Tag) && Current == this)
            {
                Game.UI.PersistentUI.HideInteraction();
                Current = null;
            }
        }

    }
}