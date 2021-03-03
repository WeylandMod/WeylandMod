using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using UnityEngine.UI;
using WeylandMod.Core;
using WeylandMod.Utils;
using Object = UnityEngine.Object;

namespace WeylandMod.Features.SharedMap
{
    internal class MinimapComponent : IFeatureComponent
    {
        private ManualLogSource Logger { get; }

        private ConfigEntry<bool> SharedPins { get; }
        private ConfigEntry<Color> SharedPinsColor { get; }

        private List<ZNet.PlayerInfo> _playersInfo;
        private float _exploreTimer;
        private GameObject _customPinPrefab;

        public MinimapComponent(ManualLogSource logger, ConfigFile config)
        {
            Logger = logger;
            MinimapExt.Logger = logger;

            SharedPins = config.Bind(
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

        public void Initialize()
        {
            _playersInfo = new List<ZNet.PlayerInfo>();
            _exploreTimer = 0.0f;
            _customPinPrefab = null;

            On.Minimap.Start += StartHook;
            On.Minimap.Update += UpdateHook;

            if (SharedPins.Value)
            {
                On.Minimap.AddPin += AddPinHook;
                On.Minimap.RemovePin_PinData += RemovePinHook;
                On.Minimap.UpdateNameInput += UpdateNameInputHook;

                IL.Minimap.UpdatePins += UpdatePinsHook;
            }
        }

        private void StartHook(On.Minimap.orig_Start orig, Minimap self)
        {
            Logger.LogDebug($"{nameof(SharedMap)}.{nameof(MinimapComponent)}.Start Server={ZNet.m_isServer}");

            orig(self);

            if (SharedPins.Value)
            {
                _customPinPrefab = Object.Instantiate(self.m_pinPrefab);
                var pinImage = _customPinPrefab.GetComponent<Image>();

                pinImage.material = new Material(pinImage.material);
                pinImage.color = SharedPinsColor.Value;
            }

            ZNet.m_world.LoadSharedMap();

            ZRoutedRpc.instance.Register<ZPackage>(
                WeylandRpc.GetName("SharedMapUpdate"),
                (sender, package) => { self.RPC_SharedMapUpdate(sender, package); }
            );

            ZRoutedRpc.instance.Register<ZPackage>(
                WeylandRpc.GetName("SharedPinAdd"),
                (sender, package) => { self.RPC_SharedPinAdd(sender, package); }
            );

            ZRoutedRpc.instance.Register<ZPackage>(
                WeylandRpc.GetName("SharedPinRemove"),
                (sender, package) => { self.RPC_SharedPinRemove(sender, package); }
            );

            ZRoutedRpc.instance.Register<ZPackage>(
                WeylandRpc.GetName("SharedPinNameUpdate"),
                (sender, package) => { self.RPC_SharedPinNameUpdate(sender, package); }
            );
        }

        private void UpdateHook(On.Minimap.orig_Update orig, Minimap self)
        {
            orig(self);

            _exploreTimer += Time.deltaTime;
            if (_exploreTimer <= self.m_exploreInterval)
                return;

            _exploreTimer = 0.0f;

            _playersInfo.Clear();
            ZNet.instance.GetOtherPublicPlayers(_playersInfo);

            foreach (var playerInfo in _playersInfo)
            {
                self.Explore(playerInfo.m_position, self.m_exploreRadius);
            }
        }

        private Minimap.PinData AddPinHook(
            On.Minimap.orig_AddPin orig, Minimap self, Vector3 pos, Minimap.PinType type,
            string name, bool save, bool isChecked)
        {
            Logger.LogDebug($"{nameof(SharedMap)}.{nameof(MinimapComponent)}.AddPin " +
                            $"Pos={pos} Type={type} Name={name} Save={save} Checked={isChecked}");

            var pin = orig(self, pos, type, name, save, isChecked);

            if (save && MinimapExt.SharedPinTypes.Contains(type) && Player.m_localPlayer != null)
            {
                var pkg = new ZPackage();
                pkg.Write(0); // version
                SharedPinData.Write(pin, pkg);

                ZRoutedRpc.instance.InvokeRoutedRPC(WeylandRpc.GetName("SharedPinAdd"), pkg);
            }

            return pin;
        }

        private void RemovePinHook(On.Minimap.orig_RemovePin_PinData orig, Minimap self, Minimap.PinData pin)
        {
            Logger.LogDebug($"{nameof(SharedMap)}.{nameof(MinimapComponent)}.RemovePin " +
                            $"Pos={pin.m_pos} Type={pin.m_type} Name={pin.m_name}");

            orig(self, pin);

            if (pin.m_save && MinimapExt.SharedPinTypes.Contains(pin.m_type) && Player.m_localPlayer != null)
            {
                var pkg = new ZPackage();
                pkg.Write(0); // version
                SharedPinData.Write(pin, pkg);

                ZRoutedRpc.instance.InvokeRoutedRPC(WeylandRpc.GetName("SharedPinRemove"), pkg);
            }
        }

        private void UpdateNameInputHook(On.Minimap.orig_UpdateNameInput orig, Minimap self)
        {
            var pin = self.m_namePin;
            var oldName = pin?.m_name;

            orig(self);

            if (pin == null || oldName == pin.m_name)
                return;

            Logger.LogDebug($"{nameof(SharedMap)}.{nameof(MinimapComponent)}.UpdateNameInput " +
                            $"Pos={pin.m_pos} Type={pin.m_type} Name={pin.m_name}");

            var pkg = new ZPackage();
            pkg.Write(0); // version
            SharedPinData.Write(pin, pkg);

            ZRoutedRpc.instance.InvokeRoutedRPC(WeylandRpc.GetName("SharedPinNameUpdate"), pkg);
        }

        private void UpdatePinsHook(ILContext il)
        {
            Logger.LogDebug($"{nameof(SharedMap)}.{nameof(MinimapComponent)}.UpdatePins");

            new ILCursor(il).GotoNext(x => x.MatchLdfld<Minimap>("m_pinPrefab"))
                .Remove()
                .Emit(OpCodes.Ldloc, 4) // push pin, Minimap.this already on stack
                .EmitDelegate<Func<Minimap, Minimap.PinData, GameObject>>(GetPinPrefab);
        }

        private GameObject GetPinPrefab(Minimap self, Minimap.PinData data)
        {
            if (!data.m_save && MinimapExt.SharedPinTypes.Contains(data.m_type))
            {
                return _customPinPrefab;
            }

            return self.m_pinPrefab;
        }
    }
}