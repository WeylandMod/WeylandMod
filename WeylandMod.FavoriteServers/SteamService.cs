using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using BepInEx.Logging;
using Steamworks;

namespace WeylandMod.FavoriteServers
{
    internal class SteamService
    {
        private static readonly MatchMakingKeyValuePair_t[] EmptyMatchmakingFilter = { };

        public delegate void FavoriteServersUpdatedHandler(List<ServerData> servers);

        public event FavoriteServersUpdatedHandler FavoriteServersUpdated;

        public bool IsFavoriteServersRefreshAvailable { get; private set; }

        private readonly ManualLogSource _logger;

        private readonly ISteamMatchmakingServerListResponse _favoriteServerListResponse;

        private readonly List<ServerData> _favoriteServers = new List<ServerData>();

        public SteamService(ManualLogSource logger)
        {
            _logger = logger;

            IsFavoriteServersRefreshAvailable = true;

            _favoriteServerListResponse = new ISteamMatchmakingServerListResponse(
                OnFavoriteServersResponded,
                (request, serverIndex) =>
                {
                    IsFavoriteServersRefreshAvailable = true;
                    FavoriteServersUpdated?.Invoke(_favoriteServers);
                },
                (request, response) =>
                {
                    IsFavoriteServersRefreshAvailable = true;
                    FavoriteServersUpdated?.Invoke(_favoriteServers);
                });
        }

        public static ServerData GetServerData(string serverString)
        {
            string[] hostTokens = serverString.Split(':');
            if (hostTokens.Length == 0)
                return null;

            IPAddress ipAddress = GetIpFromString(hostTokens[0]);
            if (ipAddress == null)
                return null;

            var address = BitConverter.ToUInt32(ipAddress.GetAddressBytes().Reverse().ToArray(), 0);
            var port = hostTokens.Length > 1 ? Convert.ToUInt16(hostTokens[1]) : (ushort) 2456;

            var serverData = new ServerData();
            serverData.m_steamHostAddr.SetIPv4(address, port);

            return serverData;
        }

        public static void AddFavoriteServer(ServerData serverData)
        {
            SteamMatchmaking.AddFavoriteGame(
                SteamUtils.GetAppID(),
                serverData.m_steamHostAddr.GetIPv4(),
                serverData.m_steamHostAddr.m_port,
                Convert.ToUInt16(serverData.m_steamHostAddr.m_port + 1u),
                Constants.k_unFavoriteFlagFavorite,
                0U
            );
        }

        public static void RemoveFavoriteServer(ServerData serverData)
        {
            SteamMatchmaking.RemoveFavoriteGame(
                SteamUtils.GetAppID(),
                serverData.m_steamHostAddr.GetIPv4(),
                serverData.m_steamHostAddr.m_port,
                Convert.ToUInt16(serverData.m_steamHostAddr.m_port + 1u),
                Constants.k_unFavoriteFlagFavorite
            );
        }

        private static IPAddress GetIpFromString(string connectionString)
        {
            try
            {
                if (IPAddress.TryParse(connectionString, out IPAddress ipAddress))
                    return ipAddress;

                IPHostEntry hostEntry = Dns.GetHostEntry(connectionString);
                return hostEntry.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void RequestFavoriteServers()
        {
            if (!IsFavoriteServersRefreshAvailable)
                return;

            IsFavoriteServersRefreshAvailable = false;
            _favoriteServers.Clear();

            SteamMatchmakingServers.RequestFavoritesServerList(
                SteamUtils.GetAppID(),
                EmptyMatchmakingFilter,
                0U,
                _favoriteServerListResponse
            );
        }

        private void OnFavoriteServersResponded(HServerListRequest request, int serverIndex)
        {
            gameserveritem_t serverDetails = SteamMatchmakingServers.GetServerDetails(request, serverIndex);

            var serverData = new ServerData
            {
                m_name = serverDetails.GetServerName(),
                m_password = serverDetails.m_bPassword,
                m_players = serverDetails.m_nPlayers,
                m_version = serverDetails.GetGameTags(),
            };

            serverData.m_steamHostAddr.SetIPv4(
                serverDetails.m_NetAdr.GetIP(),
                serverDetails.m_NetAdr.GetConnectionPort()
            );

            _logger.LogDebug(
                "OnFavoriteServersResponded " +
                $"Name='{serverData.m_name}' " +
                $"Host={serverDetails.m_NetAdr.GetConnectionAddressString()} " +
                $"Version={serverData.m_version} " +
                $"Players={serverData.m_players} " +
                $"Password={serverData.m_password}"
            );

            _favoriteServers.Add(serverData);

            FavoriteServersUpdated?.Invoke(_favoriteServers);
        }
    }
}