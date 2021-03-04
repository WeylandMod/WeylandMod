using BepInEx.Logging;
using UnityEngine;
using WeylandMod.Core;

namespace WeylandMod.Features.SharedMap
{
    internal class GameComponent : IFeatureComponent
    {
        private readonly ManualLogSource _logger;

        public GameComponent(ManualLogSource logger)
        {
            _logger = logger;
        }

        public void OnLaunch(bool enabled)
        {
        }

        public void OnConnect()
        {
            On.Game.SpawnPlayer += SpawnPlayerHook;
        }

        public void OnDisconnect()
        {
            On.Game.SpawnPlayer -= SpawnPlayerHook;
        }

        private Player SpawnPlayerHook(On.Game.orig_SpawnPlayer orig, Game self, Vector3 spawnPoint)
        {
            _logger.LogDebug($"{nameof(SharedMap)}.{nameof(GameComponent)}.SpawnPlayer " +
                             $"FirstSpawn={self.m_firstSpawn} SpawnPoint={spawnPoint}");

            var player = orig(self, spawnPoint);

            if (!ZNet.m_isServer && self.m_firstSpawn)
            {
                Minimap.instance.SharedMapSend();
            }

            return player;
        }
    }
}