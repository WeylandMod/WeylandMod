using BepInEx.Logging;
using WeylandMod.Utils;

namespace WeylandMod.Hooks
{
    internal static class ZSteamMatchmakingHooks
    {
        private static ManualLogSource Logger;

        public static void Init(ManualLogSource logger)
        {
            Logger = logger;

            On.ZSteamMatchmaking.RegisterServer += RegisterServerHook;
        }

        private static void RegisterServerHook(On.ZSteamMatchmaking.orig_RegisterServer orig, ZSteamMatchmaking self,
            string name, bool password, string version, bool publicServer, string worldName)
        {
            var pass = !WeylandConfig.Server.RemoveSteamPassword.Value && password;
            orig(self, name, pass, version, publicServer, worldName);
        }
    }
}