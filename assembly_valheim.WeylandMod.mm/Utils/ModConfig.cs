using System.IO;
using BepInEx;
using BepInEx.Configuration;

namespace WeylandMod
{
    internal class ModConfig
    {
        private static ModConfig m_instance;

        public static ModConfig Instance => m_instance;

        public readonly ConfigFile ConfigFile;

        public readonly ServerConfig Server;

        public readonly PlayerConfig Player;

        public readonly ExtendedStorageConfig ExtendedStorage;

        public static void Create()
        {
            m_instance = new ModConfig();
        }

        private ModConfig()
        {
            ConfigFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "WeylandMod.cfg"), true);

            Server = new ServerConfig
            {
                SkipPasswordValidation = ConfigFile.Bind(
                    nameof(Server),
                    "SkipPasswordValidation",
                    true,
                    "Let you launch public server without password."
                ),
            };

            Player = new PlayerConfig
            {
                ManageableDeathMarkers = ConfigFile.Bind(
                    nameof(Player),
                    "ManageableDeathMarkers",
                    true,
                    "Keep track of all your deaths and delete markers using right click."
                ),
            };

            ExtendedStorage = new ExtendedStorageConfig
            {
                Enabled = ConfigFile.Bind(
                    nameof(ExtendedStorage),
                    "Enabled",
                    false,
                    "Enable ExtendedStorage mod."
                ),
            };
        }

        public class ServerConfig
        {
            public ConfigEntry<bool> SkipPasswordValidation;
        }

        public class PlayerConfig
        {
            public ConfigEntry<bool> ManageableDeathMarkers;
        }

        public class ExtendedStorageConfig
        {
            public ConfigEntry<bool> Enabled;
        }
    }
}