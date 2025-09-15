using InventorySystem.Items.Firearms;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Extensions;
using UserSettings.ServerSpecific;
using Player = LabApi.Features.Wrappers.Player;

namespace UncomplicatedCustomItems.Events
{
    internal class EventHandler
    {
        public void OnValueReceived(ReferenceHub referenceHub, ServerSpecificSettingBase settingBase)
        {
            if (!Player.TryGet(referenceHub.gameObject, out Player player))
                return;

            if (settingBase is SSKeybindSetting keybindSetting && keybindSetting.SettingId == Plugin.Instance.Config.KeybindSettingId && keybindSetting.SyncIsPressed)
            {
                if (player.CurrentItem is null)
                {
                    foreach (Item item in player.Items)
                    {
                        if (item.Type.IsArmor())
                        {
                            if (Utilities.TryGetSummonedCustomItem(item.Serial, out SummonedCustomItem customItem))
                            {
                                if (player.GameObject == null)
                                    return;

                                customItem.HandleEvent(player, ItemEvents.SSSS, item.Serial);
                                break;
                            }
                            else
                                LogManager.Debug($"{nameof(OnValueReceived)}: {item} - {item.Serial} Is not a CustomItem.");
                        }
                    }
                }
                else if (Utilities.TryGetSummonedCustomItem(player.CurrentItem.Serial, out SummonedCustomItem item))
                    item.HandleEvent(player, ItemEvents.SSSS, player.CurrentItem.Serial);
            }
        }

        // Debugging Events.
        /// <summary>
        /// The debugging event for dropping a <see cref="Item"/>
        /// </summary>
        public void OnDrop(PlayerDroppingItemEventArgs ev)
        {
            if (ev.Item == null)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem customItem))
            {
                if (ev.Item.Serial == customItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} is dropping {customItem.CustomItem.Name}");
            }
            else return;
        }
        /// <summary>
        /// The debugging event for adding a <see cref="Item"/>
        /// </summary>
        public void OnDebuggingPickup(PlayerPickedUpItemEventArgs ev)
        {
            if (ev.Item == null)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem customItem))
            {
                if (ev.Item.Serial == customItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} is adding {customItem.CustomItem.Name}");
            }
            else return;
        }
        /// <summary>
        /// The debugging event for using a <see cref="Item"/>
        /// </summary>
        public void OnUse(PlayerUsingItemEventArgs ev)
        {
            if (ev.UsableItem == null)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.UsableItem.Serial, out SummonedCustomItem customItem))
            {
                if (ev.UsableItem.Serial == customItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} is using {customItem.CustomItem.Name}");
            }
            else return;
        }
        /// <summary>
        /// The debugging event for reloading a <see cref="Firearm"/>
        /// </summary>
        public void OnReloading(PlayerReloadingWeaponEventArgs ev)
        {
            if (ev.FirearmItem == null)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.FirearmItem.Serial, out SummonedCustomItem customItem))
            {
                if (ev.FirearmItem.Serial == customItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} is reloading {customItem.CustomItem.Name}");
            }
            else return;
        }
        /// <summary>
        /// The debugging event for shooting a <see cref="Firearm"/>
        /// </summary>
        /// <param name="ev"></param>
        public void OnShooting(PlayerShootingWeaponEventArgs ev)
        {
            if (ev.FirearmItem == null)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.FirearmItem.Serial, out SummonedCustomItem customItem))
            {
                if (ev.FirearmItem.Serial == customItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} is shooting {customItem.CustomItem.Name}");
            }
            else return;
        }
        /// <summary>
        /// The debugging event for throwing a <see cref="Throwable"/>
        /// </summary>
        /// <param name="ev"></param>
        public void OnThrown(PlayerThrewProjectileEventArgs ev)
        {
            if (ev.Projectile == null)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.Projectile.Serial, out SummonedCustomItem customItem))
            {
                if (ev.Projectile.Serial == customItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} has thrown {customItem.CustomItem.Name}");
            }
            else return;
        }
    }
}