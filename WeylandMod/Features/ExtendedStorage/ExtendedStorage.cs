using BepInEx.Configuration;
using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.ExtendedStorage
{
    internal sealed class ExtendedStorage : IFeature
    {
        public ConfigEntry<bool> Enabled { get; }

        public IFeatureComponent[] Components { get; }

        public ExtendedStorage(ManualLogSource logger, ConfigFile config)
        {
            Enabled = config.Bind(
                nameof(ExtendedStorage),
                nameof(Enabled),
                false,
                "Enable ExtendedStorage feature."
            );

            Components = new IFeatureComponent[]
            {
                new ContainerComponent(logger),
            };
        }
    }
}