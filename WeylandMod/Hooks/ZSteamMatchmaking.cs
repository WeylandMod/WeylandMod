using BepInEx.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using WeylandMod.Utils;

namespace WeylandMod.Hooks
{
    internal static class ZSteamMatchmakingHooks
    {
        private static ManualLogSource Logger;

        public static void Init(ManualLogSource logger)
        {
            Logger = logger;

            On.ZSteamMatchmaking.RegisterServer += RegisterServerHook;
            if (WeylandConfig.Player.ResolveHostNameOnJoin.Value)
            {
                On.ZSteamMatchmaking.QueueServerJoin += QueueServerJoinHook;
            }
        }

        private static void RegisterServerHook(
            On.ZSteamMatchmaking.orig_RegisterServer orig, ZSteamMatchmaking self,
            string name, bool password, string version, bool publicServer, string worldName)
        {
            var pass = !WeylandConfig.Server.RemoveSteamPassword.Value && password;
            orig(self, name, pass, version, publicServer, worldName);
        }

        private static void QueueServerJoinHook(On.ZSteamMatchmaking.orig_QueueServerJoin orig, ZSteamMatchmaking self, string addr)
        {
            Logger.LogDebug($"Trying to join server: {addr}");

            string[] addrTokens = addr.Split(':');
            string host = addrTokens.Length > 0 ? addrTokens[0] : addr;
            if (host.Length == 0)
            {
                orig(self, addr);
                return;
            }

            var ipv4Address = string.Empty;

            try
            {
                foreach (var address in Dns.GetHostAddresses(host))
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipv4Address = address.ToString();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"{nameof(ZSteamMatchmaking)}: QueueServerJoinHook hostname resolving error: addr={addr}, error={e.Message}");
            }

            ipv4Address = ipv4Address != "" && addrTokens.Length > 1
                ? ipv4Address + ":" + addrTokens[1]
                : addr;

            orig(self, ipv4Address);
        }
    }
}