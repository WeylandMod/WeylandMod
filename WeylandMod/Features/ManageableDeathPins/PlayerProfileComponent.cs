using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.ManageableDeathPins
{
    internal class PlayerProfileComponent : IFeatureComponent
    {
        private ManualLogSource Logger { get; }

        public PlayerProfileComponent(ManualLogSource logger)
        {
            Logger = logger;
        }

        public void Initialize()
        {
            On.PlayerProfile.HaveDeathPoint += HaveDeathPointHook;
        }

        private static bool HaveDeathPointHook(On.PlayerProfile.orig_HaveDeathPoint orig, PlayerProfile self)
        {
            return false;
        }
    }
}