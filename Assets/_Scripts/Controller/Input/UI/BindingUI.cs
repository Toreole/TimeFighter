using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
                mapper.GetNewKey(myBind, false, (string nKey) => { negKey.Set(nKey); negKey.SetColor(mapper.InactiveKeyColor); });
            }
        }

        //Initialize the binding on the UI
        public void InitFor(InputBinding binding)
        {
            posKey.Set(binding.positive);
            if (binding.positive == KeyCode.Escape)
                posKey.enabled = false;
            negKey.Set(binding.negative);
            if (binding.negative == KeyCode.Escape)
                negKey.enabled = false;
            bindName.text = (myBind = binding.name);
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
