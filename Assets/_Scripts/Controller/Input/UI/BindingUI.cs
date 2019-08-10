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
    }
}
