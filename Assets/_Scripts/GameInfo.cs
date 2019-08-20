namespace Game
{
    public static class GameInfo
    {
        public const string Version = "0.1.2019_Alpha_02";

        public static RuntimeInfo Config => gameIni;
        private static RuntimeInfo gameIni;

        //Information about the Game during Runtime. This COULD include things such as detected OS, and other things that generally dont change
        public class RuntimeInfo
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            public bool allowControllerVibration = true;
#endif
        }
    }
}
