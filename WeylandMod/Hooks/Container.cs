using BepInEx.Logging;
using WeylandMod.Utils;

namespace WeylandMod.Hooks
{
    internal static class ContainerHooks
    {
        private static ManualLogSource Logger;

        public static void Init(ManualLogSource logger)
        {
            Logger = logger;

            On.Container.Awake += AwakeHook;
        }

        private static void AwakeHook(On.Container.orig_Awake orig, Container self)
        {
            if (WeylandConfig.ExtendedStorage.Enabled.Value)
            {
                self.m_width += 1;
                self.m_height += 1;
            }

            orig(self);
        }
    }
}