using BepInEx;
using WeylandMod.Core;
using WeylandMod.Features.ExtendedStorage;
using WeylandMod.Features.ManageableDeathPins;
using WeylandMod.Features.NoServerPassword;
using WeylandMod.Features.PermittedPlayersNoPassword;
using WeylandMod.Features.SharedMap;

namespace WeylandMod
{
    [BepInPlugin("io.github.WeylandMod", "WeylandMod", "0.1.6")]
    internal class WeylandModPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            var features = new IFeature[]
            {
                new NoServerPassword(Logger, Config),
                new PermittedPlayersNoPassword(Logger, Config),
                new SharedMap(Logger, Config),
                new ExtendedStorage(Logger, Config),
                new ManageableDeathPins(Logger, Config),
            };

            foreach (var feature in features)
            {
                Logger.LogInfo($"{feature.GetType().Name}.Initialize Enabled={feature.Enabled.Value}");
                if (!feature.Enabled.Value)
                {
                    continue;
                }

                foreach (var component in feature.Components)
                {
                    Logger.LogDebug($"{feature.GetType().Name}.{component.GetType().Name}.Initialize");
                    component.Initialize();
                }
            }
        }
    }
}