using BepInEx.Configuration;
using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.ItemRadar
{
    internal class ItemRadar : IFeature
    {
        public string Name => nameof(ItemRadar);

        public IFeatureConfig Config { get; }

        public IFeatureComponent[] Components { get; }

        public ItemRadar(ManualLogSource logger, ConfigFile config)
        {
            var featureConfig = new ItemRadarConfig(Name, config);

            Config = featureConfig;
            Components = new IFeatureComponent[]
            {
                new ObjectDBComponent(logger, featureConfig),
                new ItemDropComponent(logger, featureConfig),
            };
        }
    }
}