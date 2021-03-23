using System.IO;

namespace WeylandMod.SharedMap
{
    internal static class WorldExt
    {
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

        public static void LoadSharedMap(this World self, bool isSharedPinsEnabled)
        {
            var sharedMapPath = self.GetSharedMapPath();
            if (!File.Exists(sharedMapPath))
                return;

            // TODO clear current data?

            var sharedMapData = File.ReadAllBytes(sharedMapPath);
            var sharedMapPackage = new ZPackage(sharedMapData);
            Minimap.instance.SetSharedMap(isSharedPinsEnabled, sharedMapPackage);
        }

        private static string GetSharedMapPath(this World self) =>
            $"{self.m_worldSavePath}/{self.m_name}.WeylandMod.SharedMap.dat";
    }
}