using BepInEx.Configuration;
using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.NoServerPassword
{
    internal sealed class NoServerPassword : IFeature
    {
        public ConfigEntry<bool> Enabled { get; }

        public IFeatureComponent[] Components { get; }

        public NoServerPassword(ManualLogSource logger, ConfigFile config)
        {
            Enabled = config.Bind(
                nameof(NoServerPassword),
                nameof(Enabled),
                true,
                "Let you launch public server without password."
            );

            Components = new IFeatureComponent[]
            {
                new FejdStartupComponent(logger),
            };
        }
    }
}