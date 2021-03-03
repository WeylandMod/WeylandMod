using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.ExtendedStorage
{
    internal class ContainerComponent : IFeatureComponent
    {
        private ManualLogSource Logger { get; }

        public ContainerComponent(ManualLogSource logger)
        {
            Logger = logger;
        }

        public void Initialize()
        {
            On.Container.Awake += AwakeHook;
        }

        private static void AwakeHook(On.Container.orig_Awake orig, Container self)
        {
            self.m_width += 1;
            self.m_height += 1;

            orig(self);
        }
    }
}