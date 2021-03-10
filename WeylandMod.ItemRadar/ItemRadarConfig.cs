using System.IO;
using System.Linq;
using BepInEx.Configuration;
using WeylandMod.Core;

namespace WeylandMod.ItemRadar
{
    internal class ItemRadarConfig : IFeatureConfig
    {
        public bool Enabled { get; private set; }
        public string[] Items { get; private set; }
        public float Radius { get; private set; }
        public float IconSize { get; private set; }

        private readonly ConfigEntry<bool> _enabled;
        private readonly ConfigEntry<string> _items;
        private readonly ConfigEntry<float> _radius;
        private readonly ConfigEntry<float> _iconSize;

        public ItemRadarConfig(ConfigFile config)
        {
            _enabled = config.Bind(
                nameof(ItemRadar),
                nameof(Enabled),
                false,
                "Show on minimap specific items within the certain radius around player."
            );

            _items = config.Bind(
                nameof(ItemRadar),
                nameof(Items),
                string.Join(",", "TinOre", "CopperOre", "SilverOre", "IronScrap", "Obsidian", "FlametalOre"),
                "Items to show on minimap."
            );

            _radius = config.Bind(
                nameof(ItemRadar),
                nameof(Radius),
                70.0f,
                "The radius of detection."
            );

            _iconSize = config.Bind(
                nameof(ItemRadar),
                nameof(IconSize),
                0.0f,
                "The size of icons on minimap (0 - default size)."
            );
        }

        public void Reload()
        {
            Enabled = _enabled.Value;
            Items = _items.Value.Split(',').Select(s => s.Trim()).Distinct().ToArray();
            Radius = _radius.Value;
            IconSize = _iconSize.Value;
        }

        public void Write(BinaryWriter writer)
        {
        }

        public void Read(BinaryReader reader)
        {
        }
    }
}