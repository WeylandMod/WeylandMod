using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.NoServerPassword
{
    internal class FejdStartupHooks : FeaturePart
    {
        public FejdStartupHooks(ManualLogSource logger)
            : base(logger)
        {
        }

        public override void Init()
        {
            Logger.LogDebug($"{nameof(NoServerPassword)}-{nameof(FejdStartupHooks)} Init");

            On.FejdStartup.IsPublicPasswordValid += IsPublicPasswordValidHook;
        }

        private bool IsPublicPasswordValidHook(
            On.FejdStartup.orig_IsPublicPasswordValid orig, FejdStartup self,
            string password, World world)
        {
            Logger.LogDebug($"{nameof(NoServerPassword)}-{nameof(FejdStartupHooks)} IsPublicPasswordValid");

            return true;
        }
    }
}