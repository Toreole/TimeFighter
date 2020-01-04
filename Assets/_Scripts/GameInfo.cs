using UnityEngine;

namespace Game
{
    public static class GameInfo
    {
        private static string version = "0.1.2019_Alpha_02";
        public static string Version { get => version; private set => version = value; }

        public static RuntimeInfo Config => gameIni;
        private static RuntimeInfo gameIni;
        
        //Information about the Game during Runtime. This COULD include things such as detected OS, and other things that generally dont change
        [System.Serializable]
        public class RuntimeInfo
        {
            public readonly string configFile = "Game.Ini";
            //windows only
            public bool allowControllerVibration = true;
            public bool fullscreen = true;
            public float autoSaveInterval = 600;
            public Resolution resolution = new Resolution() {
                width = 1920,
                height = 1080,
                refreshRate = 60
                };
        }

        static GameInfo()
        {
            TextAsset textAsset = Resources.Load<TextAsset>("Settings/Version");
            Version = textAsset.text;
            Debug.Log(Version); 
        }
    }
}
