using BepInEx.Configuration;
using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.FavoriteServers
{
    internal class FavoriteServers : IFeature
    {
        public IFeatureConfig Config => _config;

        private readonly ManualLogSource _logger;
        private readonly FavoriteServersConfig _config;

        public FavoriteServers(ManualLogSource logger, ConfigFile config)
        {
            _logger = logger;
            _config = new FavoriteServersConfig(config);
        }

        public void OnRegister()
        {
            _logger.LogDebug($"OnRegister Enabled={_config.Enabled}");

            if (!_config.Enabled)
                return;

            On.FejdStartup.Awake += AwakeHook;
        }

        public void OnConnect()
        {
        }

        public void OnDisconnect()
        {
        }

        private void AwakeHook(On.FejdStartup.orig_Awake orig, FejdStartup self)
        {
            orig(self);

            var component = self.gameObject.AddComponent<FavoriteServersComponent>();
            component.Create(_logger);
        }
    }
}