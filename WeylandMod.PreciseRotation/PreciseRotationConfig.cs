using System.IO;
using BepInEx.Configuration;
using UnityEngine;
using WeylandMod.Core;

namespace WeylandMod.PreciseRotation
{
    internal class PreciseRotationConfig : IFeatureConfig
    {
        private const int Version = 1;

        public bool Enabled { get; private set; }
        public bool ForceServerConfig { get; private set; }
        public KeyCode PrecisionModeKey { get; private set; }
        public float DefaultAngle { get; private set; }
        public float PrecisionAngle { get; private set; }

        private readonly ConfigEntry<bool> _enabled;
        private readonly ConfigEntry<KeyCode> _precisionModeKey;
        private readonly ConfigEntry<bool> _forceServerConfig;
        private readonly ConfigEntry<float> _defaultAngle;
        private readonly ConfigEntry<float> _precisionAngle;

        public PreciseRotationConfig(ConfigFile config)
        {
            _enabled = config.Bind(
                nameof(PreciseRotation),
                nameof(Enabled),
                true,
                "Let you rotate objects while building by custom angles."
            );

            _forceServerConfig = config.Bind(
                nameof(PreciseRotation),
                nameof(ForceServerConfig),
                false,
                "Force server config for precise rotation for all connected clients."
            );

            _precisionModeKey = config.Bind(
                nameof(PreciseRotation),
                nameof(PrecisionModeKey),
                KeyCode.LeftControl,
                "The key that should be held down to activate precision mode."
            );

            _defaultAngle = config.Bind(
                nameof(PreciseRotation),
                nameof(DefaultAngle),
                22.5f,
                "Default rotation angle."
            );

            _precisionAngle = config.Bind(
                nameof(PreciseRotation),
                nameof(PrecisionAngle),
                9.0f,
                "Rotation angle in precision mode."
            );
        }

        public void Reload()
        {
            Enabled = _enabled.Value;
            ForceServerConfig = _forceServerConfig.Value;
            PrecisionModeKey = _precisionModeKey.Value;
            DefaultAngle = _defaultAngle.Value;
            PrecisionAngle = _precisionAngle.Value;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(ForceServerConfig);
            writer.Write(Enabled);
            writer.Write((int) PrecisionModeKey);
            writer.Write(DefaultAngle);
            writer.Write(PrecisionAngle);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32(); // Version
            ForceServerConfig = reader.ReadBoolean();
            if (ForceServerConfig)
            {
                Enabled = reader.ReadBoolean();
                PrecisionModeKey = (KeyCode) reader.ReadInt32();
                DefaultAngle = reader.ReadSingle();
                PrecisionAngle = reader.ReadSingle();
            }
        }
    }
}