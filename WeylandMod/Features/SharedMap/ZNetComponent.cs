using BepInEx.Logging;
using MonoMod.Cil;
using WeylandMod.Core;

namespace WeylandMod.Features.SharedMap
{
    internal class ZNetComponent : IFeatureComponent
    {
        private readonly ManualLogSource _logger;

        public ZNetComponent(ManualLogSource logger)
        {
            _logger = logger;
        }

        public void OnLaunch(bool enabled)
        {
        }

        public void OnConnect()
        {
            ZNet.instance.SetPublicReferencePosition(true);

            IL.ZNet.SetPublicReferencePosition += SetPublicReferencePositionPatch;
        }

        public void OnDisconnect()
        {
            IL.ZNet.SetPublicReferencePosition -= SetPublicReferencePositionPatch;
        }

        private static void SetPublicReferencePositionPatch(ILContext il)
        {
            // remove setting m_publicReferencePosition
            new ILCursor(il)
                .GotoNext(
                    x => x.MatchLdarg(0),
                    x => x.MatchLdarg(1),
                    x => x.MatchStfld<ZNet>("m_publicReferencePosition")
                )
                .RemoveRange(3);
        }
    }
}