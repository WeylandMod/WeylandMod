using BepInEx.Configuration;
using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.NoServerPassword
{
    internal sealed class NoServerPassword : IFeature
    {
        public string Name => nameof(NoServerPassword);

        public IFeatureConfig Config { get; }

        public IFeatureComponent[] Components { get; }

        public NoServerPassword(ManualLogSource logger, ConfigFile config)
        {
            var featureConfig = new NoServerPasswordConfig(Name, config);

            Config = featureConfig;
            Components = new IFeatureComponent[]
            {
                new FejdStartupComponent(logger),
                new ZSteamMatchmakingComponent(logger, featureConfig),
            };
        }
    }
}