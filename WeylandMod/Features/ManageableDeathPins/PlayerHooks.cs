using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.ManageableDeathPins
{
    internal class PlayerHooks : FeaturePart
    {
        public PlayerHooks(ManualLogSource logger)
            : base(logger)
        {
        }

        public override void Init()
        {
            Logger.LogDebug($"{nameof(ManageableDeathPins)}-{nameof(PlayerHooks)} Init");

            On.Player.OnDeath += OnDeathHook;
        }

        private void OnDeathHook(On.Player.orig_OnDeath orig, Player self)
        {
            Logger.LogDebug($"{nameof(ManageableDeathPins)}-{nameof(PlayerHooks)} OnDeath");

            orig(self);

            Minimap.instance.AddPin(self.transform.position, Minimap.PinType.Death, "", true, false);
        }
    }
}