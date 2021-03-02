using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.ExtendedStorage
{
    internal sealed class ExtendedStorage : Feature
    {
        private ConfigEntry<bool> Enabled { get; }

        public ExtendedStorage(ManualLogSource logger, ConfigFile config)
            : base(logger, config)
        {
            Enabled = Config.Bind(
                nameof(ExtendedStorage),
                nameof(Enabled),
                false,
                "Enable ExtendedStorage feature."
            );
        }

        public override bool IsEnabled() => Enabled.Value;

        public override IEnumerable<FeaturePart> GetParts()
        {
            yield return new ContainerHooks(Logger);
        }
    }
}