using UnityEngine.UI;
using UnityEngine;
using Game.Controller;
using System.Collections;

namespace Game.UI
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField]
        protected Slider healthSlider;
        [SerializeField]
        protected Image[] staminaDots;

        [SerializeField]
        protected Player player;
        [SerializeField]
        protected PlayerController pController;

#if UNITY_EDITOR
        IEnumerator Start()
        {
            yield return null;
            pController.Stamina = 0;
        }
#endif

        private void Update()
        {
            healthSlider.value = player.Health / player.MaxHealth;
            //print(pController.Stamina);
            for (int i = 0; i < staminaDots.Length; i++)
            {
                //fill the dot based on the stamina. each dot is 20 stamina. 
                //stamina for this dot = totalStam - 20*
                staminaDots[i].fillAmount = (pController.Stamina - (i * 20f)) / 20f;
                
            }
        }
    }
}