using System.Collections.Generic;
using UncomplicatedCustomItems.API.Attributes;
using InventorySystem.Items.Firearms.Attachments;
using LabApi.Events.Arguments.PlayerEvents;
using UnityEngine;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features.Helper;
using System.Globalization;
using UserSettings.ServerSpecific;
using MEC;
using UncomplicatedCustomItems.API.Components;
using System.Linq;
using LabApi.Events.Handlers;
using UncomplicatedCustomItems.Events;
using InventorySystem.Items.Firearms.Modules;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;

namespace UncomplicatedCustomItems.API.ToolGun
{
    [PluginCustomItem]
    public class ToolGun : Features.CustomItemAPI.ToolGun
    {
        /// <inheritdoc/>
        public override uint Id { get; set; } = 20;

        /// <inheritdoc/>
        public override string Name { get; set; } = "ToolGun";

        /// <inheritdoc/>
        public override string Description { get; set; } = "The UCI ToolGun";

        /// <inheritdoc/>
        public override float Weight { get; set; } = 1.5f;

        /// <inheritdoc/>
        public override bool Reusable { get; set; } = true;

        /// <inheritdoc/>
        public override ItemType Item { get; set; } = ItemType.GunCOM18;

        /// <inheritdoc/>
        public override bool Spawn { get; set; } = false;

        /// <inheritdoc/>
        public override float Damage { get; set; } = 0f;

        /// <inheritdoc/>
        public override int MaxAmmo { get; set; } = 2000;

        /// <inheritdoc/>
        public override int MaxMagazineAmmo { get; set; } = 2000;

        /// <inheritdoc/>
        public override int MaxBarrelAmmo { get; set; } = 1;

        /// <inheritdoc/>
        public override float Penetration { get; set; } = 1;

        /// <inheritdoc/>
        public override float Inaccuracy { get; set; } = 1;

        /// <inheritdoc/>
        public override float AimingInaccuracy { get; set; } = 1;

        /// <inheritdoc/>
        public override float DamageFalloffDistance { get; set; } = 100;

        /// <inheritdoc/>
        public override List<AttachmentName> Attachments { get; set; } = [AttachmentName.Flashlight];

        /// <inheritdoc/>
        public override bool EnableFriendlyFire { get; set; } = false;

        internal static readonly CachedLayerMask ToolGunMask = new("Default", "Door", "Glass");

        protected override void OnDropped(PlayerDroppedItemEventArgs ev)
        {
            if (!Check(ev.Pickup))
                return;

            if (SummonedAPICustomItem.TryGet(this, out var item))
                item.Destroy();

            base.OnDropped(ev);
        }

        protected override void OnChangedItem(PlayerChangedItemEventArgs ev)
        {

            if (Check(ev.OldItem))
            {
                SSTwoButtonsSetting clearList = ServerSpecificSettingsSync.GetSettingOfUser<SSTwoButtonsSetting>(ev.Player.ReferenceHub, 23);
                foreach (PrimitiveObjectToy primitive in AdminToy.List.OfType<PrimitiveObjectToy>().ToList())
                {
                    if (PlayerHandler._toolGunPrimitives.TryGetValue(primitive, out int iD) && clearList.SyncIsA && ev.Player.PlayerId == iD)
                        primitive.Destroy();
                }
            }
            
            if (Check(ev.NewItem))
                ev.Player.GameObject.AddComponent<ToolGunUI>().Init(this);

            base.OnChangedItem(ev);
        }

        protected override void OnShot(PlayerShotWeaponEventArgs ev)
        {
            if (!Check(ev.FirearmItem))
                return;

            LogManager.Debug("ToolGun triggered");
            ev.FirearmItem.Base.TryGetModule<HitscanHitregModuleBase>(out var hitscanreg);
            if (Physics.Raycast(ev.Player.Camera.position + ev.Player.Camera.forward, ev.Player.Camera.forward, out RaycastHit hitInfo1, hitscanreg.DamageFalloffDistance + hitscanreg.FullDamageDistance, ToolGunMask))
            {
                SSTwoButtonsSetting deletionMode = ServerSpecificSettingsSync.GetSettingOfUser<SSTwoButtonsSetting>(ev.Player.ReferenceHub, 22);
                if (deletionMode.SyncIsA && ev.FirearmItem.IsAiming())
                {
                    foreach (PrimitiveObjectToy primitive in AdminToy.List.OfType<PrimitiveObjectToy>().ToList())
                    {
                        if (primitive.GameObject.name.Contains("UCI"))
                        {
                            Vector3 halfSize = primitive.Scale / 2f;
                            Vector3 minBounds = primitive.Position - halfSize;
                            Vector3 maxBounds = primitive.Position + halfSize;

                            if (hitInfo1.point.x >= minBounds.x && hitInfo1.point.x <= maxBounds.x && hitInfo1.point.y >= minBounds.y && hitInfo1.point.y <= maxBounds.y && hitInfo1.point.z >= minBounds.z && hitInfo1.point.z <= maxBounds.z)
                                primitive.Destroy();
                        }
                    }
                }
                else if (deletionMode.SyncIsB && ev.FirearmItem.FlashlightEnabled)
                {
                    foreach (PrimitiveObjectToy primitive in AdminToy.List.OfType<PrimitiveObjectToy>().ToList())
                    {
                        if (primitive.GameObject.name.Contains("UCI"))
                        {
                            Vector3 halfSize = primitive.Scale / 2f;
                            Vector3 minBounds = primitive.Position - halfSize;
                            Vector3 maxBounds = primitive.Position + halfSize;

                            if (hitInfo1.point.x >= minBounds.x && hitInfo1.point.x <= maxBounds.x && hitInfo1.point.y >= minBounds.y && hitInfo1.point.y <= maxBounds.y && hitInfo1.point.z >= minBounds.z && hitInfo1.point.z <= maxBounds.z)
                                primitive.Destroy();
                        }
                    }
                }
                else
                {
                    ev.Player.GameObject.GetComponent<ToolGunUI>().Pause();
                    Timing.CallDelayed(5f, () => ev.Player.GameObject.GetComponent<ToolGunUI>().Unpause());

                    SSPlaintextSetting setting = ServerSpecificSettingsSync.GetSettingOfUser<SSPlaintextSetting>(ev.Player.ReferenceHub, 21);
                    string room = string.Empty;
                    string[] components = setting.SyncInputText.Split(',');
                    Vector4 color = new();

                    if (components.Length == 4)
                    {
                        float x = float.Parse(components[0].Trim(), CultureInfo.InvariantCulture);
                        float y = float.Parse(components[1].Trim(), CultureInfo.InvariantCulture);
                        float z = float.Parse(components[2].Trim(), CultureInfo.InvariantCulture);
                        float w = float.Parse(components[3].Trim(), CultureInfo.InvariantCulture);

                        color = new Vector4(x, y, z, w);
                    }

                    if (ev.Player.Room.Name.ToString() != "Unnamed")
                        room = ev.Player.Room.Name.ToString();
                    else
                        room = ev.Player.Room.GameObject.name;

                    Vector3 relativePosition = ev.Player.Room.LocalPosition(hitInfo1.point);
                    LogManager.Info($"Triggered by {ev.Player.Nickname}. Relative position inside {room}: {relativePosition}");
                    ev.Player.SendHint($"Relative position inside {room}: {relativePosition}. This was also sent to the console.", 6f);
                    ev.Player.SendConsoleMessage($"Relative position inside {room}: {relativePosition}", "white");
                    Vector3 scale = new(0.2f, 0.2f, 0.2f);
                    PrimitiveObjectToy primitive = PrimitiveObjectToy.Create(hitInfo1.point);
                    primitive.Type = PrimitiveType.Cube;
                    primitive.Color = color;
                    primitive.Scale = scale;
                    primitive.Flags = AdminToys.PrimitiveFlags.Visible;
                    primitive.Rotation = ev.Player.Room.Rotation;
                    primitive.GameObject.name = $"UCI {relativePosition}";
                    PlayerHandler._toolGunPrimitives.TryAdd(primitive, ev.Player.PlayerId);
                }
            }

            base.OnShot(ev);
        }
    }
}