using BepInEx;
using WeylandMod.Core;

namespace WeylandMod.ItemRadar
{
    [BepInPlugin("WeylandMod.ItemRadar", "WeylandMod.ItemRadar", "1.0.0")]
    internal class ItemRadarPlugin : BaseWeylandModPlugin
    {
        protected override IFeature CreateFeature() => new ItemRadar(Logger, Config);
    }
}