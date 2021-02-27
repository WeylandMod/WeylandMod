namespace WeylandMod.Hooks
{
    internal static class ZPackageExt
    {
        public static byte[] ReadToEnd(this ZPackage z) =>
            z.m_reader.ReadBytes(z.Size() - (int) z.m_stream.Position);

        public static void Write(this ZPackage z, byte[] buffer, int offset, int count) =>
            z.m_writer.Write(buffer, offset, count);
    }
}