using System;
using BepInEx.Configuration;
using BepInEx.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using WeylandMod.Core;

namespace WeylandMod.PreciseRotation
{
    internal class PreciseRotation : IFeature
    {
        public IFeatureConfig Config => _config;

        private readonly ManualLogSource _logger;
        private readonly PreciseRotationConfig _config;

        private float _rotationAngle;

        public PreciseRotation(ManualLogSource logger, ConfigFile config)
        {
            _logger = logger;
            _config = new PreciseRotationConfig(config);
        }

        public void OnRegister()
        {
        }

        public void OnConnect()
        {
            _logger.LogDebug(
                "OnConnect " +
                $"Enabled={_config.Enabled} " +
                $"PrecisionModeKey={_config.PrecisionModeKey} " +
                $"DefaultAngle={_config.DefaultAngle} " +
                $"PrecisionAngle={_config.PrecisionAngle}"
            );

            if (!_config.Enabled)
                return;

            IL.Player.UpdatePlacementGhost += UpdatePlacementGhostPatch;
        }

        public void OnDisconnect()
        {
            _logger.LogDebug($"OnDisconnect Enabled={_config.Enabled}");

            if (!_config.Enabled)
                return;

            IL.Player.UpdatePlacementGhost -= UpdatePlacementGhostPatch;
        }

        private void UpdatePlacementGhostPatch(ILContext il)
        {
            new ILCursor(il).GotoNext(
                    x => x.MatchLdcR4(22.5f),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<Player>("m_placeRotation"),
                    x => x.MatchConvR4(),
                    x => x.MatchMul()
                )
                .RemoveRange(5)
                .Emit(OpCodes.Ldarg_0) // push this
                .EmitDelegate<Func<Player, float>>(GetRotationAngle);
        }

        private float GetRotationAngle(Player self)
        {
            var rotationAngle = Input.GetKey(_config.PrecisionModeKey)
                ? _config.PrecisionAngle
                : _config.DefaultAngle;

            _rotationAngle += rotationAngle * self.m_placeRotation;
            self.m_placeRotation = 0;

            if (_rotationAngle > 180.0f) _rotationAngle -= 360.0f;
            if (_rotationAngle < -180.0f) _rotationAngle += 360.0f;

            return _rotationAngle;
        }
    }
}