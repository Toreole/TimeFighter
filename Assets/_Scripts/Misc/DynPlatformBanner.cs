using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Discord;
using Game;

namespace Game.Misc
{
    public class DynPlatformBanner : MonoBehaviour
    {
        [SerializeField]
        protected new SpriteRenderer renderer;
        [SerializeField]
        protected Sprite balance, brilliance, bravery, discord, steam;
        
        // Start is called before the first frame update 
        void Start()
        {
#if DISCORD
            var userManager = GameManager.userManager; 
            userManager.OnCurrentUserUpdate += () =>
            {
                Debug.Log("on current user update");
                if (userManager.CurrentUserHasFlag(UserFlag.HypeSquadHouse1))
                    renderer.sprite = bravery;
                else if (userManager.CurrentUserHasFlag(UserFlag.HypeSquadHouse2))
                    renderer.sprite = brilliance;
                else if (userManager.CurrentUserHasFlag(UserFlag.HypeSquadHouse3))
                    renderer.sprite = balance;
                else
                    renderer.sprite = discord;
            };
#elif STEAM
            renderer.sprite = steam;
#endif
        }
        
    }
}