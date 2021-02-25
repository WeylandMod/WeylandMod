using MonoMod;

#pragma warning disable CS0414
#pragma warning disable CS0626
#pragma warning disable CS0649

namespace WeylandMod
{
    [MonoModPatch("global::ZNet")]
    internal class PatchZNet : ZNet
    {
        [MonoModIgnore] private bool m_publicReferencePosition;

        private extern void orig_Awake();

        private void Awake()
        {
            orig_Awake();

            m_publicReferencePosition = true;
        }

        [MonoModReplace]
        public new void SetPublicReferencePosition(bool pub)
        {
        }
    }
}