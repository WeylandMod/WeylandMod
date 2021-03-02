using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.ManageableDeathPins
{
    internal class PlayerProfileHooks : FeaturePart
    {
        public PlayerProfileHooks(ManualLogSource logger)
            : base(logger)
        {
        }

        public override void Init()
        {
            Logger.LogDebug($"{nameof(ManageableDeathPins)}-{nameof(PlayerProfileHooks)} Init");

            On.PlayerProfile.HaveDeathPoint += HaveDeathPointHook;
        }

        private bool HaveDeathPointHook(On.PlayerProfile.orig_HaveDeathPoint orig, PlayerProfile self)
        {
            return false;
        }
    }
}