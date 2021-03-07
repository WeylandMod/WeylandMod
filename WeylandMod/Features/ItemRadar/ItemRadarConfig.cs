using System.Linq;
using BepInEx.Configuration;
using WeylandMod.Core;

namespace WeylandMod.Features.ItemRadar
{
    internal class ItemRadarConfig : IFeatureConfig
    {
        private const int Version = 1;

        private readonly ConfigEntry<bool> _enabled;
        private readonly ConfigEntry<string> _items;
        private readonly ConfigEntry<float> _radius;
        private readonly ConfigEntry<float> _iconSize;

        public bool Enabled { get; private set; }
        public string[] Items { get; private set; }
        public float Radius { get; private set; }
        public float IconSize { get; private set; }

        public ItemRadarConfig(string name, ConfigFile config)
        {
            _enabled = config.Bind(
                name,
                nameof(Enabled),
                true,
                "Show on minimap specific items within the certain radius around player."
            );

            _items = config.Bind(
                name,
                nameof(Items),
                string.Join(",", "TinOre", "CopperOre", "SilverOre", "IronScrap", "Obsidian", "FlametalOre"),
                "Items to show on minimap."
            );

            _radius = config.Bind(
                name,
                nameof(Radius),
                70.0f,
                "The radius of detection."
            );

            _iconSize = config.Bind(
                name,
                nameof(IconSize),
                0.0f,
                "The size of icons on minimap (0.0 - default size)."
            );

            Reload();
        }

        public void Reload()
        {
            Enabled = _enabled.Value;
            Items = _items.Value.Split(',').Select(s => s.Trim()).ToArray();
            Radius = _radius.Value;
            IconSize = _iconSize.Value;
        }

        public void Read(ZPackage pkg)
        {
            var version = pkg.ReadInt();
        }

        public void Write(ZPackage pkg)
        {
            pkg.Write(Version);
        }
    }
}