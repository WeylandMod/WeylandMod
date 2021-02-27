using BepInEx.Logging;
using WeylandMod.Utils;

namespace WeylandMod.Hooks
{
    internal static class PlayerHooks
    {
        private static ManualLogSource Logger;

        public static void Init(ManualLogSource logger)
        {
            Logger = logger;

            On.Player.OnDeath += OnDeathHook;
        }

        private static void OnDeathHook(On.Player.orig_OnDeath orig, Player self)
        {
            orig(self);

            if (WeylandConfig.Player.ManageableDeathMarkers.Value)
            {
                Minimap.instance.AddPin(self.transform.position, Minimap.PinType.Death, "", true, false);
            }
        }
    }
}