using MonoMod;

#pragma warning disable CS0414
#pragma warning disable CS0626
#pragma warning disable CS0649

namespace WeylandMod
{
    [MonoModPatch("global::ZSteamMatchmaking")]
    internal class PatchZSteamMatchmaking
    {
        public extern void orig_RegisterServer(
            string name,
            bool password,
            string version,
            bool publicServer,
            string worldName
        );

        public void RegisterServer(
            string name,
            bool password,
            string version,
            bool publicServer,
            string worldName
        )
        {
            bool pass = !ModConfig.Instance.Server.RemoveSteamPassword.Value && password;
            orig_RegisterServer(name, pass, version, publicServer, worldName);
        }
    }
}