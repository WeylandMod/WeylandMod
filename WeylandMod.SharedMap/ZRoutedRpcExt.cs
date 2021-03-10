using System.Linq;

namespace WeylandMod.SharedMap
{
    internal static class ZRoutedRpcExt
    {
        public static void OthersInvokeRoutedRPC(this ZRoutedRpc self, string methodName, params object[] parameters)
        {
            foreach (var peer in self.m_peers.Where(peer => peer.m_uid != ZRoutedRpc.instance.m_id))
            {
                ZRoutedRpc.instance.InvokeRoutedRPC(peer.m_uid, methodName, parameters);
            }
        }
    }
}