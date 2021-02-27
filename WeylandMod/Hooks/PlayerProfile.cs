using BepInEx.Logging;
using WeylandMod.Utils;

namespace WeylandMod.Hooks
{
    internal static class PlayerProfileHooks
    {
        private static ManualLogSource Logger;

        public static void Init(ManualLogSource logger)
        {
            Logger = logger;

            On.PlayerProfile.HaveDeathPoint += HaveDeathPointHook;
        }

        private static bool HaveDeathPointHook(On.PlayerProfile.orig_HaveDeathPoint orig, PlayerProfile self)
        {
            if (WeylandConfig.Player.ManageableDeathMarkers.Value)
            {
                return false;
            }

            return orig(self);
        }
    }
}