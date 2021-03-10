using System;
using BepInEx.Configuration;
using BepInEx.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using WeylandMod.Core;

namespace WeylandMod.ExtendedServerPassword
{
    internal class ExtendedServerPassword : IFeature
    {
        public IFeatureConfig Config => _config;

        private readonly ManualLogSource _logger;
        private readonly ExtendedServerPasswordConfig _config;

        public ExtendedServerPassword(ManualLogSource logger, ConfigFile config)
        {
            _logger = logger;
            _config = new ExtendedServerPasswordConfig(config);
        }

        public void OnRegister()
        {
            _logger.LogDebug(
                "OnRegister " +
                $"SkipPublicPasswordCheck={_config.SkipPublicPasswordCheck} " +
                $"SkipPasswordCheckForPermittedPlayers={_config.SkipPasswordCheckForPermittedPlayers}"
            );

            if (_config.SkipPublicPasswordCheck)
            {
                On.FejdStartup.IsPublicPasswordValid += IsPublicPasswordValidHook;
            }

            if (_config.SkipPasswordCheckForPermittedPlayers)
            {
                On.ZNet.IsAllowed += IsAllowedHook;
                On.ZNet.CheckWhiteList += CheckWhiteListHook;

                IL.ZNet.RPC_ServerHandshake += RPC_ServerHandshakePatch;
                IL.ZNet.RPC_PeerInfo += RPC_PeerInfoPatch;
            }
        }

        public void OnConnect()
        {
        }

        public void OnDisconnect()
        {
        }

        private static bool IsPublicPasswordValidHook(
            On.FejdStartup.orig_IsPublicPasswordValid orig,
            FejdStartup self,
            string password,
            World world)
        {
            return true;
        }

        private static bool IsAllowedHook(On.ZNet.orig_IsAllowed orig, ZNet self, string hostName, string playerName)
        {
            var clientBanned = self.m_bannedList.Contains(hostName) || self.m_bannedList.Contains(playerName);
            var clientPermitted = self.m_permittedList.Count() <= 0 || self.m_permittedList.Contains(hostName);

            return !clientBanned && (clientPermitted || !string.IsNullOrEmpty(ZNet.m_serverPassword));
        }

        private static void CheckWhiteListHook(On.ZNet.orig_CheckWhiteList orig, ZNet self)
        {
            if (!string.IsNullOrEmpty(ZNet.m_serverPassword))
                return;

            orig(self);
        }

        private static void RPC_ServerHandshakePatch(ILContext il)
        {
            new ILCursor(il)
                // find server password check
                .GotoNext(
                    x => x.MatchLdsfld<ZNet>("m_serverPassword"),
                    x => x.MatchCall<String>("IsNullOrEmpty"),
                    x => x.MatchLdcI4(0),
                    x => x.MatchCeq()
                )
                // remove original check
                .RemoveRange(4)
                .Emit(OpCodes.Ldarg_0) // push this
                .Emit(OpCodes.Ldloc_0) // push peer
                // check m_serverPassword and m_permittedList for peer hostName
                .EmitDelegate<Func<ZNet, ZNetPeer, bool>>((self, peer) =>
                    !string.IsNullOrEmpty(ZNet.m_serverPassword) &&
                    !self.m_permittedList.Contains(peer.m_socket.GetEndPointString())
                );
        }

        private static void RPC_PeerInfoPatch(ILContext il)
        {
            ILLabel successLabel = null;
            new ILCursor(il)
                // find original player password check, save jump label and move after
                .GotoNext(MoveType.After,
                    x => x.MatchLdsfld<ZNet>("m_serverPassword"),
                    x => x.MatchLdloc(7),
                    x => x.MatchCall<String>("op_Inequality"),
                    x => x.MatchBrfalse(out successLabel)
                )
                // check if hostName in m_permittedList
                .Emit(OpCodes.Ldarg_0) // push this ZNet
                .Emit<ZNet>(OpCodes.Ldfld, "m_permittedList")
                .Emit(OpCodes.Ldloc, 4) // push hostName
                .Emit<SyncedList>(OpCodes.Callvirt, "Contains")
                .Emit(OpCodes.Brtrue_S, successLabel);
        }
    }
}