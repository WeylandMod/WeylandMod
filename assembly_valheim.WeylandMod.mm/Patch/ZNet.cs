using MonoMod;

#pragma warning disable CS0414
#pragma warning disable CS0626
#pragma warning disable CS0649

namespace WeylandMod
{
    [MonoModPatch("global::ZNet")]
    internal class PatchZNet : ZNet
    {
        [MonoModIgnore] private static string m_serverPassword;

        [MonoModIgnore] private bool m_publicReferencePosition;
        [MonoModIgnore] private SyncedList m_permittedList;
        [MonoModIgnore] private SyncedList m_bannedList;

        private extern void orig_Awake();
        public extern bool orig_IsAllowed(string hostName, string playerName);
        private extern void orig_RPC_ServerHandshake(ZRpc rpc);
        private extern void orig_RPC_PeerInfo(ZRpc rpc, ZPackage pkg);

        [MonoModIgnore]
        private extern void ClearPlayerData(ZNetPeer peer);

        [MonoModIgnore]
        private extern ZNetPeer GetPeer(ZRpc rpc);

        private void Awake()
        {
            orig_Awake();

            m_publicReferencePosition = true;
        }

        [MonoModReplace]
        public new void SetPublicReferencePosition(bool pub)
        {
        }

        [MonoModReplace]
        public new bool IsAllowed(string hostName, string playerName)
        {
            if (!ModConfig.Instance.Server.SkipPlayerPasswordOnPermit.Value)
            {
                return orig_IsAllowed(hostName, playerName);
            }

            bool clientBanned = this.m_bannedList.Contains(hostName) || this.m_bannedList.Contains(playerName);
            bool clientAllowedByPermittedList = this.m_permittedList.Count() <= 0 || this.m_permittedList.Contains(hostName);

            return !clientBanned && (clientAllowedByPermittedList || PatchZNet.m_serverPassword != "");
        }

        private void RPC_ServerHandshake(ZRpc rpc)
        {
            if (!ModConfig.Instance.Server.SkipPlayerPasswordOnPermit.Value)
            {
                orig_RPC_ServerHandshake(rpc);
                return;
            }

            ZNetPeer peer = this.GetPeer(rpc);
            if (peer == null)
                return;

            ZLog.Log((object)("Got handshake from client " + peer.m_socket.GetEndPointString()));

            this.ClearPlayerData(peer);

            bool needPassword = !(string.IsNullOrEmpty(PatchZNet.m_serverPassword) || this.m_permittedList.Contains(peer.m_socket.GetHostName()));

            peer.m_rpc.Invoke("ClientHandshake", (object)needPassword);
        }

        private void RPC_PeerInfo(ZRpc rpc, ZPackage pkg)
        {
            if (!ModConfig.Instance.Server.SkipPlayerPasswordOnPermit.Value || !this.IsServer())
            {
                orig_RPC_PeerInfo(rpc, pkg);
                return;
            }

            ZNetPeer peer = this.GetPeer(rpc);
            if (peer == null)
                return;

            // Repackage data with changed password for simplicity
            ZPackage zpackage = new ZPackage();
            zpackage.Write(pkg.ReadLong());
            zpackage.Write(pkg.ReadString());
            zpackage.Write(pkg.ReadVector3());
            zpackage.Write(pkg.ReadString());

            string password = pkg.ReadString();
            if (this.m_permittedList.Contains(peer.m_socket.GetHostName()))
            {
                password = PatchZNet.m_serverPassword;
            }
            zpackage.Write(password);

            byte[] end = ZPackageExt.ReadToEnd(pkg);
            ZPackageExt.Write(zpackage, end, 0, end.Length);

            orig_RPC_PeerInfo(rpc, new ZPackage(zpackage.GetArray()));
        }
    }
}