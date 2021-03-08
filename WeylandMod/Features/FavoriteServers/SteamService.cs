using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using BepInEx.Logging;
using Steamworks;

namespace WeylandMod.Features.FavoriteServers
{
    internal class SteamService
    {
        private static readonly MatchMakingKeyValuePair_t[] EmptyMatchmakingFilter = new MatchMakingKeyValuePair_t[0];

        private readonly ManualLogSource _logger;

        private readonly List<Action<List<ServerData>>> _favServersUpdatedCallbacks = new List<Action<List<ServerData>>>();
        private readonly ISteamMatchmakingServerListResponse _favServersListResponseCallback;
        private readonly List<ServerData> _favServers = new List<ServerData>();

        private bool _favServersListUpdated = true;

        public SteamService(ManualLogSource logger)
        {
            _logger = logger;

            _favServersListResponseCallback = new ISteamMatchmakingServerListResponse(
                OnFavServerResponded,
                OnFavServerFailedToRespond,
                OnFavRefreshComplete);
        }

        public void AddOnFavServerUpdatedCallback(Action<List<ServerData>> callback)
        {
            _favServersUpdatedCallbacks.Add(callback);
        }

        public bool RemoveOnFavServerUpdatedCallback(Action<List<ServerData>> callback)
        {
            return _favServersUpdatedCallbacks.Remove(callback);
        }

        public void AddServerToFavorite(ServerData serverData)
        {
            SteamMatchmaking.AddFavoriteGame(
                SteamUtils.GetAppID(),
                serverData.m_steamHostAddr.GetIPv4(),
                serverData.m_steamHostAddr.m_port,
                Convert.ToUInt16(serverData.m_steamHostAddr.m_port + 1u),
                Constants.k_unFavoriteFlagFavorite,
                0U);
        }

        public bool AddServerToFavorite(string connectionString)
        {
            string[] hostTokens = connectionString.Split(':');
            if (hostTokens.Length == 0)
                return false;

            IPAddress ipAddress = getIpFromString(hostTokens[0]);
            if (ipAddress == null)
                return false;

            uint ipBits = BitConverter.ToUInt32(ipAddress.GetAddressBytes(), 0);

            var serverData = new ServerData();
            serverData.m_steamHostAddr.SetIPv4(
                ipBits,
                hostTokens.Length > 1
                    ? Convert.ToUInt16(hostTokens[1])
                    : (ushort) 2456
                );

            AddServerToFavorite(serverData);
            return true;
        }

        private IPAddress getIpFromString(String connectionString)
        {
            try
            {
                IPAddress address1;
                if (IPAddress.TryParse(connectionString, out address1))
                    return address1;

                IPHostEntry hostEntry = Dns.GetHostEntry(connectionString);
                if (hostEntry.AddressList.Length == 0)
                    return null;

                foreach (IPAddress address2 in hostEntry.AddressList)
                {
                    if (address2.AddressFamily == AddressFamily.InterNetwork)
                        return address2;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void CallFavServerUpdatedCallbacks()
        {
            _favServersUpdatedCallbacks.ForEach(c =>
            {
                try
                {
                    c.Invoke(_favServers);
                }
                catch (Exception e)
                {
                    _logger.LogError($"Cannot call callback {e.Message}");
                }
            });
        }

        public bool IsFavServerListUpdated()
        {
            return _favServersListUpdated;
        }

        public void RequestUpdateFavServers()
        {
            if (!_favServersListUpdated)
                return;

            _favServersListUpdated = false;
            _favServers.Clear();

            SteamMatchmakingServers.RequestFavoritesServerList(
                SteamUtils.GetAppID(),
                EmptyMatchmakingFilter,
                0U,
                _favServersListResponseCallback);
        }

        private void OnFavServerResponded(HServerListRequest request, int iServer)
        {
            gameserveritem_t serverDetails = SteamMatchmakingServers.GetServerDetails(request, iServer);

            string serverName = serverDetails.GetServerName();

            ServerData serverData = new ServerData {m_name = serverName};

            serverData.m_steamHostAddr.SetIPv4(
                serverDetails.m_NetAdr.GetIP(), serverDetails.m_NetAdr.GetConnectionPort());
            serverData.m_password = serverDetails.m_bPassword;
            serverData.m_players = serverDetails.m_nPlayers;
            serverData.m_version = serverDetails.GetGameTags();

            _favServers.Add(serverData);

            CallFavServerUpdatedCallbacks();
        }

        private void OnFavServerFailedToRespond(HServerListRequest request, int iServer)
        {
            _favServersListUpdated = true;
        }

        private void OnFavRefreshComplete(HServerListRequest request, EMatchMakingServerResponse response)
        {
            _favServersListUpdated = true;
        }
    }
}