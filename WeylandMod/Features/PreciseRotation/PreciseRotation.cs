using BepInEx.Configuration;
using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.PreciseRotation
{
    internal class PreciseRotation : IFeature
    {
        public string Name => nameof(PreciseRotation);

        public IFeatureConfig Config { get; }

        public IFeatureComponent[] Components { get; }

        public PreciseRotation(ManualLogSource logger, ConfigFile config)
        {
            var featureConfig = new PreciseRotationConfig(Name, config);

            Config = featureConfig;
            Components = new IFeatureComponent[]
            {
                new PlayerComponent(logger, featureConfig),
            };
        }
    }
}