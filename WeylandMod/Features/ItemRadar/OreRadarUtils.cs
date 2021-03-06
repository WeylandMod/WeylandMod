using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WeylandMod.Features.ItemRadar
{
    internal static class OreRadarUtils
    {
        private static readonly IDictionary<string, Sprite> Items = new Dictionary<string, Sprite>();

        public static void LoadIcons(string[] itemNames)
        {
            Items.Clear();

            foreach (string itemName in itemNames)
            {
                if (Items.ContainsKey(itemName))
                    continue;

                GameObject prefab = ObjectDB.instance.GetItemPrefab(itemName.GetStableHashCode());
                if (prefab == null)
                    continue;

                var itemDrop = prefab.GetComponent<ItemDrop>();
                if (itemDrop == null)
                    continue;

                Items.Add(
                    itemDrop.m_itemData.m_shared.m_name,
                    itemDrop.m_itemData.m_shared.m_icons[0]
                );
            }
        }

        public static void AddPinComponent(ItemRadarConfig config, GameObject gameObject, DropTable dropTable)
        {
            AddPinComponent(config, gameObject, GetDroppedItem(dropTable.m_drops));
        }

        public static void AddPinComponent(ItemRadarConfig config, GameObject gameObject, Inventory inventory)
        {
            AddPinComponent(config, gameObject, GetDroppedItem(inventory.GetAllItems()));
        }

        public static void AddPinComponent(ItemRadarConfig config, GameObject gameObject, CharacterDrop characterDrop)
        {
            AddPinComponent(config, gameObject, GetDroppedItem(characterDrop.m_drops));
        }

        public static void AddPinComponent(ItemRadarConfig config, GameObject gameObject, ItemDrop itemDrop)
        {
            if (itemDrop == null)
                return;

            var itemName = itemDrop.m_itemData.m_shared.m_name;
            if (!Items.ContainsKey(itemName))
                return;

            var pinComponent = gameObject.AddComponent<PinComponent>();
            pinComponent.Create(itemName, config.Radius, config.IconSize, Items[itemName]);
        }

        private static ItemDrop GetDroppedItem(IEnumerable<DropTable.DropData> drops)
        {
            return GetDroppedItem(drops.Select(data => data.m_item.GetComponent<ItemDrop>()));
        }

        private static ItemDrop GetDroppedItem(IEnumerable<ItemDrop.ItemData> drops)
        {
            return GetDroppedItem(drops.Select(data => data.m_dropPrefab.GetComponent<ItemDrop>()));
        }

        private static ItemDrop GetDroppedItem(IEnumerable<CharacterDrop.Drop> drops)
        {
            return GetDroppedItem(drops.Select(data => data.m_prefab.GetComponent<ItemDrop>()));
        }

        private static ItemDrop GetDroppedItem(IEnumerable<ItemDrop> drops)
        {
            return drops
                .Where(drop => drop.m_itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Material)
                .FirstOrDefault(drop => Items.ContainsKey(drop.m_itemData.m_shared.m_name));
        }
    }
}