using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.SharedMap
{
    internal class WorldComponent : IFeatureComponent
    {
        private readonly ManualLogSource _logger;

        public WorldComponent(ManualLogSource logger)
        {
            WorldExt.Logger = logger;

            _logger = logger;
        }

        public void OnLaunch(bool enabled)
        {
        }

        public void OnConnect()
        {
            On.World.SaveWorldMetaData += SaveWorldMetaDataHook;
        }

        public void OnDisconnect()
        {
            On.World.SaveWorldMetaData -= SaveWorldMetaDataHook;
        }

        private static void SaveWorldMetaDataHook(On.World.orig_SaveWorldMetaData orig, World self)
        {
            orig(self);

            self.SaveSharedMap();
        }
    }
}