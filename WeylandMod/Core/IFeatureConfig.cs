using BepInEx.Configuration;

namespace WeylandMod.Core
{
    internal interface IFeatureConfig
    {
        bool Enabled { get; }

        void Reload();
        void Read(ZPackage pkg);
        void Write(ZPackage pkg);
    }
}