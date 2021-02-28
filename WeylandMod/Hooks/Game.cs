using BepInEx.Logging;
using UnityEngine;

namespace WeylandMod.Hooks
{
    internal static class GameHooks
    {
        private static ManualLogSource Logger;

        public static void Init(ManualLogSource logger)
        {
            Logger = logger;

            On.Game.SpawnPlayer += SpawnPlayerHook;
        }

        private static Player SpawnPlayerHook(On.Game.orig_SpawnPlayer orig, Game self, Vector3 spawnPoint)
        {
            Logger.LogDebug($"{nameof(Game)}: SpawnPlayer FirstSpawn={self.m_firstSpawn} SpawnPoint={spawnPoint}");

            var player = orig(self, spawnPoint);

            if (!ZNet.instance.IsServer() && self.m_firstSpawn)
            {
                Minimap.instance.SharedMapSend();
            }

            return player;
        }
    }
}