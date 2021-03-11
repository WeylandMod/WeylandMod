using System.IO;
using BepInEx.Configuration;
using UnityEngine;
using WeylandMod.Core;

namespace WeylandMod.SharedMap
{
    internal class SharedMapConfig : IFeatureConfig
    {
        private const int Version = 1;

        private readonly ConfigEntry<bool> _enabled;
        private readonly ConfigEntry<bool> _sharedPins;
        private readonly ConfigEntry<Color> _sharedPinsColor;

        public bool Enabled { get; private set; }
        public bool SharedPins { get; private set; }
        public Color SharedPinsColor { get; private set; }

        public SharedMapConfig(ConfigFile config)
        {
            _enabled = config.Bind(
                nameof(SharedMap),
                nameof(Enabled),
                true,
                "Shared map exploration between all players on server."
            );

            _sharedPins = config.Bind(
                nameof(SharedMap),
                nameof(SharedPins),
                true,
                "Shared custom player pins."
            );

            _sharedPinsColor = config.Bind(
                nameof(SharedMap),
                nameof(SharedPinsColor),
                new Color(0.7f, 0.7f, 1.0f),
                "Color for pins shared by other players."
            );
        }

        public void Reload()
        {
            Enabled = _enabled.Value;
            SharedPins = _sharedPins.Value;
            SharedPinsColor = _sharedPinsColor.Value;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(Enabled);
            writer.Write(SharedPins);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32(); // Version
            Enabled = reader.ReadBoolean();
            SharedPins = reader.ReadBoolean();
        }
    }
}