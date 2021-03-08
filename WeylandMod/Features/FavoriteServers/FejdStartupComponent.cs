using BepInEx.Logging;
using WeylandMod.Core;

namespace WeylandMod.Features.FavoriteServers
{
    internal class FejdStartupComponent : IFeatureComponent
    {
        private readonly ManualLogSource _logger;
        private readonly SteamService _steamService;

        private FavoriteServersGUI _favoriteServersGUI;

        public FejdStartupComponent(ManualLogSource logger)
        {
            _logger = logger;
            _steamService = new SteamService(logger);
        }

        public void OnLaunch(bool enabled)
        {
            if (!enabled)
                return;

            On.FejdStartup.Start += StartHook;
            On.FejdStartup.OnSelectedServer += OnSelectedServerHook;
        }

        public void OnConnect()
        {
        }

        public void OnDisconnect()
        {
        }

        private void StartHook(On.FejdStartup.orig_Start orig, FejdStartup self)
        {
            orig(self);

            _favoriteServersGUI = self.m_serverListPanel.AddComponent<FavoriteServersGUI>();
            _favoriteServersGUI.SetSteamService(_steamService);
            _logger.LogDebug("Favorite Servers GUI Mod initialized.");
        }

        private void OnSelectedServerHook(On.FejdStartup.orig_OnSelectedServer orig, FejdStartup self)
        {
            orig(self);
            _favoriteServersGUI.SetSelectedServer(self.m_joinServer);
        }
    }
}