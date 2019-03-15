#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Game
{
    public static class BuildForPlatform
    {
        const string scenesFolder = "Assets/_Scenes/";
        static string[] scenes =
        {
            "_Menu", "000_DebugTore"
        };

        [MenuItem("Build/SteamStore/Win64")]
        public static void BuildSteamWin64()
        {
            var path = EditorUtility.SaveFolderPanel("Choose Location of Built Game for Steam/Win64", "", "");
            if (string.IsNullOrEmpty(path))
                return;

            //Settings specific for this build type.
            string specificStartupScene = "_DiscordRPC_Loader";
            var allScenes = AllScenesWith(specificStartupScene);

            //Actually build the player
            BuildPipeline.BuildPlayer(allScenes, Path.Combine(path, "TimeFighter.exe"), BuildTarget.StandaloneWindows64, BuildOptions.None);
            //Add the Readme.txt
            FileUtil.CopyFileOrDirectory("Assets/Templates/Readme.txt", Path.Combine(path, "Readme.txt"));
        }

        [MenuItem("Build/Discord/Win64")]
        public static void BuildDiscordWin64()
        {
            var path = EditorUtility.SaveFolderPanel("Choose Location of Built Game for Discord/Win64", "", "");
            if (string.IsNullOrEmpty(path))
                return;

            //Settings specific for this build type.
            string specificStartupScene = "_DiscordRPC_Loader";
            var allScenes = AllScenesWith(specificStartupScene);

            //Actually build the player
            BuildPipeline.BuildPlayer(allScenes, Path.Combine(path, "TimeFighter.exe"), BuildTarget.StandaloneWindows64, BuildOptions.None);
            //Add the Readme.txt
            FileUtil.CopyFileOrDirectory("Assets/Templates/Readme.txt", Path.Combine(path, "Readme.txt"));
        }

        /// <summary>
        /// Get the entire Scene Asset paths with file ending from the array of names, with the additional index 0 startup scene.
        /// </summary>
        /// <param name="specificStartup">The Platform/Store specific startup scene</param>
        private static string[] AllScenesWith(string specificStartup)
        {
            //Combine the given string with the static scenes array
            string[] all = new string[scenes.Length + 1];
            all[0] = specificStartup;
            for(int i = 1; i < all.Length; i ++)
            {
                all[i] = scenes[i - 1].Clone() as string;
            }
            //now add the directory and the file ending
            for(int j = 0; j < all.Length; j++)
            {
                all[j] = string.Format("{0}{1}.unity", scenesFolder, all[j]);
                Debug.Log(all[j]);
            }

            return all;
        }
    }
}
#endif