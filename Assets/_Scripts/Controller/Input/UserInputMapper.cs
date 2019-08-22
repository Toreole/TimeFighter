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
        protected GameObject axisBindingPrefab, buttonBindingPrefab;
        [SerializeField]
        protected Transform bindingParent;
        [SerializeField]
        protected GameObject popupDialog;
        [SerializeField]
        protected Color inactiveKeyColor, activeKeyColor, duplicateColor;

        public Color InactiveKeyColor => inactiveKeyColor;
        public Color ActiveKeyColor => activeKeyColor;

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
            //foreach(InputBinding bind in inputModule.RuntimeInputMap.bindings)
            //{
            //    var go = Instantiate(axisBindingPrefab, bindingParent);
            //    var bUI = go.GetComponent<BindingUI>();
            //    bUI.InitFor(bind);
            //    bUI.mapper = this;
            //    bindingUIs.Add(bUI);
            //}
        }

        //public void GetNewKey(string binding, bool positiveKey, Action<string> onKeyGet)
        //{
        //    StartCoroutine(AwaitKey(onKeyGet, binding, positiveKey));
        //}

        //Gotta be a coroutine since i cant really do async lol
        //private IEnumerator AwaitKey(Action<string> onKeyGet, string binding, bool positiveKey)
        //{
        //    popupDialog.SetActive(true);
        //    var enumValues = Enum.GetValues(typeof(KeyCode));
        //    KeyCode nextKey = KeyCode.None;
        //    while(true)
        //    {
        //        yield return null;

        //        foreach(KeyCode key in enumValues)
        //        {
        //            //Dont allow keys to be rebound to controllers.
        //            if (key >= KeyCode.JoystickButton0)
        //                break;
        //            if (UnityEngine.Input.GetKeyDown(key))
        //            {
        //                nextKey = key;
        //                break;
        //            }
        //        }
        //        if (nextKey != KeyCode.None)
        //            break;
        //    }
        //    popupDialog.SetActive(false);
        //    if (nextKey == KeyCode.Escape)
        //    {
        //        string oldKey = (positiveKey)? inputModule.RuntimeInputMap.GetBinding(binding).positive.ToString() : inputModule.RuntimeInputMap.GetBinding(binding).negative.ToString();
        //        onKeyGet?.Invoke(oldKey);
        //        DoDuplicateCheck();
        //        yield break;
        //    }
        //    //New Key get succesfull!
        //    onKeyGet?.Invoke(nextKey.ToString());
        //    inputModule.OverrideInputBind(binding, nextKey, positiveKey);
        //    DoDuplicateCheck();
        //}

        void DoDuplicateCheck()
        {
            //Reset colors of all binds.
            foreach (var bind in bindingUIs)
                bind.SetColorBoth(inactiveKeyColor);
            controls = InputManager.GetControlScheme(PlayerID.One);
            //Only if the input module has any duplicate bindings, it should do this
            if (controls.HasDuplicates(out List<DuplicateKeyBind> duplicates))
            {
                //Mark the duplicates as such.
                foreach (var duplicate in duplicates)
                {
                    BindingUI target = bindingUIs.Where(x => x.myBind.Equals(duplicate.bindName)).First();
                    target.SetColor(duplicate.positiveKeyIsDuplicate, duplicate.negativeKeyIsDuplicate, duplicateColor);
                }
            }
        }

        //public void SaveInputMap()
        //{
        //    inputModule.SaveCustomControls();
        //}
        //public void ResetInputMap()
        //{
        //    inputModule.ResetInputMap();
        //    //re init the mapper.
        //    InitMapper();
        //}
    }
}
