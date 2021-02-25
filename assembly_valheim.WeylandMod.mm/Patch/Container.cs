using MonoMod;

#pragma warning disable CS0626
#pragma warning disable CS0649

namespace WeylandMod
{
    [MonoModPatch("global::Container")]
    internal class PatchContainer : Container
    {
        private extern void orig_Awake();

        private void Awake()
        {
            if (ModConfig.Instance.ExtendedStorage.Enabled.Value)
            {
                m_width += 1;
                m_height += 1;
            }

            orig_Awake();
        }
    }
}