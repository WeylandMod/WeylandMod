using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.ForcedPublicPosition
{
    internal sealed class ForcedPublicPosition : Feature
    {
        private ConfigEntry<bool> Enabled { get; }

        public ForcedPublicPosition(ManualLogSource logger, ConfigFile config)
            : base(logger, config)
        {
            Enabled = Config.Bind(
                nameof(ForcedPublicPosition),
                nameof(Enabled),
                true,
                "Force public position for all players on server."
            );
        }

        public override bool IsEnabled() => Enabled.Value;

        public override IEnumerable<FeaturePart> GetParts()
        {
            yield return new ZNetHooks(Logger);
        }
    }
}