using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.ItemRadar
{
    internal class ObjectDBComponent : IFeatureComponent
    {
        private readonly ManualLogSource _logger;
        private readonly ItemRadarConfig _config;

        public ObjectDBComponent(ManualLogSource logger, ItemRadarConfig config)
        {
            _logger = logger;
            _config = config;
        }

        public void OnLaunch(bool enabled)
        {
            On.ObjectDB.UpdateItemHashes += UpdateItemHashesHook;
        }

        public void OnConnect()
        {
        }

        public void OnDisconnect()
        {
        }

        private void UpdateItemHashesHook(On.ObjectDB.orig_UpdateItemHashes orig, ObjectDB self)
        {
            orig(self);

            OreRadarUtils.LoadIcons(_config.Items);
        }
    }
}