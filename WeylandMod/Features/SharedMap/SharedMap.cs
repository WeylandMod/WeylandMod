using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using WeylandMod.Core;

namespace WeylandMod.Features.SharedMap
{
    internal sealed class SharedMap : Feature
    {
        private ConfigEntry<bool> Enabled { get; }
        private ConfigEntry<bool> SharedPins { get; }
        private ConfigEntry<Color> SharedPinsColor { get; }

        public SharedMap(ManualLogSource logger, ConfigFile config)
            : base(logger, config)
        {
            Enabled = Config.Bind(
                nameof(SharedMap),
                nameof(Enabled),
                true,
                "Shared map exploration between players (includes ForcePublicPosition)."
            );

            SharedPins = Config.Bind(
                nameof(SharedMap),
                nameof(SharedPins),
                true,
                "Shared custom player pins."
            );

            SharedPinsColor = config.Bind(
                nameof(SharedMap),
                nameof(SharedPinsColor),
                new Color(0.7f, 0.7f, 1.0f),
                "Color for pins shared by other players."
            );
        }

        public override IEnumerable<Type> GetDependencies()
        {
            yield return typeof(ForcedPublicPosition.ForcedPublicPosition);
        }

        public override bool IsEnabled() => Enabled.Value;

        public override IEnumerable<FeaturePart> GetParts()
        {
            yield return new GameHooks(Logger);
            yield return new WorldHooks(Logger);
            yield return new MinimapHooks(Logger, SharedPins.Value, SharedPinsColor.Value);
        }
    }
}