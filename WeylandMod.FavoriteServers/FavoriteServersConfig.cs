using System.IO;
using BepInEx.Configuration;
using WeylandMod.Core;

namespace WeylandMod.FavoriteServers
{
    internal class FavoriteServersConfig : IFeatureConfig
    {
        public bool Enabled { get; private set; }

        private readonly ConfigEntry<bool> _enabled;

        public FavoriteServersConfig(ConfigFile config)
        {
            _enabled = config.Bind(
                nameof(FavoriteServers),
                nameof(Enabled),
                true,
                "Enable favorite servers in-game UI."
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