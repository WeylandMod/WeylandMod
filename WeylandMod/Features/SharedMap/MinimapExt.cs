using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using UnityEngine;
using WeylandMod.Extensions;
using WeylandMod.Utils;

namespace WeylandMod.Features.SharedMap
{
    internal static class MinimapExt
    {
        private const int Version = 1;

        private static ManualLogSource Logger { get; set; }

        public static readonly ISet<Minimap.PinType> SharedPinTypes = new HashSet<Minimap.PinType>
        {
            Minimap.PinType.Icon0,
            Minimap.PinType.Icon1,
            Minimap.PinType.Icon2,
            Minimap.PinType.Icon3,
            Minimap.PinType.Icon4,
        };

        public static void Init(ManualLogSource logger)
        {
            Logger = logger;

            Logger.LogDebug($"{nameof(SharedMap)}-{nameof(MinimapExt)} Init");
        }

        public static void SharedMapSend(this Minimap self)
        {
            Logger.LogDebug($"{nameof(SharedMap)}-{nameof(MinimapExt)} SharedMapSend");

            ZRoutedRpc.instance.InvokeRoutedRPC(
                WeylandRpc.GetName("SharedMapUpdate"),
                self.GetSharedMap()
            );
        }

        public static void RPC_SharedMapUpdate(this Minimap self, long sender, ZPackage pkg)
        {
            Logger.LogDebug($"{nameof(SharedMap)}-{nameof(MinimapExt)} RPC_SharedMapUpdate {pkg.Size()}");

            self.SetSharedMap(pkg);

            if (!ZNet.m_isServer)
                return;

            ZRoutedRpc.instance.OthersInvokeRoutedRPC(WeylandRpc.GetName("SharedMapUpdate"), self.GetSharedMap());
        }

        public static void RPC_SharedPinAdd(this Minimap self, long sender, ZPackage pkg)
        {
            Logger.LogDebug($"{nameof(SharedMap)}-{nameof(MinimapExt)} RPC_SharedPinAdd {pkg.Size()}");

            self.SharedPinAdd(pkg);

            if (!ZNet.m_isServer)
                return;

            pkg.SetPos(0);

            ZRoutedRpc.instance.OthersInvokeRoutedRPC(WeylandRpc.GetName("SharedPinAdd"), pkg);
        }

        public static void RPC_SharedPinRemove(this Minimap self, long sender, ZPackage pkg)
        {
            Logger.LogDebug($"{nameof(SharedMap)}-{nameof(MinimapExt)} RPC_SharedPinRemove {pkg.Size()}");

            self.SharedPinRemove(pkg);

            if (!ZNet.m_isServer)
                return;

            pkg.SetPos(0);

            ZRoutedRpc.instance.OthersInvokeRoutedRPC(WeylandRpc.GetName("SharedPinRemove"), pkg);
        }

        public static void RPC_SharedPinNameUpdate(this Minimap self, long sender, ZPackage pkg)
        {
            Logger.LogDebug($"{nameof(SharedMap)}-{nameof(MinimapExt)} RPC_SharedPinNameUpdate {pkg.Size()}");

            self.SharedPinNameUpdate(pkg);

            if (!ZNet.m_isServer)
                return;

            pkg.SetPos(0);

            ZRoutedRpc.instance.OthersInvokeRoutedRPC(WeylandRpc.GetName("SharedPinNameUpdate"), pkg);
        }

        private static void SharedPinAdd(this Minimap self, ZPackage pkg)
        {
            Logger.LogDebug($"{nameof(SharedMap)}-{nameof(MinimapExt)} SharedPinAdd {pkg.Size()}");

            pkg.ReadInt(); // version
            var pin = SharedPinData.Read(pkg);

            var closestPin = self.GetClosestPinWithType(pin.Pos, pin.Type, 1.0f);
            if (closestPin != null)
                return;

            self.AddPin(pin.Pos, pin.Type, pin.Name, false, false);
        }

        private static void SharedPinRemove(this Minimap self, ZPackage pkg)
        {
            Logger.LogDebug($"{nameof(SharedMap)}-{nameof(MinimapExt)} SharedPinRemove {pkg.Size()}");

            pkg.ReadInt(); // version
            var pin = SharedPinData.Read(pkg);

            var closestPin = self.GetClosestPinWithType(pin.Pos, pin.Type, 1.0f);
            if (closestPin == null || closestPin.m_save)
                return;

            self.RemovePin(closestPin);
        }

        private static void SharedPinNameUpdate(this Minimap self, ZPackage pkg)
        {
            Logger.LogDebug($"{nameof(SharedMap)}-{nameof(MinimapExt)} SharedPinNameUpdate {pkg.Size()}");

            pkg.ReadInt(); // version
            var pin = SharedPinData.Read(pkg);

            var closestPin = self.GetClosestPinWithType(pin.Pos, pin.Type, 1.0f);
            if (closestPin == null || closestPin.m_save)
                return;

            Logger.LogDebug($"PinNameUpdate {closestPin.m_pos} {pin.Type} {pin.Name}");
            closestPin.m_name = pin.Name;
        }

        private static Minimap.PinData GetClosestPinWithType(
            this Minimap self, Vector3 pos, Minimap.PinType type, float radius)
        {
            Minimap.PinData closestPin = null;
            var closestDist = 0.0f;

            foreach (var pin in self.m_pins)
            {
                if (pin.m_type != type)
                    continue;

                var dist = global::Utils.DistanceXZ(pos, pin.m_pos);
                if (dist <= radius && (closestPin == null || dist < closestDist))
                {
                    closestPin = pin;
                    closestDist = dist;
                }
            }

            return closestPin;
        }

        public static ZPackage GetSharedMap(this Minimap self)
        {
            Logger.LogDebug($"{nameof(SharedMap)}-{nameof(MinimapExt)} GetSharedMap");

            var pkg = new ZPackage();
            pkg.Write(Version);
            pkg.Write(MapCompression.Compress(self.m_explored));

            var sharedPins = self.m_pins
                .Where(pin => pin.m_save || ZNet.m_isServer)
                .Where(pin => SharedPinTypes.Contains(pin.m_type))
                .ToArray();

            pkg.Write(sharedPins.Length);
            foreach (var pin in sharedPins)
            {
                SharedPinData.Write(pin, pkg);
            }

            return pkg;
        }

        public static void SetSharedMap(this Minimap self, ZPackage pkg)
        {
            Logger.LogDebug($"{nameof(SharedMap)}-{nameof(MinimapExt)} SetSharedMap");

            pkg.ReadInt();

            var explored = MapCompression.Decompress(pkg.ReadByteArray());
            self.ApplySharedMapExplored(explored);

            var pinsLength = pkg.ReadInt();
            for (var index = 0; index < pinsLength; ++index)
            {
                var pin = SharedPinData.Read(pkg);

                var closestPin = self.GetClosestPinWithType(pin.Pos, pin.Type, 1.0f);
                if (closestPin == null)
                {
                    self.AddPin(pin.Pos, pin.Type, pin.Name, false, false);
                }
                else if (!closestPin.m_save)
                {
                    closestPin.m_name = pin.Name;
                }
            }
        }

        private static void ApplySharedMapExplored(this Minimap self, bool[] explored)
        {
            Logger.LogDebug($"{nameof(SharedMap)}-{nameof(MinimapExt)} ApplySharedMapExplored");

            if (explored.Length != self.m_explored.Length)
            {
                Logger.LogError($"{nameof(SharedMap)}-{nameof(MinimapExt)} invalid explored array length");
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
    }

    internal class SharedPinData
    {
        public Vector3 Pos;
        public Minimap.PinType Type;
        public string Name;

        public static SharedPinData Read(ZPackage pkg)
        {
            return new SharedPinData
            {
                Pos = new Vector3(pkg.ReadSingle(), 0.0f, pkg.ReadSingle()),
                Type = (Minimap.PinType) pkg.ReadByte(),
                Name = pkg.ReadString(),
            };
        }

        public static void Write(Minimap.PinData data, ZPackage pkg)
        {
            pkg.Write(data.m_pos.x);
            pkg.Write(data.m_pos.z);
            pkg.Write((byte) data.m_type);
            pkg.Write(data.m_name);
        }
    }
}