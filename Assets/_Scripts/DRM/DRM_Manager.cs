using UnityEngine.SceneManagement;
using UnityEngine;
using Discord;
using DiscordApp = Discord.Discord;
using Game;

namespace Game.DRM
{
    public class DRM_Manager : MonoBehaviour
    {
        //ALWAYS go to this scene next lol
        [SerializeField]
        protected string nextScene;

#if DISCORD

        protected DiscordApp discord;
        protected ApplicationManager app;

        //Setup discord
        void Start()
        {
            discord = new DiscordApp(555829001327869964, (ulong)Discord.CreateFlags.Default);
            app = discord.GetApplicationManager();
            SaveManager.discordStorageManager = discord.GetStorageManager();
            app.ValidateOrExit((result) =>
            {
                if (result == Result.Ok)
                {
                    Debug.Log("yaaahoo!");
                    GameManager.discord = discord;
                    SceneManager.LoadScene(nextScene);
                }
            });
        }
        // run callbacks in here for now
        void Update()
        {
            discord.RunCallbacks();
        }
#endif
    }
}