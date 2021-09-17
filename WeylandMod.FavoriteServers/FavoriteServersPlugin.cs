using BepInEx;
using WeylandMod.Core;

namespace WeylandMod.FavoriteServers
{
    [BepInPlugin("WeylandMod.FavoriteServers", "WeylandMod.FavoriteServers", "1.6.0")]
    internal class FavoriteServersPlugin : BaseWeylandModPlugin
    {
        protected override IFeature CreateFeature() => new FavoriteServers(Logger, Config);
    }
}