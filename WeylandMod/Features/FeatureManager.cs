using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using BepInEx.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using WeylandMod.Core;
using WeylandMod.Features.ExtendedStorage;
using WeylandMod.Features.ManageableDeathPins;
using WeylandMod.Features.NoServerPassword;
using WeylandMod.Features.PermittedPlayersNoPassword;
using WeylandMod.Features.PreciseRotation;
using WeylandMod.Features.SharedMap;
using WeylandMod.Utils;

namespace WeylandMod
{
    internal static class FeatureManager
    {
        private static ManualLogSource _logger;
        private static IDictionary<string, IFeature> _features;

        public static void Launch(ManualLogSource logger, ConfigFile config)
        {
            _logger = logger;
            _features = new IFeature[]
            {
                new NoServerPassword(logger, config),
                new PermittedPlayersNoPassword(logger, config),
                new SharedMap(logger, config),
                new ExtendedStorage(logger, config),
                new ManageableDeathPins(logger, config),
                new PreciseRotation(logger, config)
            }.ToDictionary(feature => feature.Name);

            IL.ZNet.RPC_ServerHandshake += RPC_ServerHandshakePatch;
            // IL.ZNet.OnNewConnection += OnNewConnectionPatch;

            On.ZNet.OnNewConnection += OnNewConnectionHook;
            On.ZNet.Awake += AwakeHook;
            On.ZNet.OnDestroy += OnDestroyHook;

            ForEachFeatureComponent((feature, component) =>
            {
                _logger.LogDebug($"{feature.GetType().Name}.{component.GetType().Name}.OnLaunch");
                component.OnLaunch(feature.Config.Enabled);
            });
        }

        private static void RPC_ServerHandshakePatch(ILContext il)
        {
            new ILCursor(il)
                .GotoNext(
                    x => x.MatchLdloc(0),
                    x => x.MatchLdfld<ZNetPeer>("m_rpc"),
                    x => x.MatchLdstr("ClientHandshake"),
                    x => x.MatchLdcI4(1),
                    x => x.MatchNewarr<Object>(),
                    x => x.MatchDup(),
                    x => x.MatchLdcI4(0),
                    x => x.MatchLdloc(1),
                    x => x.MatchBox<Boolean>(),
                    x => x.MatchStelemRef(),
                    x => x.MatchCallvirt<ZRpc>("Invoke"))
                .RemoveRange(11)
                .Emit(OpCodes.Ldloc_0) // push peer
                .Emit(OpCodes.Ldloc_1) // push needServer
                .EmitDelegate<Action<ZNetPeer, bool>>((peer, needServer) =>
                    peer.m_rpc.Invoke(
                        WeylandRpc.GetName("ClientConfig"),
                        needServer,
                        GetServerConfig()
                    ));
        }

        private static void OnNewConnectionHook(On.ZNet.orig_OnNewConnection orig, ZNet self, ZNetPeer peer)
        {
            orig(self, peer);

            if (ZNet.m_isServer)
                return;

            peer.m_rpc.Register<bool, ZPackage>(
                WeylandRpc.GetName("ClientConfig"),
                RPC_ClientConfig
            );
        }

        private static void AwakeHook(On.ZNet.orig_Awake orig, ZNet self)
        {
            orig(self);

            ForEachFeatureComponent((feature, component) =>
            {
                // also works for single player
                if (feature.Config.Enabled && (ZNet.instance.IsServer() || ZNet.instance.IsDedicated()))
                {
                    _logger.LogDebug($"{feature.GetType().Name}.{component.GetType().Name}.OnConnect");
                    component.OnConnect();
                }
            });
        }

        private static void OnDestroyHook(On.ZNet.orig_OnDestroy orig, ZNet self)
        {
            ForEachFeatureComponent((feature, component) =>
            {
                if (feature.Config.Enabled)
                {
                    _logger.LogDebug($"{feature.GetType().Name}.{component.GetType().Name}.OnDisconnect");
                    component.OnDisconnect();
                }
            });

            foreach (IFeature feature in _features.Values)
            {
                _logger.LogDebug($"{feature.GetType().Name}.Config.Reload");
                feature.Config.Reload();
            }

            orig(self);
        }

        private static ZPackage GetServerConfig()
        {
            var pkg = new ZPackage();
            pkg.Write(_features.Count);

            foreach (IFeature feature in _features.Values)
            {
                _logger.LogDebug($"{feature.GetType().Name}.Config.Write");

                pkg.Write(feature.Name);

                var config = new ZPackage();
                feature.Config.Write(config);
                pkg.Write(config);
            }

            return pkg;
        }

        private static void RPC_ClientConfig(ZRpc rpc, bool needPassword, ZPackage pkg)
        {
            _logger.LogDebug("Server config received");
            ZNet.instance.RPC_ClientHandshake(rpc, needPassword);

            var length = pkg.ReadInt();
            for (var index = 0; index < length; ++index)
            {
                var name = pkg.ReadString();
                var config = pkg.ReadPackage();
                if (!_features.TryGetValue(name, out IFeature feature))
                {
                    _logger.LogWarning($"Failed to find feature {name}");
                    continue;
                }

                _logger.LogDebug($"{feature.GetType().Name}.Config.Read");
                feature.Config.Read(config);

                foreach (IFeatureComponent component in feature.Components)
                {
                    if (feature.Config.Enabled)
                    {
                        _logger.LogDebug($"{feature.GetType().Name}.{component.GetType().Name}.OnConnect");
                        component.OnConnect();
                    }
                }
            }
        }

        private static void ForEachFeatureComponent(Action<IFeature, IFeatureComponent> componentAction)
        {
            foreach (IFeature feature in _features.Values)
            {
                foreach (IFeatureComponent component in feature.Components)
                {
                    componentAction(feature, component);
                }
            }
        }
    }
}