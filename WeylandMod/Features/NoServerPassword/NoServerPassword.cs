using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.NoServerPassword
{
    internal sealed class NoServerPassword : Feature
    {
        private ConfigEntry<bool> Enabled { get; }

        public NoServerPassword(ManualLogSource logger, ConfigFile config)
            : base(logger, config)
        {
            Enabled = Config.Bind(
                nameof(NoServerPassword),
                nameof(Enabled),
                true,
                "Let you launch public server without password."
            );
        }

        public override bool IsEnabled() => Enabled.Value;

        public override IEnumerable<FeaturePart> GetParts()
        {
            yield return new FejdStartupHooks(Logger);
        }
    }
}