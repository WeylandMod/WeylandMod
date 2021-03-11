using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WeylandMod.ItemRadar
{
    internal static class ItemRadarUtils
    {
        private static readonly IDictionary<string, Sprite> ItemIcons = new Dictionary<string, Sprite>();

        public static void LoadIcons(IEnumerable<string> itemNames)
        {
            ItemIcons.Clear();

            foreach (string itemName in itemNames)
            {
                GameObject prefab = ObjectDB.instance.GetItemPrefab(itemName.GetStableHashCode());
                if (prefab == null)
                    continue;

                var itemDrop = prefab.GetComponent<ItemDrop>();
                if (itemDrop == null)
                    continue;

                ItemIcons.Add(
                    itemDrop.m_itemData.m_shared.m_name,
                    itemDrop.m_itemData.m_shared.m_icons[0]
                );
            }
        }

        public static void AddItemPinComponent(ItemRadarConfig config, GameObject gameObject, DropTable dropTable)
        {
            AddItemPinComponent(config, gameObject, GetDroppedItem(dropTable.m_drops));
        }

        public static void AddItemPinComponent(ItemRadarConfig config, GameObject gameObject, Inventory inventory)
        {
            AddItemPinComponent(config, gameObject, GetDroppedItem(inventory.GetAllItems()));
        }

        public static void AddItemPinComponent(ItemRadarConfig config, GameObject gameObject, CharacterDrop characterDrop)
        {
            AddItemPinComponent(config, gameObject, GetDroppedItem(characterDrop.m_drops));
        }

        public static void AddItemPinComponent(ItemRadarConfig config, GameObject gameObject, ItemDrop itemDrop)
        {
            if (itemDrop == null)
                return;

            var itemName = itemDrop.m_itemData.m_shared.m_name;
            if (!ItemIcons.ContainsKey(itemName))
                return;

            var itemPinComponent = gameObject.AddComponent<ItemPinComponent>();
            itemPinComponent.Create(itemName, config.Radius, config.IconSize, ItemIcons[itemName]);
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
                .FirstOrDefault(drop => ItemIcons.ContainsKey(drop.m_itemData.m_shared.m_name));
        }
    }
}