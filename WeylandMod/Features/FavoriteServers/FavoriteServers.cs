using BepInEx.Configuration;
using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.FavoriteServers
{
    internal sealed class FavoriteServers : IFeature
    {
        public string Name => nameof(FavoriteServers);

        public IFeatureConfig Config { get; }

        public IFeatureComponent[] Components { get; }

        public FavoriteServers(ManualLogSource logger, ConfigFile config)
        {
            var featureConfig = new FavoriteServersConfig(Name, config);

            Config = featureConfig;
            Components = new IFeatureComponent[]
            {
                new FejdStartupComponent(logger),
            };
        }
    }
}