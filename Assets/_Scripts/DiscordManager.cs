using UnityEngine.SceneManagement;
using Discord;
using DiscordApp = Discord.Discord;
using UnityEngine;

namespace Game
{
    internal class DiscordManager : MonoBehaviour
    {
        internal static bool usesDiscord = false;
        internal static DiscordManager instance;

        private DiscordApp discord;
        private ActivityManager rpcManager;
        private UserManager userManager;

        public string nextScene;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
            usesDiscord = true;

            discord = new DiscordApp(555829001327869964, (ulong)CreateFlags.Default);
            rpcManager = discord.GetActivityManager();

            Activity act = new Activity();
            act.ApplicationId = 555829001327869964;
            act.State = "reeeeeeeeeeeeeee";
            act.Details = "o o f";

            rpcManager.UpdateActivity(act, (result) => 
            {
                if (result == Result.Ok)
                    Debug.Log("lol");
                else
                    Debug.Log("sad");
            });
            SceneManager.LoadScene(nextScene);
        }
    }
}
