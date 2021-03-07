using BepInEx.Logging;
using UnityEngine;
using WeylandMod.Core;

namespace WeylandMod.Features.ItemRadar
{
    internal class ItemDropComponent : IFeatureComponent
    {
        private readonly ManualLogSource _logger;
        private readonly ItemRadarConfig _config;

        public ItemDropComponent(ManualLogSource logger, ItemRadarConfig config)
        {
            _logger = logger;
            _config = config;
        }

        public void OnLaunch(bool enabled)
        {
        }

        public void OnConnect()
        {
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

        public void OnDisconnect()
        {
            On.Destructible.Awake -= DestructibleAwakeHook;
            On.MineRock.Start -= MineRockStartHook;
            On.MineRock5.Start -= MineRock5StartHook;
            On.LootSpawner.Awake -= LootSpawnerAwakeHook;
            On.CharacterDrop.Start -= CharacterDropStartHook;
            On.Container.Awake -= ContainerAwakeHook;
            On.DropOnDestroyed.Awake -= DropOnDestroyedAwakeHook;
            On.Pickable.Awake -= PickableAwakeHook;
            On.Pickable.SetPicked -= PickableSetPickedHook;
        }

        private void DestructibleAwakeHook(On.Destructible.orig_Awake orig, Destructible self)
        {
            orig(self);

            if (self.m_spawnWhenDestroyed == null)
                return;

            var mineRockComponent = self.m_spawnWhenDestroyed.GetComponent<MineRock5>();
            if (mineRockComponent == null)
                return;

            OreRadarUtils.AddPinComponent(_config, self.gameObject, mineRockComponent.m_dropItems);
        }

        private void ContainerAwakeHook(On.Container.orig_Awake orig, Container self)
        {
            orig(self);

            OreRadarUtils.AddPinComponent(_config, self.gameObject, self.m_inventory);
        }

        private void MineRockStartHook(On.MineRock.orig_Start orig, MineRock self)
        {
            orig(self);

            OreRadarUtils.AddPinComponent(_config, self.gameObject, self.m_dropItems);
        }

        private void MineRock5StartHook(On.MineRock5.orig_Start orig, MineRock5 self)
        {
            orig(self);

            OreRadarUtils.AddPinComponent(_config, self.gameObject, self.m_dropItems);
        }

        private void LootSpawnerAwakeHook(On.LootSpawner.orig_Awake orig, LootSpawner self)
        {
            orig(self);

            OreRadarUtils.AddPinComponent(_config, self.gameObject, self.m_items);
        }

        private void CharacterDropStartHook(On.CharacterDrop.orig_Start orig, CharacterDrop self)
        {
            orig(self);

            OreRadarUtils.AddPinComponent(_config, self.gameObject, self);
        }

        private void DropOnDestroyedAwakeHook(On.DropOnDestroyed.orig_Awake orig, DropOnDestroyed self)
        {
            orig(self);

            OreRadarUtils.AddPinComponent(_config, self.gameObject, self.m_dropWhenDestroyed);
        }

        private void PickableAwakeHook(On.Pickable.orig_Awake orig, Pickable self)
        {
            orig(self);

            if (self.m_itemPrefab == null || self.m_picked)
                return;

            OreRadarUtils.AddPinComponent(_config, self.gameObject, self.m_itemPrefab.GetComponent<ItemDrop>());
        }

        private static void PickableSetPickedHook(On.Pickable.orig_SetPicked orig, Pickable self, bool picked)
        {
            orig(self, picked);

            if (!picked)
                return;

            var pinComponent = self.gameObject.GetComponent<PinComponent>();
            if (pinComponent == null)
                return;

            Object.Destroy(pinComponent);
        }
    }
}