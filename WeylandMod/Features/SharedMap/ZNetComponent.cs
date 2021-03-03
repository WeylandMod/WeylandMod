using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.SharedMap
{
    internal class ZNetComponent : IFeatureComponent
    {
        private ManualLogSource Logger { get; }

        public ZNetComponent(ManualLogSource logger)
        {
            Logger = logger;
        }

        public void Initialize()
        {
            On.ZNet.Awake += AwakeHook;
            On.ZNet.SetPublicReferencePosition += SetPublicReferencePositionHook;
        }

        private void AwakeHook(On.ZNet.orig_Awake orig, ZNet self)
        {
            Logger.LogDebug($"{nameof(SharedMap)}.{nameof(ZNetComponent)}.Awake");

            orig(self);

            self.m_publicReferencePosition = true;
        }

        private static void SetPublicReferencePositionHook(On.ZNet.orig_SetPublicReferencePosition orig, ZNet self, bool pub)
        {
        }
    }
}