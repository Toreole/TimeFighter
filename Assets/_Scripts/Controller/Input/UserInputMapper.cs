using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Game.Controller.Input.UI;
using System.Linq;
using Luminosity.IO;

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

        private readonly ScanSettings scanSettings = new ScanSettings
        {
            ScanFlags = ScanFlags.Key,
            CancelScanKey = KeyCode.Escape,
            Timeout = 10
        };

        //TODO: replace PlayerInput with the InputManager from Luminosity.IO !
        //! For this i need to get the input actions in the controlscheme and represent them.
        //! I can hard-design this into the UI. Just need to mark duplicates and all that then. should be fine.
        //! Only the FIRST (index 0) input binding should be changed. since those are the ones that are mapped to the keyb
        
        //protected PlayerInput inputModule;

        protected bool alreadyAwake = false;
        protected ControlScheme controls;

        protected List<BindingUI> bindingUIs;

        private void Awake()
        {
            if (alreadyAwake)
                return;
            InitMapper();
            alreadyAwake = true;
        }

        //maybe clear and then do that other stuff idk
        private void InitMapper()
        {
            controls = InputManager.GetControlScheme(PlayerID.One);
            foreach(Transform child in bindingParent)
            {
                Destroy(child.gameObject);
            }
            bindingUIs = new List<BindingUI>();
            foreach(var action in controls.Actions)
            {
                var bind = action.Bindings[0];
                if(bind.Type == InputType.Button || bind.Type == InputType.DigitalAxis)
                {
                    var go = Instantiate(bindingPrefab, bindingParent);
                    var bUI = go.GetComponent<BindingUI>();
                    bUI.InitFor(bind, action.Name);
                    bUI.mapper = this;
                    bindingUIs.Add(bUI);
                }
            }
            DoDuplicateCheck();
        }

        public void GetNewKey(string binding, bool positiveKey, Action<string> onKeyGet)
        {
            popupDialog.SetActive(true);
            StartCoroutine(AwaitKey(onKeyGet, binding, positiveKey));
        }

        //Gotta be a coroutine since i cant really do async lol
        private IEnumerator AwaitKey(Action<string> onKeyGet, string binding, bool positiveKey)
        {
            KeyCode[] enumValues = (KeyCode[]) Enum.GetValues(typeof(KeyCode));
            
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
            var action = InputManager.GetAction(PlayerID.One, binding);
            var bind = action.Bindings[0];

            if (nextKey == KeyCode.Escape)
            {
                string oldKey = (positiveKey) ? bind.Positive.GetString() : bind.Negative.GetString();
                onKeyGet?.Invoke(oldKey);
                DoDuplicateCheck();
                yield break;
            }
            //New Key get succesfull!
            onKeyGet?.Invoke(nextKey.GetString());
            if (positiveKey)
                bind.Positive = nextKey;
            else
                bind.Negative = nextKey; 
            DoDuplicateCheck();
        }

        void DoDuplicateCheck()
        {
            print("Doing Duplicate Check:...");
            //Reset colors of all binds.
            foreach (var bind in bindingUIs)
                bind.SetColorBoth(inactiveKeyColor);
            //Only if the input module has any duplicate bindings, it should do this
            if (controls.HasDuplicates(out List<DuplicateKeyBind> duplicates))
            {
                print(duplicates.Count);
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
            InputManager.Save();
        }
        public void ResetInputMap()
        {
            //TODO: figure out a way to reset input.
            //re init the mapper.
            InitMapper();
        }
    }

    public class DuplicateKeyBind
    {
        public string bindName = "";
        public bool positiveKeyIsDuplicate = false;
        public bool negativeKeyIsDuplicate = false;

        public bool EitherIsDuplicate => positiveKeyIsDuplicate || negativeKeyIsDuplicate;
        public bool BothAre => positiveKeyIsDuplicate && negativeKeyIsDuplicate;
    }
}
