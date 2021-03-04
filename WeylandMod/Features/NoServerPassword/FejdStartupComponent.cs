using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.NoServerPassword
{
    internal class FejdStartupComponent : IFeatureComponent
    {
        private readonly ManualLogSource _logger;

        public FejdStartupComponent(ManualLogSource logger)
        {
            _logger = logger;
        }

        public void OnLaunch(bool enabled)
        {
            if (!enabled)
                return;

            On.FejdStartup.IsPublicPasswordValid += IsPublicPasswordValidHook;
        }

        public void OnConnect()
        {
        }

        public void OnDisconnect()
        {
        }

        private static bool IsPublicPasswordValidHook(
            On.FejdStartup.orig_IsPublicPasswordValid orig, FejdStartup self,
            string password, World world)
        {
            return true;
        }
    }
}