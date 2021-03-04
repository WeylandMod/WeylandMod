using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.ManageableDeathPins
{
    internal class PlayerProfileComponent : IFeatureComponent
    {
        private readonly ManualLogSource _logger;

        public PlayerProfileComponent(ManualLogSource logger)
        {
            _logger = logger;
        }

        public void OnLaunch(bool enabled)
        {
            if (!enabled)
                return;

            On.PlayerProfile.HaveDeathPoint += HaveDeathPointHook;
        }

        public void OnConnect()
        {
        }

        public void OnDisconnect()
        {
        }

        private static bool HaveDeathPointHook(On.PlayerProfile.orig_HaveDeathPoint orig, PlayerProfile self)
        {
            return false;
        }
    }
}