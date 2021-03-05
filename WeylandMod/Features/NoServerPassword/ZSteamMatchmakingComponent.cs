using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.NoServerPassword
{
    internal class ZSteamMatchmakingComponent : IFeatureComponent
    {
        private readonly ManualLogSource _logger;
        private readonly NoServerPasswordConfig _config;

        public ZSteamMatchmakingComponent(ManualLogSource logger, NoServerPasswordConfig config)
        {
            _logger = logger;
            _config = config;
        }

        public void OnLaunch(bool enabled)
        {
            if (!enabled || !_config.RemoveSteamPassword)
                return;

            On.ZSteamMatchmaking.RegisterServer += RegisterServerHook;
        }

        public void OnConnect()
        {
        }

        public void OnDisconnect()
        {
        }

        private static void RegisterServerHook(
            On.ZSteamMatchmaking.orig_RegisterServer orig, ZSteamMatchmaking self,
            string name, bool password, string version, bool publicServer, string worldName)
        {
            orig(self, name, false, version, publicServer, worldName);
        }
    }
}