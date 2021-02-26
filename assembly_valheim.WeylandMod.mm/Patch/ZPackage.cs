using System.IO;
using MonoMod;

#pragma warning disable CS0626
#pragma warning disable CS0649

namespace WeylandMod
{
    internal class PatchZPackage : ZPackage
    {
        [MonoModIgnore] private MemoryStream m_stream;
        [MonoModIgnore] private BinaryWriter m_writer;
        [MonoModIgnore] private BinaryReader m_reader;

        public byte[] WeylandMod_ReadToEnd() =>
            m_reader.ReadBytes(Size() - (int) m_stream.Position);

        public void WeylandMod_Write(byte[] buffer, int index, int count) =>
            m_writer.Write(buffer, index, count);
    }

    internal static class ZPackageExt
    {
        public static byte[] ReadToEnd(this ZPackage z) =>
            ((PatchZPackage) z).WeylandMod_ReadToEnd();

        public static void Write(this ZPackage z, byte[] buffer, int index, int count) =>
            ((PatchZPackage) z).WeylandMod_Write(buffer, index, count);
    }
}