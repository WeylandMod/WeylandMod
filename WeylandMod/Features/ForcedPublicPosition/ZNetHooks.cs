using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.ForcedPublicPosition
{
    internal class ZNetHooks : FeaturePart
    {
        public ZNetHooks(ManualLogSource logger)
            : base(logger)
        {
        }

        public override void Init()
        {
            Logger.LogDebug($"{nameof(ForcedPublicPosition)}-{nameof(ZNetHooks)} Init");

            On.ZNet.Awake += AwakeHook;
            On.ZNet.SetPublicReferencePosition += SetPublicReferencePositionHook;
        }

        private void AwakeHook(On.ZNet.orig_Awake orig, ZNet self)
        {
            Logger.LogDebug($"{nameof(ForcedPublicPosition)}-{nameof(ZNetHooks)} Awake");

            orig(self);

            self.m_publicReferencePosition = true;
        }

        private static void SetPublicReferencePositionHook(On.ZNet.orig_SetPublicReferencePosition orig, ZNet self, bool pub)
        {
        }
    }
}