using System.IO;
using BepInEx.Configuration;
using WeylandMod.Core;

namespace WeylandMod.ExtendedDeathPins
{
    internal class ExtendedDeathPinsConfig : IFeatureConfig
    {
        public bool Enabled { get; private set; }

        private readonly ConfigEntry<bool> _enabled;

        public ExtendedDeathPinsConfig(ConfigFile config)
        {
            _enabled = config.Bind(
                nameof(ExtendedDeathPins),
                nameof(Enabled),
                true,
                "Keep track of all your deaths and delete death pins using right click."
            );
        }

        public void Reload()
        {
            Enabled = _enabled.Value;
        }

        public void Write(BinaryWriter writer)
        {
        }

        public void Read(BinaryReader reader)
        {
        }
    }
}