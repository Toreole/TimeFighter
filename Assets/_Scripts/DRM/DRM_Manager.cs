using UnityEngine.SceneManagement;
using UnityEngine;
using Discord;
using Game.Serialization;
using DiscordApp = Discord.Discord;

namespace Game.DRM
{
    public class DRM_Manager : MonoBehaviour
    {
#if DISCORD
        //Setup discord
        void Start()
        {
            var discord = new DiscordApp(555829001327869964, (ulong)Discord.CreateFlags.Default);
            var app = discord.GetApplicationManager();
            SaveManager.discordStorageManager = discord.GetStorageManager();
            app.ValidateOrExit((result) =>
            {
                if (result == Result.Ok)
                {
                    Debug.Log("yaaahoo!");
                    GameManager.discord = discord;
                }
            });
        }
#endif
    }
}