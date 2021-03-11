using System.IO;
using BepInEx.Configuration;
using WeylandMod.Core;

namespace WeylandMod.ExtendedStorage
{
    internal class ExtendedStorageConfig : IFeatureConfig
    {
        private const int Version = 1;

        public bool Enabled { get; private set; }
        public int ExtraRows { get; private set; }
        public int ExtraColumns { get; private set; }

        private readonly ConfigEntry<bool> _enabled;
        private readonly ConfigEntry<int> _extraRows;
        private readonly ConfigEntry<int> _extraColumns;

        public ExtendedStorageConfig(ConfigFile config)
        {
            _enabled = config.Bind(
                nameof(ExtendedStorage),
                nameof(Enabled),
                false,
                "Extends size of every game container (chest, wagon, ship, etc)."
            );

            _extraRows = config.Bind(
                nameof(ExtendedStorage),
                nameof(ExtraRows),
                1,
                "Extra rows for containers."
            );

            _extraColumns = config.Bind(
                nameof(ExtendedStorage),
                nameof(ExtraColumns),
                1,
                "Extra columns for containers."
            );
        }

        public void Reload()
        {
            Enabled = _enabled.Value;
            ExtraRows = _extraRows.Value;
            ExtraColumns = _extraColumns.Value;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(Enabled);
            writer.Write(ExtraRows);
            writer.Write(ExtraColumns);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32(); // Version
            Enabled = reader.ReadBoolean();
            ExtraRows = reader.ReadInt32();
            ExtraColumns = reader.ReadInt32();
        }
    }
}