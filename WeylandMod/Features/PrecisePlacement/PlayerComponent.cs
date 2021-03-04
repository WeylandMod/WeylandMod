using System;
using BepInEx.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using WeylandMod.Core;

namespace WeylandMod.Features.PrecisePlacement
{
    internal class PlayerComponent : IFeatureComponent
    {
        private readonly ManualLogSource _logger;
        private readonly PrecisePlacementConfig _config;

        private float _rotationAngle = 0.0f;

        public PlayerComponent(ManualLogSource logger, PrecisePlacementConfig config)
        {
            _logger = logger;
            _config = config;
        }

        public void OnLaunch(bool enabled)
        {
        }

        public void OnConnect()
        {
            IL.Player.UpdatePlacementGhost += UpdatePlacementGhostPatch;
        }

        public void OnDisconnect()
        {
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