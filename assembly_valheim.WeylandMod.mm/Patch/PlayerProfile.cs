using MonoMod;

#pragma warning disable CS0626
#pragma warning disable CS0649

namespace WeylandMod
{
    [MonoModPatch("global::PlayerProfile")]
    internal class PatchPlayerProfile : PlayerProfile
    {
        public extern bool orig_HaveDeathPoint();

        public new bool HaveDeathPoint()
        {
            if (ModConfig.Instance.Player.ManageableDeathMarkers.Value)
            {
                return false;
            }

            return orig_HaveDeathPoint();
        }
    }
}