using MonoMod;

#pragma warning disable CS0626
#pragma warning disable CS0649

namespace WeylandMod
{
    [MonoModPatch("global::FejdStartup")]
    internal class PatchFejdStartup : FejdStartup
    {
        private extern void orig_Awake();
        private extern bool orig_IsPublicPasswordValid(string password, World world);

        private void Awake()
        {
            ModConfig.Create();
            ModLogger.LogInfo("WeylandMod initialized.");

            orig_Awake();
        }

        private bool IsPublicPasswordValid(string password, World world)
        {
            if (ModConfig.Instance.Server.SkipPasswordValidation.Value)
            {
                return true;
            }

            return orig_IsPublicPasswordValid(password, world);
        }
    }
}