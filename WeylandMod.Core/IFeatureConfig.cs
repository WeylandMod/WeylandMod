using System.IO;

namespace WeylandMod.Core
{
    public interface IFeatureConfig
    {
        void Reload();

        void Write(BinaryWriter writer);

        void Read(BinaryReader reader);
    }
}