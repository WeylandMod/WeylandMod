using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;
using WeylandMod.Utils;

namespace WeylandMod.Hooks
{
    internal static class MinimapHooks
    {
        private static ManualLogSource Logger;

        private static List<ZNet.PlayerInfo> m_playersInfo;
        private static float m_exploreTimer;

        public static void Init(ManualLogSource logger)
        {
            MinimapExt.Init(logger);

            Logger = logger;

            On.Minimap.Start += StartHook;
            On.Minimap.Update += UpdateHook;
        }

        private static void StartHook(On.Minimap.orig_Start orig, Minimap self)
        {
            Logger.LogDebug($"{nameof(Minimap)}: Start IsServer={ZNet.instance.IsServer()}");
            orig(self);

            m_playersInfo = new List<ZNet.PlayerInfo>();
            m_exploreTimer = 0.0f;

            if (ZNet.instance.IsServer())
            {
                ZRoutedRpc.instance.Register<ZPackage>("WeylandMod_SharedMapUpdate",
                    (sender, package) => { self.RPC_SharedMapUpdate(sender, package); }
                );
            }
            else
            {
                ZRoutedRpc.instance.Register<ZPackage>("WeylandMod_SharedMapApply",
                    (sender, package) => { self.RPC_SharedMapApply(sender, package); }
                );
            }
        }

        private static void UpdateHook(On.Minimap.orig_Update orig, Minimap self)
        {
            orig(self);

            m_exploreTimer += Time.deltaTime;
            if (m_exploreTimer <= self.m_exploreInterval)
                return;

            m_exploreTimer = 0.0f;

            m_playersInfo.Clear();
            ZNet.instance.GetOtherPublicPlayers(m_playersInfo);

            foreach (var playerInfo in m_playersInfo)
            {
                self.Explore(playerInfo.m_position, self.m_exploreRadius);
            }
        }

        public static void SharedMapUpdate()
        {
            var compressedMap = new ZPackage(MapCompression.Compress(Minimap.instance.m_explored));
            ZRoutedRpc.instance.InvokeRoutedRPC("WeylandMod_SharedMapUpdate", compressedMap);
        }
    }

    internal static class MinimapExt
    {
        private static ManualLogSource Logger;

        public static void Init(ManualLogSource logger)
        {
            Logger = logger;
        }

        public static void RPC_SharedMapUpdate(this Minimap self, long sender, ZPackage mapData)
        {
            Logger.LogDebug($"{nameof(Minimap)}: RPC_SharedMap_Update Sender={sender} MapDataSize={mapData.Size()}");

            var explored = MapCompression.Decompress(mapData.GetArray());
            if (explored.Length != self.m_explored.Length)
            {
                Logger.LogError($"{nameof(Minimap)}: RPC_SharedMap_Update invalid map data");
                return;
            }

            for (var index = 0; index < explored.Length; ++index)
            {
                // server side m_fogTexture can be ignored
                self.m_explored[index] = self.m_explored[index] || explored[index];
            }

            var compressedMap = new ZPackage(MapCompression.Compress(self.m_explored));
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "WeylandMod_SharedMapApply", compressedMap);
        }

        public static void RPC_SharedMapApply(this Minimap self, long sender, ZPackage mapData)
        {
            Logger.LogDebug($"{nameof(Minimap)}: RPC_SharedMap_Apply Sender={sender} MapDataSize={mapData.Size()}");

            var explored = MapCompression.Decompress(mapData.GetArray());
            if (explored.Length != self.m_explored.Length)
            {
                Logger.LogError($"{nameof(Minimap)}: RPC_SharedMap_Apply invalid map data");
                return;
            }

            for (var index = 0; index < explored.Length; ++index)
            {
                if (explored[index])
                {
                    self.Explore(index % self.m_textureSize, index / self.m_textureSize);
                }
            }

            self.m_fogTexture.Apply();
        }
    }
}