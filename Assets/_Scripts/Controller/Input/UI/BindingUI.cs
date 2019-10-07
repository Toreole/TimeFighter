using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Luminosity.IO;

namespace Game.Controller.Input.UI
{
    public class BindingUI : MonoBehaviour
    {
        [SerializeField]
        protected KeyUI posKey, negKey;
        [SerializeField]
        protected TextMeshProUGUI bindName;

        public UserInputMapper mapper;
        public string myBind;

        public void Rebind(KeyUI key)
        {
            bool isPositive = key.Equals(posKey);
            if (isPositive)
            {
                posKey.SetColor(mapper.ActiveKeyColor);
                mapper.GetNewKey(myBind,  true, (string nKey) => { posKey.Set(nKey); posKey.SetColor(mapper.InactiveKeyColor); }); 
            }
            else
            {
                negKey.SetColor(mapper.ActiveKeyColor);
                //mapper.GetNewKey(myBind, false, (string nKey) => { negKey.Set(nKey); negKey.SetColor(mapper.InactiveKeyColor); });
            }
        }

        //Initialize the binding on the UI
        public void InitFor(InputBinding binding, string actionName)
        {
            posKey.Set(binding.Positive);
            if (binding.Positive == KeyCode.Escape)
                posKey.enabled = false;

            if (binding.Type == InputType.DigitalAxis)
            {
                negKey.Set(binding.Negative);
                if (binding.Negative == KeyCode.Escape)
                    negKey.enabled = false;
            }
            else
            {
                negKey.gameObject.SetActive(false);
            }
            bindName.text = (myBind = actionName);
        }

        public void SetColor(bool positiveKey, bool negativeKey, Color c)
        {
            if(positiveKey)
                posKey.SetColor(c);
            if (negativeKey)
                negKey.SetColor(c);
        }
        public void SetColorBoth(Color c)
        {
            posKey.SetColor(c);
            negKey.SetColor(c);
        }
    }
}
