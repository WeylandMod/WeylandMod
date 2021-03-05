using BepInEx.Configuration;
using WeylandMod.Core;

namespace WeylandMod.Features.ManageableDeathPins
{
    internal class ManageableDeathPinsConfig : IFeatureConfig
    {
        private const int Version = 1;

        private readonly ConfigEntry<bool> _enabled;

        public bool Enabled { get; private set; }

        public ManageableDeathPinsConfig(string name, ConfigFile config)
        {
            _enabled = config.Bind(
                name,
                nameof(Enabled),
                true,
                "Keep track of all your deaths and delete death pins using right click."
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