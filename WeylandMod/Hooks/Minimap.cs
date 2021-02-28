using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using WeylandMod.Utils;

namespace WeylandMod.Hooks
{
    internal static class MinimapHooks
    {
        private static ManualLogSource Logger;

        private static List<ZNet.PlayerInfo> m_playersInfo;
        private static float m_exploreTimer;
        private static ConditionalWeakTable<Texture2D, Texture2D> m_textureDuplicates;
        private static IDictionary<Minimap.PinType, Sprite> m_sharedPinIcons;

        public static void Init(ManualLogSource logger)
        {
            MinimapExt.Init(logger);

            Logger = logger;

            m_playersInfo = new List<ZNet.PlayerInfo>();
            m_exploreTimer = 0.0f;
            m_textureDuplicates = new ConditionalWeakTable<Texture2D, Texture2D>();
            m_sharedPinIcons = new Dictionary<Minimap.PinType, Sprite>();

            On.Minimap.Start += StartHook;
            On.Minimap.Update += UpdateHook;
            On.Minimap.AddPin += AddPinHook;
            On.Minimap.RemovePin_PinData += RemovePinHook;
            On.Minimap.UpdateNameInput += UpdateNameInputHook;
        }

        private static void StartHook(On.Minimap.orig_Start orig, Minimap self)
        {
            Logger.LogDebug($"{nameof(Minimap)}: Start IsServer={ZNet.instance.IsServer()}");
            orig(self);

            InitSharedPinIcons(self);

            ZRoutedRpc.instance.Register<ZPackage>(
                WeylandRpc.GetName("SharedMapUpdate"),
                (sender, package) => { self.RPC_SharedMapUpdate(sender, package); }
            );

            ZRoutedRpc.instance.Register<ZPackage>(
                WeylandRpc.GetName("SharedPinAdd"),
                (sender, package) => { self.RPC_SharedPinAdd(sender, package); }
            );

            ZRoutedRpc.instance.Register<ZPackage>(
                WeylandRpc.GetName("SharedPinRemove"),
                (sender, package) => { self.RPC_SharedPinRemove(sender, package); }
            );

            ZRoutedRpc.instance.Register<ZPackage>(
                WeylandRpc.GetName("SharedPinNameUpdate"),
                (sender, package) => { self.RPC_SharedPinNameUpdate(sender, package); }
            );
        }

        private static void InitSharedPinIcons(Minimap self)
        {
            m_sharedPinIcons.Clear();

            foreach (var icon in self.m_icons)
            {
                if (!MinimapExt.SharedPinTypes.Contains(icon.m_name))
                    continue;

                m_sharedPinIcons.Add(icon.m_name, CreateSharedPinSprite(icon.m_icon));
            }
        }

        private static Sprite CreateSharedPinSprite(Sprite sprite)
        {
            var readable = MakeReadableTexture2D(sprite.texture);

            var rect = new RectInt(
                (int) sprite.textureRect.x,
                (int) sprite.textureRect.y,
                (int) sprite.rect.width,
                (int) sprite.rect.height
            );

            var texture = new Texture2D(rect.width, rect.height, readable.format, false);
            var pixels = readable.GetPixels(rect.x, rect.y, rect.width, rect.height, 0);

            var pinColor = WeylandConfig.SharedMap.SharedPinColor.Value;
            for (var index = 0; index < pixels.Length; ++index)
            {
                var alpha = pixels[index].a;
                pixels[index] = (pixels[index] + pinColor) / 2.0f;
                pixels[index].a = alpha;
            }

            texture.SetPixels(pixels);
            texture.Apply(true);

            return Sprite.Create(texture, sprite.rect, new Vector2(0.5f, 0.5f));
        }

        private static Texture2D MakeReadableTexture2D(Texture2D texture)
        {
            // based on https://stackoverflow.com/a/44734346
            if (texture.isReadable)
            {
                return texture;
            }

            if (m_textureDuplicates.TryGetValue(texture, out var readable))
            {
                return readable;
            }

            var render = RenderTexture.GetTemporary(
                texture.width,
                texture.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear
            );

            Graphics.Blit(texture, render);

            var active = RenderTexture.active;
            RenderTexture.active = render;

            readable = new Texture2D(texture.width, texture.height, texture.format, false);
            readable.ReadPixels(new Rect(0, 0, render.width, render.height), 0, 0);
            readable.Apply();

            RenderTexture.active = active;
            RenderTexture.ReleaseTemporary(render);

            m_textureDuplicates.Add(texture, readable);
            return readable;
        }

        private static void UpdateHook(On.Minimap.orig_Update orig, Minimap self)
        {
            orig(self);

            var sharedMap = WeylandConfig.SharedMap;
            if (sharedMap.SharedExplorationEnabled.Value || sharedMap.ForcePublicPosition.Value)
            {
                m_exploreTimer += Time.deltaTime;
                if (m_exploreTimer <= self.m_exploreInterval)
                    return;

                m_exploreTimer = 0.0f;

                m_playersInfo.Clear();
                ZNet.instance.GetOtherPublicPlayers(m_playersInfo);

                foreach (var playerInfo in m_playersInfo)
                {
                    self.Explore(playerInfo.m_position, self.m_exploreRadius);
                }
            }
        }

        private static Minimap.PinData AddPinHook(
            On.Minimap.orig_AddPin orig, Minimap self, Vector3 pos, Minimap.PinType type,
            string name, bool save, bool isChecked)
        {
            var pin = orig(self, pos, type, name, save, isChecked);

            if (!WeylandConfig.SharedMap.SharedPinsEnabled.Value)
                return pin;

            Logger.LogDebug($"AddPin {pos} {type} {name} {save} {isChecked}");

            if (!save && m_sharedPinIcons.TryGetValue(pin.m_type, out var icon))
            {
                pin.m_icon = icon;
            }

            if (save && MinimapExt.SharedPinTypes.Contains(type) && Player.m_localPlayer != null)
            {
                var pkg = new ZPackage();
                pkg.Write(0); // version
                SharedPinData.Write(pin, pkg);

                ZRoutedRpc.instance.InvokeRoutedRPC(WeylandRpc.GetName("SharedPinAdd"), pkg);
            }

            return pin;
        }

        private static void RemovePinHook(On.Minimap.orig_RemovePin_PinData orig, Minimap self, Minimap.PinData pin)
        {
            orig(self, pin);

            if (!WeylandConfig.SharedMap.SharedPinsEnabled.Value)
                return;

            Logger.LogDebug($"RemovePin {pin.m_pos} {pin.m_type} {pin.m_name}");

            if (pin.m_save && MinimapExt.SharedPinTypes.Contains(pin.m_type) && Player.m_localPlayer != null)
            {
                var pkg = new ZPackage();
                pkg.Write(0); // version
                SharedPinData.Write(pin, pkg);

                ZRoutedRpc.instance.InvokeRoutedRPC(WeylandRpc.GetName("SharedPinRemove"), pkg);
            }
        }

        private static void UpdateNameInputHook(On.Minimap.orig_UpdateNameInput orig, Minimap self)
        {
            var pin = self.m_namePin;
            var oldName = pin?.m_name;

            orig(self);

            if (!WeylandConfig.SharedMap.SharedPinsEnabled.Value)
                return;

            if (pin == null || oldName == pin.m_name)
                return;

            Logger.LogDebug($"UpdateName {pin?.m_pos} {pin?.m_type} {oldName} {pin?.m_name}");

            var pkg = new ZPackage();
            pkg.Write(0); // version
            SharedPinData.Write(pin, pkg);

            ZRoutedRpc.instance.InvokeRoutedRPC(WeylandRpc.GetName("SharedPinNameUpdate"), pkg);
        }
    }

    internal static class MinimapExt
    {
        private const int SHAREDMAPVERSION = 1;

        private static ManualLogSource Logger;

        public static readonly ISet<Minimap.PinType> SharedPinTypes = new HashSet<Minimap.PinType>
        {
            Minimap.PinType.Icon0,
            Minimap.PinType.Icon1,
            Minimap.PinType.Icon2,
            Minimap.PinType.Icon3,
            Minimap.PinType.Icon4,
        };

        public static void Init(ManualLogSource logger)
        {
            Logger = logger;
        }

        public static void SharedMapSend(this Minimap self)
        {
            if (!WeylandConfig.SharedMap.SharedExplorationEnabled.Value)
                return;

            ZRoutedRpc.instance.InvokeRoutedRPC(
                WeylandRpc.GetName("SharedMapUpdate"),
                self.GetSharedMap()
            );
        }

        public static void RPC_SharedMapUpdate(this Minimap self, long sender, ZPackage pkg)
        {
            Logger.LogDebug($"{nameof(Minimap)}: RPC_SharedMapUpdate Sender={sender} PackageSize={pkg.Size()}");

            if (!WeylandConfig.SharedMap.SharedExplorationEnabled.Value)
                return;

            self.SetSharedMap(pkg);

            if (!ZNet.m_isServer)
                return;

            ZRoutedRpc.instance.OthersInvokeRoutedRPC(WeylandRpc.GetName("SharedMapUpdate"), self.GetSharedMap());
        }

        public static void RPC_SharedPinAdd(this Minimap self, long sender, ZPackage pkg)
        {
            Logger.LogDebug($"{nameof(Minimap)}: RPC_SharedPinAdd Sender={sender} PackageSize={pkg.Size()}");

            if (!WeylandConfig.SharedMap.SharedPinsEnabled.Value)
                return;

            self.SharedPinAdd(pkg);

            if (!ZNet.m_isServer)
                return;

            pkg.SetPos(0);

            ZRoutedRpc.instance.OthersInvokeRoutedRPC(WeylandRpc.GetName("SharedPinAdd"), pkg);
        }

        public static void RPC_SharedPinRemove(this Minimap self, long sender, ZPackage pkg)
        {
            Logger.LogDebug($"{nameof(Minimap)}: RPC_SharedPinRemove Sender={sender} PackageSize={pkg.Size()}");

            if (!WeylandConfig.SharedMap.SharedPinsEnabled.Value)
                return;

            self.SharedPinRemove(pkg);

            if (!ZNet.m_isServer)
                return;

            pkg.SetPos(0);

            ZRoutedRpc.instance.OthersInvokeRoutedRPC(WeylandRpc.GetName("SharedPinRemove"), pkg);
        }

        public static void RPC_SharedPinNameUpdate(this Minimap self, long sender, ZPackage pkg)
        {
            Logger.LogDebug($"{nameof(Minimap)}: RPC_SharedPinNameUpdate Sender={sender} PackageSize={pkg.Size()}");

            if (!WeylandConfig.SharedMap.SharedPinsEnabled.Value)
                return;

            self.SharedPinNameUpdate(pkg);

            if (!ZNet.m_isServer)
                return;

            pkg.SetPos(0);

            ZRoutedRpc.instance.OthersInvokeRoutedRPC(WeylandRpc.GetName("SharedPinNameUpdate"), pkg);
        }

        private static void SharedPinAdd(this Minimap self, ZPackage pkg)
        {
            pkg.ReadInt(); // version
            var pin = SharedPinData.Read(pkg);

            var closestPin = self.GetClosestPinWithType(pin.Pos, pin.Type, 1.0f);
            if (closestPin != null)
                return;

            self.AddPin(pin.Pos, pin.Type, pin.Name, false, false);
        }

        private static void SharedPinRemove(this Minimap self, ZPackage pkg)
        {
            pkg.ReadInt(); // version
            var pin = SharedPinData.Read(pkg);

            var closestPin = self.GetClosestPinWithType(pin.Pos, pin.Type, 1.0f);
            if (closestPin == null || closestPin.m_save)
                return;

            self.RemovePin(closestPin);
        }

        private static void SharedPinNameUpdate(this Minimap self, ZPackage pkg)
        {
            pkg.ReadInt(); // version
            var pin = SharedPinData.Read(pkg);

            var closestPin = self.GetClosestPinWithType(pin.Pos, pin.Type, 1.0f);
            if (closestPin == null || closestPin.m_save)
                return;

            Logger.LogDebug($"PinNameUpdate {closestPin.m_pos} {pin.Type} {pin.Name}");
            closestPin.m_name = pin.Name;
        }

        private static Minimap.PinData GetClosestPinWithType(
            this Minimap self, Vector3 pos, Minimap.PinType type, float radius)
        {
            Minimap.PinData closestPin = null;
            var closestDist = 0.0f;

            foreach (var pin in self.m_pins)
            {
                if (pin.m_type != type)
                    continue;

                var dist = global::Utils.DistanceXZ(pos, pin.m_pos);
                if (dist <= radius && (closestPin == null || dist < closestDist))
                {
                    closestPin = pin;
                    closestDist = dist;
                }
            }

            return closestPin;
        }

        private static ZPackage GetSharedMap(this Minimap self)
        {
            var pkg = new ZPackage();
            pkg.Write(SHAREDMAPVERSION);
            pkg.Write(MapCompression.Compress(self.m_explored));

            var sharedPins = self.m_pins
                .Where(pin => pin.m_save || ZNet.m_isServer)
                .Where(pin => SharedPinTypes.Contains(pin.m_type))
                .ToArray();

            pkg.Write(sharedPins.Length);
            foreach (var pin in sharedPins)
            {
                SharedPinData.Write(pin, pkg);
            }

            return pkg;
        }

        private static void SetSharedMap(this Minimap self, ZPackage pkg)
        {
            pkg.ReadInt();

            var explored = MapCompression.Decompress(pkg.ReadByteArray());
            self.ApplySharedMapExplored(explored);

            var pinsLength = pkg.ReadInt();
            for (var index = 0; index < pinsLength; ++index)
            {
                var pin = SharedPinData.Read(pkg);

                var closestPin = self.GetClosestPinWithType(pin.Pos, pin.Type, 1.0f);
                if (closestPin == null)
                {
                    self.AddPin(pin.Pos, pin.Type, pin.Name, false, false);
                }
                else if (!closestPin.m_save)
                {
                    closestPin.m_name = pin.Name;
                }
            }
        }

        private static void ApplySharedMapExplored(this Minimap self, bool[] explored)
        {
            if (explored.Length != self.m_explored.Length)
            {
                Logger.LogError($"{nameof(Minimap)}: invalid explored array length");
                return;
            }

            if (ZNet.m_isServer)
            {
                for (var index = 0; index < explored.Length; ++index)
                {
                    if (explored[index])
                    {
                        self.m_explored[index] = self.m_explored[index] || explored[index];
                    }
                }
            }
            else
            {
                for (var index = 0; index < explored.Length; ++index)
                {
                    if (explored[index])
                    {
                        self.Explore(index % self.m_textureSize, index / self.m_textureSize);
                    }
                }

                self.m_fogTexture.Apply();
            }
        }
    }

    internal class SharedPinData
    {
        public Vector3 Pos;
        public Minimap.PinType Type;
        public string Name;

        public static SharedPinData Read(ZPackage pkg)
        {
            return new SharedPinData
            {
                Pos = new Vector3(pkg.ReadSingle(), 0.0f, pkg.ReadSingle()),
                Type = (Minimap.PinType) pkg.ReadByte(),
                Name = pkg.ReadString(),
            };
        }

        public static void Write(Minimap.PinData data, ZPackage pkg)
        {
            pkg.Write(data.m_pos.x);
            pkg.Write(data.m_pos.z);
            pkg.Write((byte) data.m_type);
            pkg.Write(data.m_name);
        }
    }
}