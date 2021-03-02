using BepInEx.Logging;
using UnityEngine;
using WeylandMod.Core;

namespace WeylandMod.Features.SharedMap
{
    internal class GameHooks : FeaturePart
    {
        public GameHooks(ManualLogSource logger)
            : base(logger)
        {
        }

        public override void Init()
        {
            Logger.LogDebug($"{nameof(SharedMap)}-{nameof(GameHooks)} Init");

            On.Game.SpawnPlayer += SpawnPlayerHook;
        }

        private Player SpawnPlayerHook(On.Game.orig_SpawnPlayer orig, Game self, Vector3 spawnPoint)
        {
            Logger.LogDebug($"{nameof(SharedMap)}-{nameof(GameHooks)} SpawnPlayer {self.m_firstSpawn} {spawnPoint}");

            var player = orig(self, spawnPoint);

            if (!ZNet.instance.IsServer() && self.m_firstSpawn)
            {
                Minimap.instance.SharedMapSend();
            }

            return player;
        }
    }
}