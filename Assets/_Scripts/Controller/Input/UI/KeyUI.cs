using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

namespace Game.Controller.Input.UI
{
    public class KeyUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        protected BindingUI binding;
        [SerializeField]
        protected TextMeshProUGUI keyText;
        [SerializeField]
        protected Image background;

        public void OnPointerClick(PointerEventData eventData)
        {
            binding.Rebind(this);
        }

        //public setters
        public void Set(string key)
        {
            keyText.text = key;
        }
        public void Set(KeyCode key)
        {
            keyText.text = key.GetString();
        }
        public void SetColor(Color c)
        {
            background.color = c;
        }
    }
}
