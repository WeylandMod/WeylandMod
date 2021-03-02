using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using WeylandMod.Core;

namespace WeylandMod
{
    [BepInPlugin("io.github.WeylandMod", "WeylandMod", "0.1.6")]
    internal class WeylandModPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            var features = new Dictionary<Type, FeatureEntry>();

            foreach (Type type in typeof(WeylandModPlugin).Assembly.GetTypes())
            {
                if (!type.IsSubclassOf(typeof(Feature)) || features.ContainsKey(type))
                {
                    continue;
                }

                var instance = Activator.CreateInstance(type, Logger, Config) as Feature;
                if (instance == null)
                {
                    Logger.LogError($"Failed to instantiate feature of type {type}");
                    continue;
                }

                var feature = new FeatureEntry
                {
                    Instance = instance,
                    Enabled = instance.IsEnabled(),
                };

                features.Add(type, feature);
            }

            foreach (Type type in features.SelectMany(entry => entry.Value.Instance.GetDependencies()))
            {
                if (features.TryGetValue(type, out var instance))
                {
                    instance.Enabled = true;
                }
                else
                {
                    Logger.LogError($"Failed to find dependency type {type}");
                    return;
                }
            }

            foreach (var entry in features.Where(entry => entry.Value.Enabled))
            {
                Logger.LogInfo($"Initializing feature {entry.Key.Name}");

                foreach (FeaturePart part in entry.Value.Instance.GetParts())
                {
                    part.Init();
                }
            }
        }

        private class FeatureEntry
        {
            public Feature Instance;
            public bool Enabled;
        }
    }
}