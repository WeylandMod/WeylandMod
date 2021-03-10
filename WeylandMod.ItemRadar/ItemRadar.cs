using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using WeylandMod.Core;

namespace WeylandMod.ItemRadar
{
    internal class ItemRadar : IFeature
    {
        public IFeatureConfig Config => _config;

        private readonly ManualLogSource _logger;
        private readonly ItemRadarConfig _config;

        public ItemRadar(ManualLogSource logger, ConfigFile config)
        {
            _logger = logger;
            _config = new ItemRadarConfig(config);
        }

        public void OnRegister()
        {
            _logger.LogDebug(
                "OnRegister " +
                $"Enabled={_config.Enabled} " +
                $"Radius={_config.Radius} " +
                $"IconSize={_config.IconSize} " +
                $"Items=({string.Join(",", _config.Items)})"
            );

            if (!_config.Enabled)
                return;

            On.ObjectDB.UpdateItemHashes += UpdateItemHashesHook;
            On.Destructible.Awake += DestructibleAwakeHook;
            On.MineRock.Start += MineRockStartHook;
            On.MineRock5.Start += MineRock5StartHook;
            On.LootSpawner.Awake += LootSpawnerAwakeHook;
            On.CharacterDrop.Start += CharacterDropStartHook;
            On.Container.Awake += ContainerAwakeHook;
            On.DropOnDestroyed.Awake += DropOnDestroyedAwakeHook;
            On.Pickable.Awake += PickableAwakeHook;
            On.Pickable.SetPicked += PickableSetPickedHook;
        }

        public void OnConnect()
        {
        }

        public void OnDisconnect()
        {
        }

        private void UpdateItemHashesHook(On.ObjectDB.orig_UpdateItemHashes orig, ObjectDB self)
        {
            orig(self);

            ItemRadarUtils.LoadIcons(_config.Items);
        }

        private void DestructibleAwakeHook(On.Destructible.orig_Awake orig, Destructible self)
        {
            orig(self);

            if (self.m_spawnWhenDestroyed == null)
                return;

            var mineRockComponent = self.m_spawnWhenDestroyed.GetComponent<MineRock5>();
            if (mineRockComponent == null)
                return;

            ItemRadarUtils.AddItemPinComponent(_config, self.gameObject, mineRockComponent.m_dropItems);
        }

        private void ContainerAwakeHook(On.Container.orig_Awake orig, Container self)
        {
            orig(self);

            ItemRadarUtils.AddItemPinComponent(_config, self.gameObject, self.m_inventory);
        }

        private void MineRockStartHook(On.MineRock.orig_Start orig, MineRock self)
        {
            orig(self);

            ItemRadarUtils.AddItemPinComponent(_config, self.gameObject, self.m_dropItems);
        }

        private void MineRock5StartHook(On.MineRock5.orig_Start orig, MineRock5 self)
        {
            orig(self);

            ItemRadarUtils.AddItemPinComponent(_config, self.gameObject, self.m_dropItems);
        }

        private void LootSpawnerAwakeHook(On.LootSpawner.orig_Awake orig, LootSpawner self)
        {
            orig(self);

            ItemRadarUtils.AddItemPinComponent(_config, self.gameObject, self.m_items);
        }

        private void CharacterDropStartHook(On.CharacterDrop.orig_Start orig, CharacterDrop self)
        {
            orig(self);

            ItemRadarUtils.AddItemPinComponent(_config, self.gameObject, self);
        }

        private void DropOnDestroyedAwakeHook(On.DropOnDestroyed.orig_Awake orig, DropOnDestroyed self)
        {
            orig(self);

            ItemRadarUtils.AddItemPinComponent(_config, self.gameObject, self.m_dropWhenDestroyed);
        }

        private void PickableAwakeHook(On.Pickable.orig_Awake orig, Pickable self)
        {
            orig(self);

            if (self.m_itemPrefab == null || self.m_picked)
                return;

            ItemRadarUtils.AddItemPinComponent(_config, self.gameObject, self.m_itemPrefab.GetComponent<ItemDrop>());
        }

        private void PickableSetPickedHook(On.Pickable.orig_SetPicked orig, Pickable self, bool picked)
        {
            orig(self, picked);

            if (!picked)
                return;

            var itemPinComponent = self.gameObject.GetComponent<ItemPinComponent>();
            if (itemPinComponent == null)
                return;

            Object.Destroy(itemPinComponent);
        }
    }
}