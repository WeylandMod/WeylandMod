using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace WeylandMod.SharedMap
{
    internal class SharedMapComponent : MonoBehaviour
    {
        public static SharedMapComponent Instance { get; private set; }

        private const string RpcSharedMapUpdateName = "RPC_WeylandMod_SharedMapUpdate";
        public const string RpcSharedPinAddName = "RPC_WeylandMod_SharedPinAdd";
        public const string RpcSharedPinRemoveName = "RPC_WeylandMod_SharedPinRemove";
        public const string RpcSharedPinNameUpdateName = "RPC_WeylandMod_SharedPinNameUpdate";

        public GameObject SharedPinPrefab;

        private ManualLogSource _logger;
        private SharedMapConfig _config;
        private Minimap _minimap;
        private List<ZNet.PlayerInfo> _playersInfo;
        private float _exploreTimer;

        public void Create(ManualLogSource logger, SharedMapConfig config, Minimap minimap)
        {
            Instance = this;

            _logger = logger;
            _config = config;
            _minimap = minimap;
            _playersInfo = new List<ZNet.PlayerInfo>();
            _exploreTimer = 0.0f;
            SharedPinPrefab = null;
        }

        public void SendSharedMap()
        {
            _logger.LogDebug("SendSharedMap");

            ZRoutedRpc.instance.InvokeRoutedRPC(RpcSharedMapUpdateName, _minimap.GetSharedMap());
        }

        private void Start()
        {
            SharedPinPrefab = Instantiate(_minimap.m_pinPrefab);

            var pinImage = SharedPinPrefab.GetComponent<Image>();
            pinImage.material = new Material(pinImage.material);
            pinImage.color = _config.SharedPinsColor;

            ZRoutedRpc.instance.Register<ZPackage>(RpcSharedMapUpdateName, RPC_SharedMapUpdate);
            ZRoutedRpc.instance.Register<ZPackage>(RpcSharedPinAddName, RPC_SharedPinAdd);
            ZRoutedRpc.instance.Register<ZPackage>(RpcSharedPinRemoveName, RPC_SharedPinRemove);
            ZRoutedRpc.instance.Register<ZPackage>(RpcSharedPinNameUpdateName, RPC_SharedPinNameUpdate);
        }

        private void Update()
        {
            if (!_config.Enabled)
                return;

            _exploreTimer += Time.deltaTime;
            if (_exploreTimer <= _minimap.m_exploreInterval)
                return;

            _exploreTimer = 0.0f;

            _playersInfo.Clear();
            ZNet.instance.GetOtherPublicPlayers(_playersInfo);

            foreach (var playerInfo in _playersInfo.Where(playerInfo => playerInfo.m_publicPosition))
            {
                _minimap.Explore(playerInfo.m_position, _minimap.m_exploreRadius);
            }
        }

        private void RPC_SharedMapUpdate(long sender, ZPackage pkg)
        {
            _logger.LogDebug($"RPC_SharedMapUpdate Size={pkg.Size()}");

            _minimap.SetSharedMap(pkg);

            if (!ZNet.m_isServer)
                return;

            ZRoutedRpc.instance.OthersInvokeRoutedRPC(RpcSharedMapUpdateName, _minimap.GetSharedMap());
        }

        private void RPC_SharedPinAdd(long sender, ZPackage pkg)
        {
            _logger.LogDebug($"RPC_SharedPinAdd Size={pkg.Size()}");

            _minimap.SharedPinAdd(pkg);

            if (!ZNet.m_isServer)
                return;

            pkg.SetPos(0);

            ZRoutedRpc.instance.OthersInvokeRoutedRPC(RpcSharedPinAddName, pkg);
        }

        private void RPC_SharedPinRemove(long sender, ZPackage pkg)
        {
            _logger.LogDebug($"RPC_SharedPinRemove Size={pkg.Size()}");

            _minimap.SharedPinRemove(pkg);

            if (!ZNet.m_isServer)
                return;

            pkg.SetPos(0);

            ZRoutedRpc.instance.OthersInvokeRoutedRPC(RpcSharedPinRemoveName, pkg);
        }

        private void RPC_SharedPinNameUpdate(long sender, ZPackage pkg)
        {
            _logger.LogDebug($"RPC_SharedPinNameUpdate Size={pkg.Size()}");

            _minimap.SharedPinNameUpdate(pkg);

            if (!ZNet.m_isServer)
                return;

            pkg.SetPos(0);

            ZRoutedRpc.instance.OthersInvokeRoutedRPC(RpcSharedPinNameUpdateName, pkg);
        }
    }
}