using System;
using BepInEx.Configuration;
using BepInEx.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using WeylandMod.Core;

namespace WeylandMod.SharedMap
{
    internal class SharedMap : IFeature
    {
        public const string RpcAdminPinRemoveName = "RPC_WeylandMod_AdminPinRemove";

        public IFeatureConfig Config => _config;

        private readonly ManualLogSource _logger;
        private readonly SharedMapConfig _config;

        public SharedMap(ManualLogSource logger, ConfigFile config)
        {
            _logger = logger;
            _config = new SharedMapConfig(config);
        }

        public void OnRegister()
        {
            _logger.LogDebug("OnRegister");

            On.Minimap.Awake += AwakeHook;
            On.Minimap.Start += StartHook;
        }

        public void OnConnect()
        {
            _logger.LogDebug(
                "OnConnect " +
                $"Enabled={_config.Enabled} " +
                $"SharedPins={_config.SharedPins} " +
                $"SharedPinsColor={_config.SharedPinsColor}"
            );

            if (!_config.Enabled)
                return;

            ZNet.instance.SetPublicReferencePosition(true);

            IL.ZNet.SetPublicReferencePosition += SetPublicReferencePositionPatch;
            IL.Minimap.UpdatePins += UpdatePinsPatch;

            On.Game.SpawnPlayer += SpawnPlayerHook;
            On.World.SaveWorldMetaData += SaveWorldMetaDataHook;
            On.Minimap.AddPin += AddPinHook;
            On.Minimap.RemovePin_PinData += RemovePinDataHook;
            On.Minimap.RemovePin_Vector3_float += RemovePinRadiusHook;
            On.Minimap.UpdateNameInput += UpdateNameInputHook;
            On.Minimap.OnMapLeftClick += OnMapLeftClickHook;
        }

        public void OnDisconnect()
        {
            _logger.LogDebug("OnDisconnect");

            if (!_config.Enabled)
                return;

            IL.ZNet.SetPublicReferencePosition -= SetPublicReferencePositionPatch;
            IL.Minimap.UpdatePins -= UpdatePinsPatch;

            On.Game.SpawnPlayer -= SpawnPlayerHook;
            On.World.SaveWorldMetaData -= SaveWorldMetaDataHook;
            On.Minimap.AddPin -= AddPinHook;
            On.Minimap.RemovePin_PinData -= RemovePinDataHook;
            On.Minimap.RemovePin_Vector3_float -= RemovePinRadiusHook;
            On.Minimap.UpdateNameInput -= UpdateNameInputHook;
        }

        private static void SetPublicReferencePositionPatch(ILContext il)
        {
            // remove setting m_publicReferencePosition
            new ILCursor(il)
                .GotoNext(
                    x => x.MatchLdarg(0),
                    x => x.MatchLdarg(1),
                    x => x.MatchStfld<ZNet>("m_publicReferencePosition")
                )
                .RemoveRange(3);
        }

        private static void UpdatePinsPatch(ILContext il)
        {
            new ILCursor(il).GotoNext(x => x.MatchLdfld<Minimap>("m_pinPrefab"))
                .Remove()
                .Emit(OpCodes.Ldloc, 4) // push pin, Minimap.this already on stack
                .EmitDelegate<Func<Minimap, Minimap.PinData, GameObject>>(GetPinPrefab);
        }

        private static GameObject GetPinPrefab(Minimap self, Minimap.PinData data)
        {
            if (!data.m_save && SharedMapData.SharedTypes.Contains(data.m_type))
            {
                return SharedMapComponent.Instance.SharedPinPrefab;
            }

            return self.m_pinPrefab;
        }

        private void AwakeHook(On.Minimap.orig_Awake orig, Minimap self)
        {
            _logger.LogDebug($"Awake");

            orig(self);

            var mapComponent = self.gameObject.AddComponent<SharedMapComponent>();
            mapComponent.Create(_logger, _config, self);
        }

        private void StartHook(On.Minimap.orig_Start orig, Minimap self)
        {
            _logger.LogDebug($"Start IsServer={ZNet.m_isServer}");

            orig(self);

            if (_config.Enabled && ZNet.m_isServer)
            {
                _logger.LogDebug("LoadSharedMap");
                ZNet.m_world.LoadSharedMap(_config.SharedPins);
            }
        }

        private Player SpawnPlayerHook(On.Game.orig_SpawnPlayer orig, Game self, Vector3 spawnPoint)
        {
            _logger.LogDebug($"SpawnPlayer FirstSpawn={self.m_firstSpawn} SpawnPoint={spawnPoint}");

            var player = orig(self, spawnPoint);

            if (!ZNet.m_isServer && self.m_firstSpawn)
            {
                SharedMapComponent.Instance.SendSharedMap();
            }

            return player;
        }

        private void SaveWorldMetaDataHook(On.World.orig_SaveWorldMetaData orig, World self)
        {
            orig(self);

            _logger.LogDebug("SaveSharedMap");
            self.SaveSharedMap();
        }

        private Minimap.PinData AddPinHook(
            On.Minimap.orig_AddPin orig,
            Minimap self,
            Vector3 pos,
            Minimap.PinType type,
            string name,
            bool save,
            bool isChecked)
        {
            _logger.LogDebug($"AddPin Pos={pos} Type={type} Name={name} Save={save} Checked={isChecked}");

            var pin = orig(self, pos, type, name, save, isChecked);

            if (save && SharedMapData.SharedTypes.Contains(type) && Player.m_localPlayer != null)
            {
                ZRoutedRpc.instance.InvokeRoutedRPC(
                    SharedMapComponent.RpcSharedPinAddName,
                    SharedPinData.Write(pin, new ZPackage())
                );
            }

            return pin;
        }

        private void RemovePinDataHook(On.Minimap.orig_RemovePin_PinData orig, Minimap self, Minimap.PinData pin)
        {
            _logger.LogDebug($"RemovePinData Pos={pin.m_pos} Type={pin.m_type} Name={pin.m_name}");

            orig(self, pin);

            if (pin.m_save && SharedMapData.SharedTypes.Contains(pin.m_type) && Player.m_localPlayer != null)
            {
                ZRoutedRpc.instance.InvokeRoutedRPC(
                    SharedMapComponent.RpcSharedPinRemoveName,
                    SharedPinData.Write(pin, new ZPackage())
                );
            }
        }

        private bool RemovePinRadiusHook(On.Minimap.orig_RemovePin_Vector3_float orig, Minimap self, Vector3 pos, float radius)
        {
            _logger.LogDebug($"RemovePinRadius Pos={pos} Radius={radius}");

            if (orig(self, pos, radius))
                return true;

            var pinData = self.GetClosestPinPredicate(pos, radius, pin => pin.m_save == false);
            if (_config.AdminCanRemoveSharedPins && pinData != null)
            {
                ZRoutedRpc.instance.InvokeRoutedRPC(
                    SharedMapComponent.RpcSharedPinAdminRemoveName,
                    SharedPinData.Write(pinData, new ZPackage())
                );
            }

            return false;
        }

        private void UpdateNameInputHook(On.Minimap.orig_UpdateNameInput orig, Minimap self)
        {
            var pin = self.m_namePin;
            var oldName = pin?.m_name;

            orig(self);

            if (pin == null || oldName == pin.m_name)
                return;

            _logger.LogDebug($"UpdateNameInput Pos={pin.m_pos} Type={pin.m_type} Name={pin.m_name}");

            ZRoutedRpc.instance.InvokeRoutedRPC(
                SharedMapComponent.RpcSharedPinNameUpdateName,
                SharedPinData.Write(pin, new ZPackage())
            );
        }

        private void OnMapLeftClickHook(On.Minimap.orig_OnMapLeftClick orig, Minimap self)
        {
            orig(self);

            var closestPin = self.GetClosestPin(self.ScreenToWorldPoint(Input.mousePosition), self.m_removeRadius * (self.m_largeZoom * 2f));
            if (closestPin == null)
                return;

            _logger.LogDebug($"OnMapLeftClick Pos={closestPin.m_pos} Type={closestPin.m_type} Name={closestPin.m_name}");

            ZRoutedRpc.instance.InvokeRoutedRPC(
                SharedMapComponent.RpcSharedPinCheckUpdateName,
                SharedPinData.Write(closestPin, new ZPackage())
            );
        }
    }
}