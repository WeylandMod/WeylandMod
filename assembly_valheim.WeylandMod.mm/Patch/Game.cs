using MonoMod;
using UnityEngine;

#pragma warning disable CS0626
#pragma warning disable CS0649

namespace WeylandMod
{
    [MonoModPatch("global::Game")]
    internal class PatchGame : Game
    {
        [MonoModIgnore] private bool m_firstSpawn;

        private extern Player orig_SpawnPlayer(Vector3 spawnPoint);

        private Player SpawnPlayer(Vector3 spawnPoint)
        {
            ModLogger.LogDebug($"{nameof(Game)}: SpawnPlayer FirstSpawn={m_firstSpawn} SpawnPoint={spawnPoint}");
            var player = orig_SpawnPlayer(spawnPoint);

            if (!ZNet.instance.IsServer() && m_firstSpawn)
            {
                ((PatchMinimap) Minimap.instance).SharedMap_Update();
            }

            return player;
        }
    }
}