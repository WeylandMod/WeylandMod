using BepInEx.Configuration;
using WeylandMod.Core;

namespace WeylandMod.Features.NoServerPassword
{
    internal class NoServerPasswordConfig : IFeatureConfig
    {
        private const int Version = 1;

        private readonly ConfigEntry<bool> _enabled;
        private readonly ConfigEntry<bool> _removeSteamPassword;

        public bool Enabled { get; private set; }
        public bool RemoveSteamPassword { get; private set; }

        public NoServerPasswordConfig(string name, ConfigFile config)
        {
            _enabled = config.Bind(
                name,
                nameof(Enabled),
                true,
                "Let you launch public server without password."
            );

            _removeSteamPassword = config.Bind(
                name,
                nameof(RemoveSteamPassword),
                false,
                "Turn off setting Steam server password."
            );

            Reload();
        }

        public void Reload()
        {
            Enabled = _enabled.Value;
            RemoveSteamPassword = _removeSteamPassword.Value;
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