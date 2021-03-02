using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.PermittedPlayersNoPassword
{
    internal sealed class PermittedPlayersNoPassword : Feature
    {
        private ConfigEntry<bool> Enabled { get; }

        public PermittedPlayersNoPassword(ManualLogSource logger, ConfigFile config)
            : base(logger, config)
        {
            Enabled = Config.Bind(
                nameof(PermittedPlayersNoPassword),
                nameof(Enabled),
                true,
                "Allow players in permittedlist.txt to log in to server without password."
            );
        }

        public override bool IsEnabled() => Enabled.Value;

        public override IEnumerable<FeaturePart> GetParts()
        {
            yield return new ZNetHooks(Logger);
        }
    }
}