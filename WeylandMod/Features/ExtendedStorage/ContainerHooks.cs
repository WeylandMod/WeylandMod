using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.ExtendedStorage
{
    internal class ContainerHooks : FeaturePart
    {
        public ContainerHooks(ManualLogSource logger)
            : base(logger)
        {
        }

        public override void Init()
        {
            Logger.LogDebug($"{nameof(ExtendedStorage)}-{nameof(ContainerHooks)} Init");

            On.Container.Awake += AwakeHook;
        }

        private void AwakeHook(On.Container.orig_Awake orig, Container self)
        {
            Logger.LogDebug($"{nameof(ExtendedStorage)}-{nameof(ContainerHooks)} Awake");

            self.m_width += 1;
            self.m_height += 1;

            orig(self);
        }
    }
}