using UnityEngine;

namespace WeylandMod.SharedMap
{
    internal static class MinimapExt
    {
        public static ZPackage GetSharedMap(this Minimap self) =>
            SharedMapData.Write(self, new ZPackage());

        public static void SetSharedMap(this Minimap self, ZPackage pkg)
        {
            var mapData = SharedMapData.Read(pkg);
            self.ApplySharedMapExplored(mapData.Explored);

            foreach (SharedPinData pinData in mapData.Pins)
            {
                var closestPin = self.GetClosestPinWithType(pinData.Pos, pinData.Type, 1.0f);
                if (closestPin == null)
                {
                    self.AddPin(pinData.Pos, pinData.Type, pinData.Name, false, false);
                }
                else if (!closestPin.m_save)
                {
                    closestPin.m_name = pinData.Name;
                }
            }
        }

        public static void SharedPinAdd(this Minimap self, ZPackage pkg)
        {
            var pinData = SharedPinData.Read(pkg);

            var closestPin = self.GetClosestPinWithType(pinData.Pos, pinData.Type, 1.0f);
            if (closestPin != null)
                return;

            self.AddPin(pinData.Pos, pinData.Type, pinData.Name, false, false);
        }

        public static void SharedPinRemove(this Minimap self, ZPackage pkg)
        {
            var pinData = SharedPinData.Read(pkg);

            var closestPin = self.GetClosestPinWithType(pinData.Pos, pinData.Type, 1.0f);
            if (closestPin == null || closestPin.m_save)
                return;

            self.RemovePin(closestPin);
        }

        public static void SharedPinNameUpdate(this Minimap self, ZPackage pkg)
        {
            var pinData = SharedPinData.Read(pkg);

            var closestPin = self.GetClosestPinWithType(pinData.Pos, pinData.Type, 1.0f);
            if (closestPin == null || closestPin.m_save)
                return;

            closestPin.m_name = pinData.Name;
        }

        private static void ApplySharedMapExplored(this Minimap self, bool[] explored)
        {
            if (explored.Length != self.m_explored.Length)
            {
                return;
            }

            if (ZNet.m_isServer)
            {
                for (var index = 0; index < explored.Length; ++index)
                {
                    if (explored[index])
                    {
                        self.m_explored[index] = self.m_explored[index] || explored[index];
                    }
                }
            }
            else
            {
                for (var index = 0; index < explored.Length; ++index)
                {
                    if (explored[index])
                    {
                        self.Explore(index % self.m_textureSize, index / self.m_textureSize);
                    }
                }

                self.m_fogTexture.Apply();
            }
        }

        private static Minimap.PinData GetClosestPinWithType(this Minimap self, Vector3 pos, Minimap.PinType type, float radius)
        {
            Minimap.PinData closestPin = null;
            var closestDist = 0.0f;

            foreach (var pin in self.m_pins)
            {
                if (pin.m_type != type)
                    continue;

                var dist = Utils.DistanceXZ(pos, pin.m_pos);
                if (dist <= radius && (closestPin == null || dist < closestDist))
                {
                    closestPin = pin;
                    closestDist = dist;
                }
            }

            return closestPin;
        }
    }
}