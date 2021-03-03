using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.ManageableDeathPins
{
    internal class PlayerComponent : IFeatureComponent
    {
        private ManualLogSource Logger { get; }

        public PlayerComponent(ManualLogSource logger)
        {
            Logger = logger;
        }

        public void Initialize()
        {
            On.Player.OnDeath += OnDeathHook;
        }

        private void OnDeathHook(On.Player.orig_OnDeath orig, Player self)
        {
            Logger.LogDebug($"{nameof(ManageableDeathPins)}.{nameof(PlayerComponent)}.OnDeath");

            orig(self);

            Minimap.instance.AddPin(self.transform.position, Minimap.PinType.Death, "", true, false);
        }
    }
}