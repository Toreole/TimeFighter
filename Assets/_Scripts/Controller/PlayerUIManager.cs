using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

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
        [SerializeField]
        protected Image dashCooldown;
        [SerializeField]
        protected TextMeshProUGUI dashCount;

        [SerializeField]
        protected PlayerController player;
        
        protected BaseAction currentAction;

        private void Start()
        {
            if (!player)
                player = FindObjectOfType<PlayerController>(); 
        }

        public void SetAction(BaseAction action)
        {
            currentAction = action;
            actionDisplay.sprite = currentAction.UISprite;
        }

        protected void Update()
        {
            filler.fillAmount = currentAction.RelativeCooldown;

            dashCooldown.fillAmount = player.RelativeDashCD;
            dashCount.text = player.AvailableDashes.ToString();
        }
    }
}