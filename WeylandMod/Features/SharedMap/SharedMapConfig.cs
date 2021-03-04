using BepInEx.Configuration;
using UnityEngine;
using WeylandMod.Core;

namespace WeylandMod.Features.SharedMap
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

        public SharedMapConfig(string name, ConfigFile config)
        {
            _enabled = config.Bind(
                name,
                nameof(Enabled),
                true,
                "Shared map exploration between all players on server."
            );

            _sharedPins = config.Bind(
                name,
                nameof(SharedPins),
                true,
                "Shared custom player pins."
            );

            _sharedPinsColor = config.Bind(
                name,
                nameof(SharedPinsColor),
                new Color(0.7f, 0.7f, 1.0f),
                "Color for pins shared by other players."
            );

            Reload();
        }

        public void Reload()
        {
            Enabled = _enabled.Value;
            SharedPins = _sharedPins.Value;
            SharedPinsColor = _sharedPinsColor.Value;
        }

        public void Read(ZPackage pkg)
        {
            var version = pkg.ReadInt();
            Enabled = pkg.ReadBool();
            SharedPins = pkg.ReadBool();
        }

        public void Write(ZPackage pkg)
        {
            pkg.Write(Version);
            pkg.Write(Enabled);
            pkg.Write(SharedPins);
        }
    }
}