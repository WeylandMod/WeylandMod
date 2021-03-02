using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace WeylandMod.Core
{
    internal abstract class Feature
    {
        protected readonly ManualLogSource Logger;
        protected readonly ConfigFile Config;

        protected Feature(ManualLogSource logger, ConfigFile config)
        {
            Logger = logger;
            Config = config;
        }

        public virtual IEnumerable<Type> GetDependencies()
        {
            yield break;
        }

        public abstract bool IsEnabled();

        public abstract IEnumerable<FeaturePart> GetParts();
    }
}