namespace WeylandMod.Core
{
    internal interface IFeatureComponent
    {
        void OnLaunch(bool enabled);
        void OnConnect();
        void OnDisconnect();
    }
}