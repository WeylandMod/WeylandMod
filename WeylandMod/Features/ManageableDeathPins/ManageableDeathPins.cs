using BepInEx.Configuration;
using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.ManageableDeathPins
{
    internal sealed class ManageableDeathPins : IFeature
    {
        public string Name => nameof(ManageableDeathPins);

        public IFeatureConfig Config { get; }

        public IFeatureComponent[] Components { get; }

        public ManageableDeathPins(ManualLogSource logger, ConfigFile config)
        {
            Config = new ManageableDeathPinsConfig(Name, config);
            Components = new IFeatureComponent[]
            {
                new PlayerComponent(logger),
                new PlayerProfileComponent(logger),
            };
        }
    }
}