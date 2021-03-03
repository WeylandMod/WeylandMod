using BepInEx.Logging;
using UnityEngine;
using WeylandMod.Core;

namespace WeylandMod.Features.SharedMap
{
    internal class GameComponent : IFeatureComponent
    {
        private ManualLogSource Logger { get; }

        public GameComponent(ManualLogSource logger)
        {
            Logger = logger;
        }

        public void Initialize()
        {
            On.Game.SpawnPlayer += SpawnPlayerHook;
        }

        private Player SpawnPlayerHook(On.Game.orig_SpawnPlayer orig, Game self, Vector3 spawnPoint)
        {
            Logger.LogDebug($"{nameof(SharedMap)}.{nameof(GameComponent)}.SpawnPlayer " +
                            $"FirstSpawn={self.m_firstSpawn} SpawnPoint={spawnPoint}");

            var player = orig(self, spawnPoint);

            if (!ZNet.instance.IsServer() && self.m_firstSpawn)
            {
                Minimap.instance.SharedMapSend();
            }

            return player;
        }
    }
}