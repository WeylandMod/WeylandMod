using System;
using BepInEx.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using WeylandMod.Core;

namespace WeylandMod.Features.PermittedPlayersNoPassword
{
    internal class ZNetHooks : FeaturePart
    {
        public ZNetHooks(ManualLogSource logger)
            : base(logger)
        {
        }

        public override void Init()
        {
            Logger.LogDebug($"{nameof(PermittedPlayersNoPassword)}-{nameof(ZNetHooks)} Init");

            On.ZNet.IsAllowed += IsAllowedHook;
            On.ZNet.CheckWhiteList += CheckWhiteListHook;

            IL.ZNet.RPC_ServerHandshake += RPC_ServerHandshakeHook;
            IL.ZNet.RPC_PeerInfo += RPC_PeerInfoHook;
        }

        private bool IsAllowedHook(On.ZNet.orig_IsAllowed orig, ZNet self, string hostName, string playerName)
        {
            Logger.LogDebug($"{nameof(PermittedPlayersNoPassword)}-{nameof(ZNetHooks)} IsAllowed {hostName} {playerName}");

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

        private void RPC_ServerHandshakeHook(ILContext il)
        {
            Logger.LogDebug($"{nameof(PermittedPlayersNoPassword)}-{nameof(ZNetHooks)} RPC_ServerHandshake");

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

        private void RPC_PeerInfoHook(ILContext il)
        {
            Logger.LogDebug($"{nameof(PermittedPlayersNoPassword)}-{nameof(ZNetHooks)} RPC_PeerInfo");

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