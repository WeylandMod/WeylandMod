namespace WeylandMod.Core
{
    internal interface IFeature
    {
        string Name { get; }
        IFeatureConfig Config { get; }
        IFeatureComponent[] Components { get; }
    }
}