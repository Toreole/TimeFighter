using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Game.Controller.Input.UI;
using System.Linq;

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

        protected bool alreadyAwake = false;

        protected List<BindingUI> bindingUIs;

        private void Awake()
        {
            if (alreadyAwake)
                return;
            inputModule = PlayerInput.Instance;
            InitMapper();
            DoDuplicateCheck();
            alreadyAwake = true;
        }

        //maybe clear and then do that other stuff idk
        private void InitMapper()
        {
            foreach(Transform child in bindingParent)
            {
                Destroy(child.gameObject);
            }
            bindingUIs = new List<BindingUI>();
            foreach(InputBinding bind in inputModule.RuntimeInputMap.bindings)
            {
                var go = Instantiate(bindingPrefab, bindingParent);
                var bUI = go.GetComponent<BindingUI>();
                bUI.InitFor(bind);
                bUI.mapper = this;
                bindingUIs.Add(bUI);
            }
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

                foreach(KeyCode key in enumValues)
                {
                    //Dont allow keys to be rebound to controllers.
                    if (key >= KeyCode.JoystickButton0)
                        break;
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
            {
                string oldKey = (positiveKey)? inputModule.RuntimeInputMap.GetBinding(binding).positive.ToString() : inputModule.RuntimeInputMap.GetBinding(binding).negative.ToString();
                onKeyGet?.Invoke(oldKey);
                DoDuplicateCheck();
                yield break;
            }
            //New Key get succesfull!
            onKeyGet?.Invoke(nextKey.ToString());
            inputModule.OverrideInputBind(binding, nextKey, positiveKey);
            DoDuplicateCheck();
        }

        void DoDuplicateCheck()
        {
            //Reset colors of all binds.
            foreach (var bind in bindingUIs)
                bind.SetColorBoth(inactiveKeyColor);

            //Only if the input module has any duplicate bindings, it should do this
            if (inputModule.HasDuplicateBinds(out List<DuplicateKeyBind> duplicates))
            {
                //Mark the duplicates as such.
                foreach (var duplicate in duplicates)
                {
                    BindingUI target = bindingUIs.Where(x => x.myBind.Equals(duplicate.bindName)).First();
                    target.SetColor(duplicate.positiveKeyIsDuplicate, duplicate.negativeKeyIsDuplicate, duplicateColor);
                }
            }
        }

        public void SaveInputMap()
        {
            inputModule.SaveCustomControls();
        }
        public void ResetInputMap()
        {
            inputModule.ResetInputMap();
            //re init the mapper.
            InitMapper();
        }
    }
}
