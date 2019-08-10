using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game.Controller.Input
{
    public class UserInputMapper : MonoBehaviour
    {
        [SerializeField]
        protected GameObject bindingPrefab;
        [SerializeField]
        protected Transform bindingParent;
        [SerializeField]
        protected GameObject popupDialog;

        protected PlayerInput inputModule;

        private void Awake()
        {
            inputModule = PlayerInput.Instance;
        }
    }
}
