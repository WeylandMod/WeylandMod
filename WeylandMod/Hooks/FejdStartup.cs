using BepInEx.Logging;
using WeylandMod.Utils;

namespace WeylandMod.Hooks
{
    internal static class FejdStartupHooks
    {
        private static ManualLogSource Logger;

        public static void Init(ManualLogSource logger)
        {
            Logger = logger;

            On.FejdStartup.IsPublicPasswordValid += IsPublicPasswordValidHook;
        }

        private static bool IsPublicPasswordValidHook(On.FejdStartup.orig_IsPublicPasswordValid orig, FejdStartup self,
            string password, World world)
        {
            if (WeylandConfig.Server.SkipPasswordValidation.Value)
            {
                return true;
            }

            return orig(self, password, world);
        }
    }
}