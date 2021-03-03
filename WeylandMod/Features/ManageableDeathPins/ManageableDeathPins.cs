using BepInEx.Configuration;
using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.ManageableDeathPins
{
    internal sealed class ManageableDeathPins : IFeature
    {
        public ConfigEntry<bool> Enabled { get; }

        public IFeatureComponent[] Components { get; }

        public ManageableDeathPins(ManualLogSource logger, ConfigFile config)
        {
            Enabled = config.Bind(
                nameof(ManageableDeathPins),
                nameof(Enabled),
                true,
                "Keep track of all your deaths and delete death pins using right click."
            );

            Components = new IFeatureComponent[]
            {
                new PlayerComponent(logger),
                new PlayerProfileComponent(logger),
            };
        }
    }
}