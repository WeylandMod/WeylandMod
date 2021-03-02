using System;
using System.IO;
using BepInEx.Logging;
using WeylandMod.Utils;

namespace WeylandMod.Hooks
{
    internal static class WorldHooks
    {
        private static ManualLogSource Logger;

        public static void Init(ManualLogSource logger)
        {
            Logger = logger;

            WorldExt.Init(Logger);

            On.World.SaveWorldMetaData += SaveWorldMetaDataHook;
        }

        private static void SaveWorldMetaDataHook(On.World.orig_SaveWorldMetaData orig, World self)
        {
            orig(self);

            if (ZNet.m_isServer && WeylandConfig.SharedMap.SharedExplorationEnabled.Value)
            {
                self.SaveSharedMap();
            }
        }
    }

    internal static class WorldExt
    {
        private static ManualLogSource Logger;

        public static void Init(ManualLogSource logger)
        {
            Logger = logger;
        }

        public static string GetSharedMapPath(this World self) =>
            $"{self.m_worldSavePath}/{self.m_name}.WeylandSharedMap.dat";

        public static void SaveSharedMap(this World self)
        {
            var sharedMapPath = self.GetSharedMapPath();
            var newSharedMapPath = sharedMapPath + ".new";
            var oldSharedMapPath = sharedMapPath + ".old";

            ZPackage sharedMapPackage = Minimap.instance.GetSharedMap();
            File.WriteAllBytes(newSharedMapPath, sharedMapPackage.GetArray());

            if (File.Exists(sharedMapPath))
            {
                if (File.Exists(oldSharedMapPath))
                {
                    File.Delete(oldSharedMapPath);
                }

                File.Move(sharedMapPath, oldSharedMapPath);
            }

            File.Move(newSharedMapPath, sharedMapPath);
        }

        public static void LoadSharedMap(this World self)
        {
            try
            {
                var sharedMapPath = self.GetSharedMapPath();
                if (!File.Exists(sharedMapPath))
                    return;

                // TODO clear current data?

                var sharedMapData = File.ReadAllBytes(sharedMapPath);
                var sharedMapPackage = new ZPackage(sharedMapData);
                Minimap.instance.SetSharedMap(sharedMapPackage);
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Failed to load SharedMap: {e.Message}");
            }
        }
    }
}