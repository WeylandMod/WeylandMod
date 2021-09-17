using BepInEx;
using WeylandMod.Core;

namespace WeylandMod.SharedMap
{
    [BepInPlugin("WeylandMod.SharedMap", "WeylandMod.SharedMap", "1.6.0")]
    internal class SharedMapPlugin : BaseWeylandModPlugin
    {
        protected override IFeature CreateFeature() => new SharedMap(Logger, Config);
    }
}