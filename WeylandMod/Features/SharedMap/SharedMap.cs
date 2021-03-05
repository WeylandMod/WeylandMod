using BepInEx.Configuration;
using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.SharedMap
{
    internal sealed class SharedMap : IFeature
    {
        public string Name => nameof(SharedMap);

        public IFeatureConfig Config { get; }

        public IFeatureComponent[] Components { get; }

        public SharedMap(ManualLogSource logger, ConfigFile config)
        {
            var featureConfig = new SharedMapConfig(Name, config);

            Config = featureConfig;
            Components = new IFeatureComponent[]
            {
                new ZNetComponent(logger),
                new GameComponent(logger),
                new WorldComponent(logger),
                new MinimapComponent(logger, featureConfig),
            };
        }
    }
}