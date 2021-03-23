using BepInEx;
using WeylandMod.Core;

namespace WeylandMod.PreciseRotation
{
    [BepInPlugin("WeylandMod.PreciseRotation", "WeylandMod.PreciseRotation", "1.1.0")]
    internal class PreciseRotationPlugin : BaseWeylandModPlugin
    {
        protected override IFeature CreateFeature() => new PreciseRotation(Logger, Config);
    }
}