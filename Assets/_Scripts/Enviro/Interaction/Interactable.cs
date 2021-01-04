using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Misc;

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

        private void OnGUI()
        {
            if(DebugUtil.ShowDebugGUI && Current == this)
            {
                System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
                stringBuilder.AppendLine($"<b>Interactable</b>: {this.name}");
                stringBuilder.AppendLine($"pos: {transform.position.x} / {transform.position.y}");
                GUI.skin.box.richText = true;
                GUI.skin.box.alignment = TextAnchor.UpperLeft;
                GUI.Box(new Rect(220, 0, 300, 150), stringBuilder.ToString());
                //GUILayout.Box(stringBuilder.ToString());
            }
        }

    }
}