using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.SharedMap
{
    internal class WorldHooks : FeaturePart
    {
        public WorldHooks(ManualLogSource logger)
            : base(logger)
        {
            WorldExt.Init(logger);
        }

        public override void Init()
        {
            Logger.LogDebug($"{nameof(SharedMap)}-{nameof(WorldHooks)} Init");

            On.World.SaveWorldMetaData += SaveWorldMetaDataHook;
        }

        private void SaveWorldMetaDataHook(On.World.orig_SaveWorldMetaData orig, World self)
        {
            Logger.LogDebug($"{nameof(SharedMap)}-{nameof(WorldHooks)} SaveWorldMetaData");

            orig(self);
            self.SaveSharedMap();
        }
    }
}