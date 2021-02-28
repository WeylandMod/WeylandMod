namespace WeylandMod.Utils
{
    internal static class WeylandRpc
    {
        public static string GetName(string name) => $"{nameof(WeylandRpc)}_{name}";
    }
}