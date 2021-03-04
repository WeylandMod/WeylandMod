using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.ExtendedStorage
{
    internal class ContainerComponent : IFeatureComponent
    {
        private readonly ManualLogSource _logger;

        public ContainerComponent(ManualLogSource logger)
        {
            _logger = logger;
        }

        public void OnLaunch(bool enabled)
        {
        }

        public void OnConnect()
        {
            On.Container.Awake += AwakeHook;
        }

        public void OnDisconnect()
        {
            On.Container.Awake -= AwakeHook;
        }

        private static void AwakeHook(On.Container.orig_Awake orig, Container self)
        {
            self.m_width += 1;
            self.m_height += 1;

            orig(self);
        }
    }
}