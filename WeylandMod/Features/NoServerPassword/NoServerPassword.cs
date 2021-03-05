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
            Config = new NoServerPasswordConfig(Name, config);
            Components = new IFeatureComponent[]
            {
                new FejdStartupComponent(logger),
            };
        }
    }
}