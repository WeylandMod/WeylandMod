using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeylandMod.Features.FavoriteServers
{
    internal sealed class FavoriteServersGUI : MonoBehaviour
    {
        private SteamService _steamService;

        private Rect _windowRect = new Rect (860, 400, 370, 350);
        private List<ServerData> _favServers = new List<ServerData>(0);
        private ServerData _selectedServer;
        private bool _addServerModalShown;
        private string _ipToAdd;

        private Action<List<ServerData>> _onFavServerUpdatedCallback;

        private void Awake()
        {
            _onFavServerUpdatedCallback = favServers => _favServers = favServers;
        }

        private void OnEnable()
        {
            _steamService.AddOnFavServerUpdatedCallback(_onFavServerUpdatedCallback);
            _steamService.RequestUpdateFavServers();
        }

        private void OnDisable()
        {
            _steamService.RemoveOnFavServerUpdatedCallback(_onFavServerUpdatedCallback);
            _selectedServer = null;
            _addServerModalShown = false;
            _ipToAdd = "";
        }

        private void OnGUI()
        {
            GUI.enabled = _selectedServer != null;
            if (GUI.Button(new Rect(650, 750, 150, 50), "Add Server To Favorite"))
            {
                _steamService.AddServerToFavorite(_selectedServer);
                _steamService.RequestUpdateFavServers();
            }
            GUI.enabled = !_addServerModalShown;

            GUI.Box(_windowRect,"Favorite Servers");

            GUILayout.BeginArea(new Rect(_windowRect.x, _windowRect.y + 25, _windowRect.width, _windowRect.height));
            {
                GUILayout.BeginVertical();
                {
                    _favServers.ForEach(server =>
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Button(server.m_name);
                        GUILayout.EndHorizontal();
                    });
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(_windowRect.x, _windowRect.y + _windowRect.height, _windowRect.width, 100));
            {
                GUILayout.BeginHorizontal();
                {
                    GUI.enabled = _steamService.IsFavServerListUpdated() && !_addServerModalShown;
                    if (GUILayout.Button("Refresh"))
                    {
                        _steamService.RequestUpdateFavServers();
                    }
                    GUI.enabled = !_addServerModalShown;

                    if (GUILayout.Button("Add Server"))
                    {
                        _addServerModalShown = true;
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();

            if (_addServerModalShown)
            {
                GUI.enabled = true;
                GUI.Box(
                    new Rect(_windowRect.x, _windowRect.y + _windowRect.height + 25, _windowRect.width, 100),
                    "Add server to favorite");
                GUILayout.BeginArea(new Rect(_windowRect.x, _windowRect.y + _windowRect.height + 50, _windowRect.width, 100));
                {
                    GUILayout.BeginVertical();
                    {
                        GUILayout.Label("Connection string (<IP\\Domain>:<port>)");
                        _ipToAdd = GUILayout.TextField(_ipToAdd);
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Add"))
                        {
                            if (_steamService.AddServerToFavorite(_ipToAdd))
                            {
                                HideAddToFav();
                                _steamService.RequestUpdateFavServers();
                            }
                        }
                        if (GUILayout.Button("Cancel"))
                        {
                            HideAddToFav();
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndArea();
            }
        }

        private void HideAddToFav()
        {
            _addServerModalShown = false;
            _ipToAdd = "";
        }

        public void SetSteamService(SteamService steamService)
        {
            _steamService = steamService;
        }

        public void SetSelectedServer(ServerData selectedServer)
        {
            _selectedServer = selectedServer;
        }
    }
}