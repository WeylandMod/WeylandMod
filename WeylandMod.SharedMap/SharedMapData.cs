using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WeylandMod.SharedMap
{
    internal class SharedMapData
    {
        public static readonly ISet<Minimap.PinType> SharedTypes = new HashSet<Minimap.PinType>
        {
            Minimap.PinType.Icon0,
            Minimap.PinType.Icon1,
            Minimap.PinType.Icon2,
            Minimap.PinType.Icon3,
            Minimap.PinType.Icon4,
        };

        private const int Version = 1;

        public bool[] Explored;
        public SharedPinData[] Pins;

        public static ZPackage Write(Minimap self, ZPackage pkg)
        {
            var sharedPins = self.m_pins
                .Where(pin => pin.m_save || ZNet.instance.IsServer())
                .Where(pin => SharedTypes.Contains(pin.m_type))
                .ToArray();

            pkg.Write(Version);
            pkg.Write(SharedMapUtils.CompressExploredMap(self.m_explored));
            pkg.Write(sharedPins.Length);
            foreach (var sharedPin in sharedPins)
            {
                SharedPinData.Write(sharedPin, pkg);
            }

            return pkg;
        }

        public static SharedMapData Read(ZPackage pkg)
        {
            pkg.ReadInt(); // Version

            var data = new SharedMapData
            {
                Explored = SharedMapUtils.DecompressExploredMap(pkg.ReadByteArray()),
                Pins = new SharedPinData[pkg.ReadInt()],
            };

            for (var index = 0; index < data.Pins.Length; ++index)
            {
                data.Pins[index] = SharedPinData.Read(pkg);
            }

            return data;
        }
    }

    internal class SharedPinData
    {
        private const int Version = 3;

        public Vector3 Pos;
        public Minimap.PinType Type;
        public string Name;
        public bool IsChecked;
        public long OwnerId;

        public static ZPackage Write(Minimap.PinData self, ZPackage pkg)
        {
            pkg.Write(Version);
            pkg.Write(self.m_pos.x);
            pkg.Write(self.m_pos.z);
            pkg.Write((byte) self.m_type);
            pkg.Write(self.m_name);
            pkg.Write(self.m_checked);
            pkg.Write(self.m_ownerID);
            return pkg;
        }

        public static SharedPinData Read(ZPackage pkg)
        {
            var version = pkg.ReadInt(); // Version
            return new SharedPinData
            {
                Pos = new Vector3(pkg.ReadSingle(), 0.0f, pkg.ReadSingle()),
                Type = (Minimap.PinType) pkg.ReadByte(),
                Name = pkg.ReadString(),
                IsChecked = version > 1 && pkg.ReadBool(),
                OwnerId = version > 2 ? pkg.ReadLong() : 0,
            };
        }
    }
}