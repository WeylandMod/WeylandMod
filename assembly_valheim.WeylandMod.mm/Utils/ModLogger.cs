using BepInEx.Logging;

namespace WeylandMod
{
    internal static class ModLogger
    {
        private static ManualLogSource m_log = Logger.CreateLogSource("WeylandMod");

        public static void LogInfo(object data) => m_log.LogInfo(data);
        public static void LogDebug(object data) => m_log.LogDebug(data);
        public static void LogError(object data) => m_log.LogError(data);
    }
}