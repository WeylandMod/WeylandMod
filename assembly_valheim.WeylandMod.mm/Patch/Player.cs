using MonoMod;

#pragma warning disable CS0626
#pragma warning disable CS0649

namespace WeylandMod
{
    [MonoModPatch("global::Player")]
    internal class PatchPlayer : Player
    {
        protected extern void orig_OnDeath();

        protected override void OnDeath()
        {
            orig_OnDeath();

            if (ModConfig.Instance.Player.ManageableDeathMarkers.Value)
            {
                Minimap.instance.AddPin(transform.position, Minimap.PinType.Death, "", true, false);
            }
        }
    }
}