using BepInEx.Logging;
using WeylandMod.Utils;

namespace WeylandMod.Hooks
{
    internal static class ZNetHooks
    {
        private static ManualLogSource Logger;

        public static void Init(ManualLogSource logger)
        {
            Logger = logger;

            On.ZNet.Awake += AwakeHook;
            On.ZNet.SetPublicReferencePosition += SetPublicReferencePositionHook;

            if (WeylandConfig.Server.SkipPlayerPasswordOnPermit.Value)
            {
                On.ZNet.IsAllowed += IsAllowedHook;
                On.ZNet.RPC_ServerHandshake += RPC_ServerHandshakeHook;
                On.ZNet.RPC_PeerInfo += RPC_PeerInfoHook;
            }
        }

        private static void AwakeHook(On.ZNet.orig_Awake orig, ZNet self)
        {
            orig(self);

            self.m_publicReferencePosition = true;
        }

        private static void SetPublicReferencePositionHook(On.ZNet.orig_SetPublicReferencePosition orig,
            ZNet self, bool pub)
        {
            // denied
        }

        private static bool IsAllowedHook(On.ZNet.orig_IsAllowed orig, ZNet self, string hostName, string playerName)
        {
            var clientBanned = self.m_bannedList.Contains(hostName) || self.m_bannedList.Contains(playerName);
            var clientPermitted = self.m_permittedList.Count() <= 0 || self.m_permittedList.Contains(hostName);

            return !clientBanned && (clientPermitted || ZNet.m_serverPassword != "");
        }

        private static void RPC_ServerHandshakeHook(On.ZNet.orig_RPC_ServerHandshake orig, ZNet self, ZRpc rpc)
        {
            var peer = self.GetPeer(rpc);
            if (peer == null)
            {
                return;
            }

            ZLog.Log($"Got handshake from client {peer.m_socket.GetEndPointString()}");

            self.ClearPlayerData(peer);

            var emptyPassword = string.IsNullOrEmpty(ZNet.m_serverPassword);
            var needPassword = !(emptyPassword || self.m_permittedList.Contains(peer.m_socket.GetHostName()));

            peer.m_rpc.Invoke("ClientHandshake", needPassword);
        }

        private static void RPC_PeerInfoHook(On.ZNet.orig_RPC_PeerInfo orig, ZNet self, ZRpc rpc, ZPackage pkg)
        {
            if (!self.IsServer())
            {
                orig(self, rpc, pkg);
                return;
            }

            var peer = self.GetPeer(rpc);
            if (peer == null)
            {
                return;
            }

            // Repackage data with changed password for simplicity
            var buffer = new ZPackage();
            buffer.Write(pkg.ReadLong());
            buffer.Write(pkg.ReadString());
            buffer.Write(pkg.ReadVector3());
            buffer.Write(pkg.ReadString());

            var password = pkg.ReadString();
            var clientPermitted = self.m_permittedList.Contains(peer.m_socket.GetHostName());
            buffer.Write(clientPermitted ? ZNet.m_serverPassword : password);

            var tail = pkg.ReadToEnd();
            buffer.Write(tail, 0, tail.Length);
            buffer.SetPos(0);

            orig(self, rpc, buffer);
        }
    }
}