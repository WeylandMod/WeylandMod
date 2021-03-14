using System;
using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WeylandMod.FavoriteServers
{
    internal class FavoriteServersComponent : MonoBehaviour
    {
        private ManualLogSource _logger;
        private SteamService _steamService;

        private int _selectedServerIndex;
        private List<ServerData> _serverDataList;
        private List<GameObject> _serverObjectList;

        private GameObject _serverListRoot;
        private GameObject _serverListElement;

        private Button _refreshButton;
        private Button _connectButton;
        private Button _addButton;
        private Button _removeButton;
        private Button _addToFavoritesButton;

        private GameObject _addDialog;
        private GameObject _removeDialog;

        public void Create(ManualLogSource logger)
        {
            _logger = logger;
            _steamService = new SteamService(logger);

            _selectedServerIndex = -1;
            _serverDataList = new List<ServerData>();
            _serverObjectList = new List<GameObject>();

            _steamService.FavoriteServersUpdated += OnFavoriteServersUpdated;
        }

        private void Start()
        {
            CreateFavoritesTab();
            CreateAddToFavoritesButton();

            _addDialog = CreateAddDialog();
            _removeDialog = CreateRemoveDialog();

            _refreshButton.gameObject.SetActive(true);
            _refreshButton.onClick = new Button.ButtonClickedEvent();
            _refreshButton.onClick.AddListener(OnRefreshButtonClick);

            _connectButton.gameObject.SetActive(true);
            _connectButton.onClick = new Button.ButtonClickedEvent();
            _connectButton.onClick.AddListener(OnConnectButtonClick);

            _addButton.gameObject.SetActive(true);
            _addButton.onClick = new Button.ButtonClickedEvent();
            _addButton.onClick.AddListener(OnAddButtonClick);

            _removeButton.gameObject.SetActive(true);
            _removeButton.onClick = new Button.ButtonClickedEvent();
            _removeButton.onClick.AddListener(OnRemoveButtonClick);

            _addToFavoritesButton.gameObject.SetActive(true);
            _addToFavoritesButton.onClick = new Button.ButtonClickedEvent();
            _addToFavoritesButton.onClick.AddListener(OnAddToFavoritesButtonClick);
        }

        private void Update()
        {
            _refreshButton.interactable = _steamService.IsFavoriteServersRefreshAvailable;
            _connectButton.interactable = _selectedServerIndex != -1;
            _removeButton.interactable = _selectedServerIndex != -1;
            _addToFavoritesButton.interactable = FejdStartup.instance.m_joinServer != null;
        }

        private void OnFavoriteServersUpdated(List<ServerData> servers)
        {
            _serverDataList = servers;

            foreach (GameObject favoriteServer in _serverObjectList)
            {
                Destroy(favoriteServer);
            }

            _serverObjectList.Clear();

            if (_selectedServerIndex == -1 || _selectedServerIndex >= _serverDataList.Count)
            {
                _selectedServerIndex = _serverDataList.Count == 0 ? -1 : 0;
            }

            for (var index = 0; index < _serverDataList.Count; ++index)
            {
                ServerData server = _serverDataList[index];

                GameObject element = Instantiate(_serverListElement, _serverListRoot.transform);
                element.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, -34.0f * index);
                element.GetComponentInChildren<Text>().text = $"{index}. {server.m_name}";
                element.GetComponentInChildren<UITooltip>().m_text = server.ToString();
                element.GetComponent<Button>().onClick.AddListener(OnServerSelected);
                element.transform.Find("version").GetComponent<Text>().text = server.m_version;
                element.transform.Find("players").GetComponent<Text>().text = $"Players: {server.m_players}";
                element.transform.Find("Private").gameObject.SetActive(server.m_password);
                element.transform.Find("selected").gameObject.SetActive(index == _selectedServerIndex);

                element.SetActive(true);
                _serverObjectList.Add(element);
            }
        }

        public void OnServerSelected()
        {
            _selectedServerIndex = -1;

            var currentSelectedObject = EventSystem.current.currentSelectedGameObject;
            for (var index = 0; index < _serverObjectList.Count; ++index)
            {
                var serverObject = _serverObjectList[index];
                var serverTransform = serverObject.transform.Find("selected");

                if (serverObject.Equals(currentSelectedObject))
                {
                    _selectedServerIndex = index;
                    serverTransform.gameObject.SetActive(true);
                }
                else
                {
                    serverTransform.gameObject.SetActive(false);
                }
            }
        }

        private void OnRefreshButtonClick()
        {
            if (!_steamService.IsFavoriteServersRefreshAvailable)
                return;

            _logger.LogDebug("OnRefreshButtonClick");

            _steamService.RequestFavoriteServers();
        }

        private void OnConnectButtonClick()
        {
            if (_selectedServerIndex == -1)
                return;

            _logger.LogDebug("OnConnectButtonClick");

            ZNet.SetServer(false, false, false, "", "", null);

            var serverData = _serverDataList[_selectedServerIndex];
            if (serverData.m_steamHostID != 0UL)
            {
                ZNet.SetServerHost(serverData.m_steamHostID);
            }
            else
            {
                ZNet.SetServerHost(serverData.m_steamHostAddr);
            }

            FejdStartup.instance.TransitionToMainScene();
        }

        private void OnAddButtonClick()
        {
            _logger.LogDebug("OnAddButtonClick");

            _addDialog.SetActive(true);

            var inputField = _addDialog.transform.Find("panel/AddressField").GetComponent<InputField>();
            inputField.text = "";
            inputField.ActivateInputField();
        }

        private void OnRemoveButtonClick()
        {
            if (_selectedServerIndex == -1)
                return;

            _logger.LogDebug("OnRemoveButtonClick");

            var serverData = _serverDataList[_selectedServerIndex];
            _removeDialog.transform.Find("Dialog/name").GetComponent<Text>().text = serverData.m_name;
            _removeDialog.SetActive(true);
        }

        private void OnAddToFavoritesButtonClick()
        {
            var serverData = FejdStartup.instance.m_joinServer;
            if (serverData == null)
                return;

            serverData.m_steamHostAddr.ToString(out var address, true);
            _logger.LogDebug($"OnAddToFavoritesButtonClick Host={address}");

            SteamService.AddFavoriteServer(serverData);
            OnRefreshButtonClick();
        }

        private void CreateFavoritesTab()
        {
            var startGamePanel = gameObject.transform.Find("StartGame/Panel");
            var tabHandler = startGamePanel.GetComponentInChildren<TabHandler>();

            var favoritesEvent = new UnityEvent();
            favoritesEvent.AddListener(OnRefreshButtonClick);

            var favoritesButton = CloneButton(
                tabHandler.m_tabs[1].m_button,
                "Favorites",
                new Vector2(120.0f, 35.0f),
                new Vector2(482.0f, -33.1f)
            );

            var favoritesPanel = CreateFavoritesPanel(startGamePanel);

            tabHandler.m_tabs.Add(new TabHandler.Tab
            {
                m_button = favoritesButton.GetComponent<Button>(),
                m_default = false,
                m_page = favoritesPanel.GetComponent<RectTransform>(),
                m_onClick = favoritesEvent,
            });
        }

        private Transform CreateFavoritesPanel(Transform startGamePanel)
        {
            var joinPanel = startGamePanel.Find("JoinPanel");

            var favoritesPanel = Instantiate(joinPanel, joinPanel.parent);
            favoritesPanel.transform.SetSiblingIndex(joinPanel.transform.GetSiblingIndex() + 1);

            favoritesPanel.name = "FavoritesPanel";

            _serverListRoot = favoritesPanel.Find("ServerList/ListRoot").gameObject;
            _serverListElement = favoritesPanel.Find("ServerList/ServerElement").gameObject;

            var topicText = favoritesPanel.Find("topic").GetComponent<Text>();
            topicText.text = "Favorite Servers";

            var disabledElements = new[]
            {
                "serverCount",
                "FilterField",
                "FriendGames",
                "PublicGames",
                "Join manually",
            };

            foreach (var elementName in disabledElements)
            {
                favoritesPanel.Find(elementName).gameObject.SetActive(false);
            }

            var joinButton = favoritesPanel.Find("Join manually").GetComponent<Button>();

            _refreshButton = favoritesPanel.Find("Refresh").GetComponent<Button>();

            _connectButton = favoritesPanel.Find("Join").GetComponent<Button>();

            _addButton = CloneButton(
                joinButton,
                "Add",
                new Vector2(180.0f, 45.0f),
                new Vector2(-38.0f, -114.5f)
            );

            _removeButton = CloneButton(
                joinButton,
                Localization.instance.Localize("$menu_remove"),
                new Vector2(180.0f, 45.0f),
                new Vector2(148.0f, -114.5f)
            );

            return favoritesPanel;
        }

        private void CreateAddToFavoritesButton()
        {
            var joinButton = gameObject.transform.Find("StartGame/Panel/JoinPanel/Join manually");
            var rectTransform = joinButton.GetComponent<RectTransform>();

            _addToFavoritesButton = CloneButton(
                joinButton.GetComponent<Button>(),
                "Add to favorites",
                rectTransform.sizeDelta,
                new Vector2(191.0f, -165.0f)
            );
        }

        private GameObject CreateAddDialog()
        {
            var dialogObject = gameObject.transform.Find("JoinIP");
            var addDialog = Instantiate(dialogObject, dialogObject.parent);

            addDialog.Find("panel/Text").GetComponent<Text>().text = "Add Favorite Server";

            addDialog.Find("panel/Join/Text").GetComponent<Text>().text = "Add";

            var addButton = addDialog.Find("panel/Join").GetComponent<Button>();
            addButton.onClick = new Button.ButtonClickedEvent();
            addButton.onClick.AddListener(OnAddServerAddButtonClick);

            var cancelButton = addDialog.Find("panel/Cancel").GetComponent<Button>();
            cancelButton.onClick = new Button.ButtonClickedEvent();
            cancelButton.onClick.AddListener(OnAddServerCancelButtonClick);

            return addDialog.gameObject;
        }

        private void OnAddServerAddButtonClick()
        {
            var serverAddress = _addDialog.transform.Find("panel/AddressField").GetComponent<InputField>().text;
            var serverData = SteamService.GetServerData(serverAddress);

            if (serverData != null)
            {
                serverData.m_steamHostAddr.ToString(out var address, true);
                _logger.LogDebug($"AddFavoriteServer Host={address}");

                SteamService.AddFavoriteServer(serverData);
                OnRefreshButtonClick();
            }

            _addDialog.SetActive(false);
        }

        private void OnAddServerCancelButtonClick()
        {
            _addDialog.SetActive(false);
        }

        private GameObject CreateRemoveDialog()
        {
            var dialogObject = gameObject.transform.Find("StartGame/RemoveWorldDialog");
            var removeDialog = Instantiate(dialogObject, dialogObject.parent);

            removeDialog.Find("Dialog/topic").GetComponent<Text>().text = "Remove Favorite Server?";

            var yesButton = removeDialog.Find("Dialog/ButtonYes").GetComponent<Button>();
            yesButton.onClick = new Button.ButtonClickedEvent();
            yesButton.onClick.AddListener(OnRemoveServerYesButtonClick);

            var noButton = removeDialog.Find("Dialog/ButtonNo").GetComponent<Button>();
            noButton.onClick = new Button.ButtonClickedEvent();
            noButton.onClick.AddListener(OnRemoveServerNoButtonClick);

            return removeDialog.gameObject;
        }

        private void OnRemoveServerYesButtonClick()
        {
            if (_selectedServerIndex != -1)
            {
                var serverData = _serverDataList[_selectedServerIndex];

                serverData.m_steamHostAddr.ToString(out var address, true);
                _logger.LogDebug($"RemoveFavoriteServer Host={address}");

                SteamService.RemoveFavoriteServer(serverData);
                OnRefreshButtonClick();
            }

            _removeDialog.gameObject.SetActive(false);
        }

        private void OnRemoveServerNoButtonClick()
        {
            _removeDialog.gameObject.SetActive(false);
        }

        private static Button CloneButton(Button button, string label, Vector2 size, Vector2 position)
        {
            var clone = Instantiate(button, button.transform.parent);
            clone.transform.SetSiblingIndex(button.transform.GetSiblingIndex() + 1);
            clone.name = label;

            var text = clone.GetComponentInChildren<Text>();
            text.text = label;

            var rect = clone.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            return clone;
        }
    }
}