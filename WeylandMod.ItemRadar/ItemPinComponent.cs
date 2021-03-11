using UnityEngine;

namespace WeylandMod.ItemRadar
{
    internal class ItemPinComponent : MonoBehaviour
    {
        private string _itemName;
        private Vector2 _radius;
        private float _iconSize;
        private Sprite _itemIcon;
        private Minimap.PinData _pinData;

        public void Create(string itemName, float radius, float iconSize, Sprite itemIcon)
        {
            _itemName = itemName;
            _radius = new Vector2(radius, radius + 10.0f);
            _iconSize = iconSize;
            _itemIcon = itemIcon;
            _pinData = null;
        }

        private void Update()
        {
            var localPlayer = Player.m_localPlayer;
            if (!(bool) localPlayer)
                return;

            var distance = localPlayer.transform.position - transform.position;
            if (_pinData == null && distance.magnitude < _radius.x)
            {
                //var localizedName = Localization.instance.Localize(_itemName);
                _pinData = Minimap.instance.AddPin(
                    transform.position,
                    Minimap.PinType.None,
                    "",
                    false,
                    false
                );

                _pinData.m_icon = _itemIcon;
                _pinData.m_worldSize = _iconSize;
            }
            else if (_pinData != null && distance.magnitude > _radius.y)
            {
                Minimap.instance.RemovePin(_pinData);
                _pinData = null;
            }

            if (_pinData != null)
            {
                _pinData.m_pos = transform.position;
            }
        }

        private void OnDestroy()
        {
            if (_pinData != null)
            {
                Minimap.instance.RemovePin(_pinData);
                _pinData = null;
            }
        }
    }
}