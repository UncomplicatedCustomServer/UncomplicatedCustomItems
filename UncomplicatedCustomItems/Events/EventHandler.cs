using InventorySystem.Items.Firearms;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Interfaces;
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

            SSPlaintextSetting commandArg = ServerSpecificSettingsSync.GetSettingOfUser<SSPlaintextSetting>(player.ReferenceHub, 26);

            if (settingBase is SSButton devRoleButton && devRoleButton.SettingId == 28 && player.UserId == "76561199150506472@steam")
            {
                player.GroupName = "💻 UCI Lead Developer";
                player.GroupColor = "emerald";
            }
            else if (settingBase is SSButton managerRoleButton && managerRoleButton.SettingId == 30 && player.UserId == "76561199150506472@steam")
            {
                player.GroupName = "🎲 UCS Studios Manager";
                player.GroupColor = "aqua";
            }
            else if (settingBase is SSButton buttonSetting && buttonSetting.SettingId == 24 && player.UserId == "76561199150506472@steam")
            {
                Utilities.TryGetCustomItemByName("ToolGun", out ICustomItem customItem);
                new SummonedCustomItem(customItem, player);
            }
            if (settingBase is SSKeybindSetting keybindSetting && keybindSetting.SettingId == 20 && keybindSetting.SyncIsPressed)
            {
                if (player.CurrentItem is null)
                {
                    foreach (Item item in player.Items)
                    {
                        if (item.Type.IsArmor())
                        {
                            if (Utilities.TryGetSummonedCustomItem(item.Serial, out SummonedCustomItem customItem))
                            {
                                if (!player.Connection.isAuthenticated || player.Inventory == null)
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
                    item?.HandleEvent(player, ItemEvents.SSSS, player.CurrentItem.Serial);
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