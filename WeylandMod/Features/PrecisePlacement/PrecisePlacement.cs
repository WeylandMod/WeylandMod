using BepInEx.Configuration;
using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.PrecisePlacement
{
    internal class PrecisePlacement : IFeature
    {
        public string Name => nameof(PrecisePlacement);

        public IFeatureConfig Config { get; }

        public IFeatureComponent[] Components { get; }

        public PrecisePlacement(ManualLogSource logger, ConfigFile config)
        {
            var featureConfig = new PrecisePlacementConfig(Name, config);

            Config = featureConfig;
            Components = new IFeatureComponent[]
            {
                new PlayerComponent(logger, featureConfig),
            };
        }
    }
}