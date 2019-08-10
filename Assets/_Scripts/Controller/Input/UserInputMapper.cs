using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Game.Controller.Input.UI;

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
        [SerializeField]
        protected Color inactiveKeyColor, activeKeyColor, duplicateColor;

        public Color InactiveKeyColor => inactiveKeyColor;
        public Color ActiveKeyColor => activeKeyColor;

        protected PlayerInput inputModule;

        protected BindingUI[] bindingUIs;

        private void Start()
        {
            inputModule = PlayerInput.Instance;
        }

        public void GetNewKey(string binding, bool positiveKey, Action<string> onKeyGet)
        {
            StartCoroutine(AwaitKey(onKeyGet, binding, positiveKey));
        }

        //Gotta be a coroutine since i cant really do async lol
        private IEnumerator AwaitKey(Action<string> onKeyGet, string binding, bool positiveKey)
        {
            popupDialog.SetActive(true);
            var enumValues = Enum.GetValues(typeof(KeyCode));
            KeyCode nextKey = KeyCode.None;
            while(true)
            {
                yield return null;

                foreach (KeyCode key in enumValues)
                {
                    if (UnityEngine.Input.GetKeyDown(key))
                    {
                        nextKey = key;
                        break;
                    }
                }
                if (nextKey != KeyCode.None)
                    break;
            }
            popupDialog.SetActive(false);
            if (nextKey == KeyCode.Escape)
                yield break;
            //New Key get succesfull!
            onKeyGet?.Invoke(nextKey.ToString());
            inputModule.OverrideInputBind(binding, nextKey, positiveKey);
        }
    }
}
