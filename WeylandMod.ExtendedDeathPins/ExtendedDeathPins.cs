using BepInEx.Configuration;
using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.ExtendedDeathPins
{
    internal class ExtendedDeathPins : IFeature
    {
        public IFeatureConfig Config => _config;

        private readonly ManualLogSource _logger;
        private readonly ExtendedDeathPinsConfig _config;

        public ExtendedDeathPins(ManualLogSource logger, ConfigFile config)
        {
            _logger = logger;
            _config = new ExtendedDeathPinsConfig(config);
        }

        public void OnRegister()
        {
            _logger.LogDebug($"OnRegister Enabled={_config.Enabled}");

            if (!_config.Enabled)
                return;

            On.Player.OnDeath += OnDeathHook;
            On.PlayerProfile.HaveDeathPoint += HaveDeathPointHook;
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

        private static bool HaveDeathPointHook(On.PlayerProfile.orig_HaveDeathPoint orig, PlayerProfile self)
        {
            return false;
        }
    }
}