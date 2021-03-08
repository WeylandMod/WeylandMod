using BepInEx.Configuration;
using WeylandMod.Core;

namespace WeylandMod.Features.FavoriteServers
{
    internal class FavoriteServersConfig : IFeatureConfig
    {
        private const int Version = 1;

        private readonly ConfigEntry<bool> _enabled;

        public bool Enabled { get; private set; }

        public FavoriteServersConfig(string name, ConfigFile config)
        {
            _enabled = config.Bind(
                name,
                nameof(Enabled),
                true,
                "Enable favorite servers menu in game and allow connect to them."
            );

            Reload();
        }

        public void Reload()
        {
            Enabled = _enabled.Value;
        }

        public void Read(ZPackage pkg)
        {
            var version = pkg.ReadInt();
        }

        public void Write(ZPackage pkg)
        {
            pkg.Write(Version);
        }
    }
}