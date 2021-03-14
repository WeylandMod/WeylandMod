using BepInEx;

namespace WeylandMod.Core
{
    [BepInDependency("WeylandMod.Core", "1.1.0")]
    public abstract class BaseWeylandModPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            Logger.LogInfo($"{Info.Metadata.Name} Location={Info.Location}");

            if (!CorePlugin.TryGet(out var core))
            {
                Logger.LogError("Failed to get CorePlugin");
                return;
            }

            var feature = CreateFeature();
            core.RegisterFeature(feature.GetType().Name, feature);
        }

        protected abstract IFeature CreateFeature();
    }
}