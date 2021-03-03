using BepInEx.Configuration;
using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.SharedMap
{
    internal sealed class SharedMap : IFeature
    {
        public ConfigEntry<bool> Enabled { get; }

        public IFeatureComponent[] Components { get; }

        public SharedMap(ManualLogSource logger, ConfigFile config)
        {
            Enabled = config.Bind(
                nameof(SharedMap),
                nameof(Enabled),
                true,
                "Shared map exploration between all players on server."
            );

            Components = new IFeatureComponent[]
            {
                new ZNetComponent(logger),
                new GameComponent(logger),
                new WorldComponent(logger),
                new MinimapComponent(logger, config),
            };
        }
    }
}