using BepInEx.Configuration;
using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.ExtendedStorage
{
    internal class ExtendedStorage : IFeature
    {
        public IFeatureConfig Config => _config;

        private readonly ManualLogSource _logger;
        private readonly ExtendedStorageConfig _config;

        public ExtendedStorage(ManualLogSource logger, ConfigFile config)
        {
            _logger = logger;
            _config = new ExtendedStorageConfig(config);
        }

        public void OnRegister()
        {
        }

        public void OnConnect()
        {
            _logger.LogDebug(
                "OnConnect " +
                $"Enabled={_config.Enabled} " +
                $"ExtraRows={_config.ExtraRows} " +
                $"ExtraColumns={_config.ExtraColumns}"
            );

            if (!_config.Enabled)
                return;

            On.Container.Awake += AwakeHook;
        }

        public void OnDisconnect()
        {
            _logger.LogDebug($"OnDisconnect Enabled={_config.Enabled}");

            if (!_config.Enabled)
                return;

            On.Container.Awake -= AwakeHook;
        }

        private void AwakeHook(On.Container.orig_Awake orig, Container self)
        {
            if (self.gameObject.GetComponent<TombStone>() == null)
            {
                self.m_width += _config.ExtraColumns;
                self.m_height += _config.ExtraRows;
            }

            orig(self);
        }
    }
}