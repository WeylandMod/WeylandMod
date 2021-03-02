using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.ManageableDeathPins
{
    internal sealed class ManageableDeathPins : Feature
    {
        private ConfigEntry<bool> Enabled { get; }

        public ManageableDeathPins(ManualLogSource logger, ConfigFile config)
            : base(logger, config)
        {
            Enabled = Config.Bind(
                nameof(ManageableDeathPins),
                nameof(Enabled),
                true,
                "Keep track of all your deaths and delete death pins using right click."
            );
        }

        public override bool IsEnabled() => Enabled.Value;

        public override IEnumerable<FeaturePart> GetParts()
        {
            yield return new PlayerHooks(Logger);
            yield return new PlayerProfileHooks(Logger);
        }
    }
}