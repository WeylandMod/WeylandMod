using BepInEx;
using WeylandMod.Hooks;
using WeylandMod.Utils;

namespace WeylandMod
{
    [BepInPlugin("io.github.WeylandMod", "WeylandMod", "0.1.5")]
    internal class WeylandModPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            WeylandConfig.Init(Config);
            InitHooks();
        }

        private void InitHooks()
        {
            FejdStartupHooks.Init(Logger);
            ZNetHooks.Init(Logger);
            ZSteamMatchmakingHooks.Init(Logger);
            GameHooks.Init(Logger);
            MinimapHooks.Init(Logger);
            PlayerHooks.Init(Logger);
            PlayerProfileHooks.Init(Logger);
            ContainerHooks.Init(Logger);
        }
    }
}