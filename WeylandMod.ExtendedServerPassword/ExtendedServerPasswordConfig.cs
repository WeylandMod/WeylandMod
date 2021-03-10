using System.IO;
using BepInEx.Configuration;
using WeylandMod.Core;

namespace WeylandMod.ExtendedServerPassword
{
    internal class ExtendedServerPasswordConfig : IFeatureConfig
    {
        public bool SkipPublicPasswordCheck { get; private set; }
        public bool SkipPasswordCheckForPermittedPlayers { get; private set; }

        private readonly ConfigEntry<bool> _skipPublicPasswordCheck;
        private readonly ConfigEntry<bool> _skipPasswordCheckForPermittedPlayers;

        public ExtendedServerPasswordConfig(ConfigFile config)
        {
            _skipPublicPasswordCheck = config.Bind(
                nameof(ExtendedServerPassword),
                nameof(SkipPublicPasswordCheck),
                true,
                "Let you launch public server without password."
            );

            _skipPasswordCheckForPermittedPlayers = config.Bind(
                nameof(ExtendedServerPassword),
                nameof(SkipPasswordCheckForPermittedPlayers),
                true,
                "Allow players in permittedlist.txt to log in to server without password."
            );
        }

        public void Reload()
        {
            SkipPublicPasswordCheck = _skipPublicPasswordCheck.Value;
            SkipPasswordCheckForPermittedPlayers = _skipPasswordCheckForPermittedPlayers.Value;
        }

        public void Write(BinaryWriter writer)
        {
        }

        public void Read(BinaryReader reader)
        {
        }
    }
}