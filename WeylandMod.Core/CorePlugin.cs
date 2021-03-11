using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace WeylandMod.Core
{
    [BepInPlugin("WeylandMod.Core", "WeylandMod.Core", "1.0.0")]
    internal class CorePlugin : BaseUnityPlugin
    {
        private readonly IDictionary<string, IFeature> _features = new Dictionary<string, IFeature>();

        private ConfigEntry<bool> _changeVersion;

        public static bool TryGet(out CorePlugin plugin)
        {
            if (Chainloader.PluginInfos.TryGetValue("WeylandMod.Core", out var pluginInfo))
            {
                plugin = pluginInfo.Instance as CorePlugin;
            }
            else
            {
                plugin = null;
            }

            return plugin != null;
        }

        public void Awake()
        {
            Logger.LogInfo($"{Info.Metadata.Name} Location={Info.Location}");

            _changeVersion = Config.Bind(
                "Core",
                "ChangeVersion",
                true,
                "Change game version to deny connections from clients without WeylandMod."
            );

            Logger.LogDebug($"Awake ChangeVersion={_changeVersion.Value}");

            if (_changeVersion.Value)
            {
                On.Version.GetVersionString += GetVersionStringHook;
            }

            On.ZNet.Awake += AwakeHook;
            On.ZNet.OnDestroy += OnDestroyHook;

            IL.ZNet.SendPeerInfo += SendPeerInfoPatch;
            IL.ZNet.RPC_PeerInfo += RPC_PeerInfoPatch;
        }

        public void RegisterFeature(string featureName, IFeature feature)
        {
            if (_features.ContainsKey(featureName))
                return;

            Logger.LogInfo($"RegisterFeature Name={featureName}");

            _features.Add(featureName, feature);

            feature.Config.Reload();
            feature.OnRegister();
        }

        private string GetVersionStringHook(On.Version.orig_GetVersionString orig)
        {
            return $"{orig()}/WeylandMod-{Info.Metadata.Version}";
        }

        private void AwakeHook(On.ZNet.orig_Awake orig, ZNet self)
        {
            orig(self);

            if (ZNet.instance.IsServer() || ZNet.instance.IsDedicated())
            {
                // also works for single player
                foreach (IFeature feature in _features.Values)
                {
                    feature.Config.Reload();
                    feature.OnConnect();
                }
            }
        }

        private void OnDestroyHook(On.ZNet.orig_OnDestroy orig, ZNet self)
        {
            foreach (IFeature feature in _features.Values)
            {
                feature.OnDisconnect();
                feature.Config.Reload();
            }

            orig(self);
        }

        private void SendPeerInfoPatch(ILContext il)
        {
            new ILCursor(il)
                .GotoNext(
                    MoveType.After,
                    x => x.MatchLdloc(0),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<ZNet>("m_netTime"),
                    x => x.MatchCallvirt<ZPackage>("Write")
                )
                .Emit(OpCodes.Ldloc_0) // push ZPackage
                .EmitDelegate<Action<ZPackage>>(WriteServerConfig);
        }

        private void RPC_PeerInfoPatch(ILContext il)
        {
            new ILCursor(il)
                .GotoNext(
                    MoveType.After,
                    x => x.MatchLdarg(0),
                    x => x.MatchLdarg(2),
                    x => x.MatchCallvirt<ZPackage>("ReadDouble"),
                    x => x.MatchStfld<ZNet>("m_netTime")
                )
                .Emit(OpCodes.Ldarg_2) // push ZPackage
                .EmitDelegate<Action<ZPackage>>(ReadServerConfig);
        }

        private void WriteServerConfig(ZPackage pkg)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                WriteFeaturesConfig(writer);
                writer.Close();

                pkg.Write(stream.ToArray());
            }
        }

        private void ReadServerConfig(ZPackage pkg)
        {
            using (var stream = new MemoryStream(pkg.ReadByteArray()))
            using (var reader = new BinaryReader(stream))
            {
                ReadFeaturesConfig(reader);
            }

            foreach (IFeature feature in _features.Values)
            {
                feature.OnConnect();
            }
        }

        private void WriteFeaturesConfig(BinaryWriter writer)
        {
            foreach (var entry in _features)
            {
                using (var configStream = new MemoryStream())
                using (var configWriter = new BinaryWriter(configStream))
                {
                    entry.Value.Config.Write(configWriter);
                    configWriter.Close();

                    var configData = configStream.ToArray();
                    if (configData.Length == 0)
                    {
                        continue;
                    }

                    Logger.LogDebug($"WriteConfig Name={entry.Key} Length={configData.Length}");

                    writer.Write(entry.Key);
                    writer.Write(configData.Length);
                    writer.Write(configData);
                }
            }
        }

        private void ReadFeaturesConfig(BinaryReader reader)
        {
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                var featureName = reader.ReadString();
                var dataLength = reader.ReadInt32();
                var configData = reader.ReadBytes(dataLength);

                Logger.LogDebug($"ReadConfig Name={featureName} Length={dataLength}");

                if (configData.Length != dataLength)
                {
                    Logger.LogError($"Failed to read config data: {configData.Length} != {dataLength}");
                    break;
                }

                if (!_features.TryGetValue(featureName, out var feature))
                {
                    Logger.LogWarning($"Missing feature Name={featureName}");
                    continue;
                }

                using (var configStream = new MemoryStream(configData))
                using (var configReader = new BinaryReader(configStream))
                {
                    feature.Config.Read(configReader);
                }
            }
        }
    }
}