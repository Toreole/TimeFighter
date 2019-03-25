using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Game.Controller
{
    /// <summary>
    /// Represent player stuff
    /// </summary>
    public class PlayerUIManager : MonoBehaviour
    {
        [SerializeField]
        protected Image actionDisplay;
        [SerializeField]
        protected Image filler;

        protected BaseAction currentAction;

        public void SetAction(BaseAction action)
        {
            currentAction = action;
            actionDisplay.sprite = currentAction.UISprite;
        }

        protected void Update()
        {
            filler.fillAmount = currentAction.RelativeCooldown;
        }
    }
}