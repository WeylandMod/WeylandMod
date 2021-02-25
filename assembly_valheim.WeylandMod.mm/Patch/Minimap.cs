using System.Collections.Generic;
using MonoMod;
using UnityEngine;

#pragma warning disable CS0626
#pragma warning disable CS0649

namespace WeylandMod
{
    [MonoModPatch("global::Minimap")]
    internal class PatchMinimap : Minimap
    {
        [MonoModIgnore] private Texture2D m_fogTexture;
        [MonoModIgnore] private bool[] m_explored;

        private List<ZNet.PlayerInfo> m_SharedMap_playersInfo;
        private float m_SharedMap_exploreTimer;

        [MonoModIgnore]
        private extern bool Explore(int x, int y);

        [MonoModIgnore]
        private extern void Explore(Vector3 p, float radius);

        private extern void orig_Start();
        private extern void orig_Update();

        private void Start()
        {
            Debug.Log($"{nameof(Minimap)}: Start IsServer={ZNet.instance.IsServer()}");
            orig_Start();

            m_SharedMap_playersInfo = new List<ZNet.PlayerInfo>();
            m_SharedMap_exploreTimer = 0.0f;

            if (ZNet.instance.IsServer())
            {
                ZRoutedRpc.instance.Register<ZPackage>("SharedMap_Update", RPC_SharedMap_Update);
            }
            else
            {
                ZRoutedRpc.instance.Register<ZPackage>("SharedMap_Apply", RPC_SharedMap_Apply);
            }
        }

        private void Update()
        {
            orig_Update();

            m_SharedMap_exploreTimer += Time.deltaTime;
            if (m_SharedMap_exploreTimer <= m_exploreInterval)
                return;

            m_SharedMap_exploreTimer = 0.0f;

            m_SharedMap_playersInfo.Clear();
            ZNet.instance.GetOtherPublicPlayers(m_SharedMap_playersInfo);

            foreach (var playerInfo in m_SharedMap_playersInfo)
            {
                Explore(playerInfo.m_position, m_exploreRadius);
            }
        }

        public void SharedMap_Update()
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(
                "SharedMap_Update",
                new ZPackage(MapCompression.Compress(m_explored))
            );
        }

        private void RPC_SharedMap_Update(long sender, ZPackage mapData)
        {
            Debug.Log($"{nameof(Minimap)}: RPC_SharedMap_Update Sender={sender} MapDataSize={mapData.Size()}");

            var explored = MapCompression.Decompress(mapData.GetArray());
            if (explored.Length != m_explored.Length)
            {
                Debug.LogError($"{nameof(Minimap)}: RPC_SharedMap_Update invalid map data");
                return;
            }

            for (var index = 0; index < explored.Length; ++index)
            {
                // server side m_fogTexture can be ignored
                m_explored[index] = m_explored[index] || explored[index];
            }

            ZRoutedRpc.instance.InvokeRoutedRPC(
                ZRoutedRpc.Everybody,
                "SharedMap_Apply",
                new ZPackage(MapCompression.Compress(m_explored))
            );
        }

        private void RPC_SharedMap_Apply(long sender, ZPackage mapData)
        {
            Debug.Log($"{nameof(Minimap)}: RPC_SharedMap_Apply Sender={sender} MapDataSize={mapData.Size()}");

            var explored = MapCompression.Decompress(mapData.GetArray());
            if (explored.Length != m_explored.Length)
            {
                Debug.LogError($"{nameof(Minimap)}: RPC_SharedMap_Apply invalid map data");
                return;
            }

            for (var index = 0; index < explored.Length; ++index)
            {
                if (explored[index])
                {
                    Explore(index % m_textureSize, index / m_textureSize);
                }
            }

            m_fogTexture.Apply();
        }
    }
}