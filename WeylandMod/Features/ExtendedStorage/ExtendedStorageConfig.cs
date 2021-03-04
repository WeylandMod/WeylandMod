using BepInEx.Configuration;
using WeylandMod.Core;

namespace WeylandMod.Features.ExtendedStorage
{
    internal class ExtendedStorageConfig : IFeatureConfig
    {
        private const int Version = 1;

        private readonly ConfigEntry<bool> _enabled;

        public bool Enabled { get; private set; }

        public ExtendedStorageConfig(string name, ConfigFile config)
        {
            _enabled = config.Bind(
                name,
                nameof(Enabled),
                false,
                "Extends size of every game container (chest, wagon, ship, etc)."
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
            Enabled = pkg.ReadBool();
        }

        public void Write(ZPackage pkg)
        {
            pkg.Write(Version);
            pkg.Write(Enabled);
        }
    }
}