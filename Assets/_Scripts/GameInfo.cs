using UnityEngine;

namespace Game
{
    public static class GameInfo
    {
        public const string Version = "0.1.2019_Alpha_02";

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
            public Resolution resolution = new Resolution() {
                width = 1920,
                height = 1080,
                refreshRate = 60
                };
        }
    }
}
