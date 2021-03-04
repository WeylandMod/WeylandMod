using BepInEx;

namespace WeylandMod
{
    [BepInPlugin("io.github.WeylandMod", "WeylandMod", "0.1.6")]
    internal class WeylandModPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            FeatureManager.Launch(Logger, Config);
        }
    }
}