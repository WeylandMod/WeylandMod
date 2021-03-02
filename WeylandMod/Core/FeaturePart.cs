using BepInEx.Logging;

namespace WeylandMod.Core
{
    internal abstract class FeaturePart
    {
        protected readonly ManualLogSource Logger;

        protected FeaturePart(ManualLogSource logger)
        {
            Logger = logger;
        }

        public abstract void Init();
    }
}