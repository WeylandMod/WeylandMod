using BepInEx.Configuration;
using UnityEngine;
using WeylandMod.Core;

namespace WeylandMod.Features.PrecisePlacement
{
    internal class PrecisePlacementConfig : IFeatureConfig
    {
        private const int Version = 1;

        private readonly ConfigEntry<bool> _enabled;
        private readonly ConfigEntry<KeyCode> _precisionModeKey;
        private readonly ConfigEntry<bool> _forceServerConfig;
        private readonly ConfigEntry<float> _defaultAngle;
        private readonly ConfigEntry<float> _precisionAngle;

        public bool Enabled { get; private set; }
        public KeyCode PrecisionModeKey { get; private set; }
        public bool ForceServerConfig { get; private set; }
        public float DefaultAngle { get; private set; }
        public float PrecisionAngle { get; private set; }

        public PrecisePlacementConfig(string name, ConfigFile config)
        {
            _enabled = config.Bind(
                name,
                nameof(Enabled),
                true,
                "Let you rotate objects while building by custom angles."
            );

            _precisionModeKey = config.Bind(
                name,
                nameof(PrecisionModeKey),
                KeyCode.LeftControl,
                "The key that should be held down to activate precision mode."
            );

            _forceServerConfig = config.Bind(
                name,
                nameof(ForceServerConfig),
                false,
                "Force server config for precise rotation for all connected clients."
            );

            _defaultAngle = config.Bind(
                name,
                nameof(DefaultAngle),
                22.5f,
                "Default rotation angle."
            );

            _precisionAngle = config.Bind(
                name,
                nameof(PrecisionAngle),
                9.0f,
                "Rotation angle in precision mode."
            );

            Reload();
        }

        public void Reload()
        {
            Enabled = _enabled.Value;
            PrecisionModeKey = _precisionModeKey.Value;
            ForceServerConfig = _forceServerConfig.Value;
            DefaultAngle = _defaultAngle.Value;
            PrecisionAngle = _precisionAngle.Value;
        }

        public void Read(ZPackage pkg)
        {
            var version = pkg.ReadInt();
            ForceServerConfig = pkg.ReadBool();
            if (ForceServerConfig)
            {
                Enabled = pkg.ReadBool();
                DefaultAngle = pkg.ReadSingle();
                PrecisionAngle = pkg.ReadSingle();
            }
        }

        public void Write(ZPackage pkg)
        {
            pkg.Write(Version);
            pkg.Write(ForceServerConfig);
            if (ForceServerConfig)
            {
                pkg.Write(Enabled);
                pkg.Write(DefaultAngle);
                pkg.Write(PrecisionAngle);
            }
        }
    }
}