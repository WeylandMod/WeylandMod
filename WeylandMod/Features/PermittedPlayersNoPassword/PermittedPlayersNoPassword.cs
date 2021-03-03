using BepInEx.Configuration;
using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.PermittedPlayersNoPassword
{
    internal sealed class PermittedPlayersNoPassword : IFeature
    {
        public ConfigEntry<bool> Enabled { get; }

        public IFeatureComponent[] Components { get; }

        public PermittedPlayersNoPassword(ManualLogSource logger, ConfigFile config)
        {
            Enabled = config.Bind(
                nameof(PermittedPlayersNoPassword),
                nameof(Enabled),
                true,
                "Allow players in permittedlist.txt to log in to server without password."
            );

            Components = new IFeatureComponent[]
            {
                new ZNetComponent(logger),
            };
        }
    }
}