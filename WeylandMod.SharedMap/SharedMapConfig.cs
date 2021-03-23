using System.IO;
using BepInEx.Configuration;
using UnityEngine;
using WeylandMod.Core;

namespace WeylandMod.SharedMap
{
    internal class SharedMapConfig : IFeatureConfig
    {
        private const int Version = 2;

        private readonly ConfigEntry<bool> _enabled;
        private readonly ConfigEntry<bool> _sharedPins;
        private readonly ConfigEntry<Color> _sharedPinsColor;
        private readonly ConfigEntry<bool> _adminCanRemoveSharedPins;

        public bool Enabled { get; private set; }
        public bool SharedPins { get; private set; }
        public Color SharedPinsColor { get; private set; }
        public bool AdminCanRemoveSharedPins { get; private set; }

        public SharedMapConfig(ConfigFile config)
        {
            _enabled = config.Bind(
                nameof(SharedMap),
                nameof(Enabled),
                true,
                "Share map exploration between all players on server."
            );

            _sharedPins = config.Bind(
                nameof(SharedMap),
                nameof(SharedPins),
                true,
                "Share custom player pins."
            );

            _sharedPinsColor = config.Bind(
                nameof(SharedMap),
                nameof(SharedPinsColor),
                new Color(0.7f, 0.7f, 1.0f),
                "Color for pins shared by other players."
            );

            _adminCanRemoveSharedPins = config.Bind(
                nameof(SharedMap),
                nameof(AdminCanRemoveSharedPins),
                true,
                "Players in adminlist.txt can remove shared pins."
            );
        }

        public void Reload()
        {
            Enabled = _enabled.Value;
            SharedPins = _sharedPins.Value;
            SharedPinsColor = _sharedPinsColor.Value;
            AdminCanRemoveSharedPins = _adminCanRemoveSharedPins.Value;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(Enabled);
            writer.Write(SharedPins);
            writer.Write(AdminCanRemoveSharedPins);
        }

        public void Read(BinaryReader reader)
        {
            var version = reader.ReadInt32(); // Version
            Enabled = reader.ReadBoolean();
            SharedPins = reader.ReadBoolean();
            if (version > 1)
            {
                AdminCanRemoveSharedPins = reader.ReadBoolean();
            }
        }
    }
}