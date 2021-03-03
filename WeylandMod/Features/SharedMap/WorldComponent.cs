using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.SharedMap
{
    internal class WorldComponent : IFeatureComponent
    {
        private ManualLogSource Logger { get; }

        public WorldComponent(ManualLogSource logger)
        {
            Logger = logger;
            WorldExt.Logger = logger;
        }

        public void Initialize()
        {
            On.World.SaveWorldMetaData += SaveWorldMetaDataHook;
        }

        private void SaveWorldMetaDataHook(On.World.orig_SaveWorldMetaData orig, World self)
        {
            Logger.LogDebug($"{nameof(SharedMap)}.{nameof(WorldComponent)}.SaveWorldMetaData");

            orig(self);

            self.SaveSharedMap();
        }
    }
}