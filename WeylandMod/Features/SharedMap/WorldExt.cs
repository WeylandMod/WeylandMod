using System;
using System.IO;
using BepInEx.Logging;

namespace WeylandMod.Features.SharedMap
{
    internal static class WorldExt
    {
        private static ManualLogSource Logger { get; set; }

        public static void Init(ManualLogSource logger)
        {
            Logger = logger;

            Logger.LogDebug($"{nameof(SharedMap)}-{nameof(WorldExt)} Init");
        }

        private static string GetSharedMapPath(this World self) =>
            $"{self.m_worldSavePath}/{self.m_name}.WeylandSharedMap.dat";

        public static void SaveSharedMap(this World self)
        {
            Logger.LogDebug($"{nameof(SharedMap)}-{nameof(WorldExt)} SaveSharedMap");

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
            Logger.LogDebug($"{nameof(SharedMap)}-{nameof(WorldExt)} LoadSharedMap");

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
                Logger.LogWarning($"Failed to load shared map: {e.Message}");
            }
        }
    }
}