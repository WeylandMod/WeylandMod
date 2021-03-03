using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.NoServerPassword
{
    internal class FejdStartupComponent : IFeatureComponent
    {
        private ManualLogSource Logger { get; }

        public FejdStartupComponent(ManualLogSource logger)
        {
            Logger = logger;
        }

        public void Initialize()
        {
            On.FejdStartup.IsPublicPasswordValid += IsPublicPasswordValidHook;
        }

        private static bool IsPublicPasswordValidHook(
            On.FejdStartup.orig_IsPublicPasswordValid orig, FejdStartup self,
            string password, World world)
        {
            return true;
        }
    }
}