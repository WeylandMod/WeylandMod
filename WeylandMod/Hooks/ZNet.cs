using System;
using BepInEx.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using WeylandMod.Utils;

namespace WeylandMod.Hooks
{
    internal static class ZNetHooks
    {
        private static ManualLogSource Logger;

        public static void Init(ManualLogSource logger)
        {
            Logger = logger;

            var sharedMap = WeylandConfig.SharedMap;
            if (sharedMap.SharedExplorationEnabled.Value || sharedMap.ForcePublicPosition.Value)
            {
                On.ZNet.Awake += AwakeHook;
                On.ZNet.SetPublicReferencePosition += SetPublicReferencePositionHook;
            }

            if (WeylandConfig.Server.SkipPlayerPasswordOnPermit.Value)
            {
                On.ZNet.IsAllowed += IsAllowedHook;
                On.ZNet.CheckWhiteList += CheckWhiteListHook;
                IL.ZNet.RPC_ServerHandshake += RPC_ServerHandshakeHook;
                IL.ZNet.RPC_PeerInfo += RPC_PeerInfoHook;
            }
        }

        private static void AwakeHook(On.ZNet.orig_Awake orig, ZNet self)
        {
            orig(self);

            self.m_publicReferencePosition = true;
        }

        private static void SetPublicReferencePositionHook(
            On.ZNet.orig_SetPublicReferencePosition orig,
            ZNet self, bool pub)
        {
            // denied
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

        private static void RPC_ServerHandshakeHook(ILContext il)
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
                .Emit(OpCodes.Ldarg_0)
                .Emit(OpCodes.Ldloc_0)
                // check m_serverPassword and m_permittedList for peer hostName
                .EmitDelegate<Func<ZNet, ZNetPeer, bool>>((self, peer) =>
                    !string.IsNullOrEmpty(ZNet.m_serverPassword) &&
                    !self.m_permittedList.Contains(peer.m_socket.GetEndPointString())
                );
        }

        private static void RPC_PeerInfoHook(ILContext il)
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