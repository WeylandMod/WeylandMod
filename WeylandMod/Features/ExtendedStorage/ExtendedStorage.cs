using BepInEx.Configuration;
using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.ExtendedStorage
{
    internal sealed class ExtendedStorage : IFeature
    {
        public string Name => nameof(ExtendedStorage);

        public IFeatureConfig Config { get; }

        public IFeatureComponent[] Components { get; }

        public ExtendedStorage(ManualLogSource logger, ConfigFile config)
        {
            Config = new ExtendedStorageConfig(Name, config);
            Components = new IFeatureComponent[]
            {
                new ContainerComponent(logger),
            };
        }
    }
}