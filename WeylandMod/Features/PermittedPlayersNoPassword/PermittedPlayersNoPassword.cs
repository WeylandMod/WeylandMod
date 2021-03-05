using BepInEx.Configuration;
using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.PermittedPlayersNoPassword
{
    internal sealed class PermittedPlayersNoPassword : IFeature
    {
        public string Name => nameof(PermittedPlayersNoPassword);

        public IFeatureConfig Config { get; }

        public IFeatureComponent[] Components { get; }

        public PermittedPlayersNoPassword(ManualLogSource logger, ConfigFile config)
        {
            Config = new PermittedPlayersNoPasswordConfig(Name, config);
            Components = new IFeatureComponent[]
            {
                new ZNetComponent(logger),
            };
        }
    }
}