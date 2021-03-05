using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.ManageableDeathPins
{
    internal class PlayerComponent : IFeatureComponent
    {
        private readonly ManualLogSource _logger;

        public PlayerComponent(ManualLogSource logger)
        {
            _logger = logger;
        }

        public void OnLaunch(bool enabled)
        {
            if (!enabled)
                return;

            On.Player.OnDeath += OnDeathHook;
        }

        public void OnConnect()
        {
        }

        public void OnDisconnect()
        {
        }

        private static void OnDeathHook(On.Player.orig_OnDeath orig, Player self)
        {
            orig(self);

            Minimap.instance.AddPin(self.transform.position, Minimap.PinType.Death, "", true, false);
        }
    }
}