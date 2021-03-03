using BepInEx.Configuration;

namespace WeylandMod.Core
{
    internal interface IFeature
    {
        ConfigEntry<bool> Enabled { get; }

        IFeatureComponent[] Components { get; }
    }
}