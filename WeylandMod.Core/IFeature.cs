using System.IO;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace WeylandMod.Core
{
    public interface IFeature
    {
        IFeatureConfig Config { get; }

        void OnRegister();

        void OnConnect();

        void OnDisconnect();
    }
}