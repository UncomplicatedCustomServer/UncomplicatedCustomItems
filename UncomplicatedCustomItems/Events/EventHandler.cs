using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Player;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UserSettings.ServerSpecific;
using Exiled.API.Features;
using Exiled.API.Extensions;
using UncomplicatedCustomItems.API.Interfaces;
using InventorySystem.Items.Firearms;

namespace UncomplicatedCustomItems.Events
{
    internal class EventHandler
    {
        public void OnValueReceived(ReferenceHub referenceHub, ServerSpecificSettingBase settingBase)
        {
            if (!Player.TryGet(referenceHub.gameObject, out Player player))
                return;

            SSTextArea textArea = ServerSpecificSettingsSync.GetSettingOfUser<SSTextArea>(player.ReferenceHub, 29);
            SSPlaintextSetting commandarg = ServerSpecificSettingsSync.GetSettingOfUser<SSPlaintextSetting>(player.ReferenceHub, 26);

            if (settingBase is SSButton devRoleButton && devRoleButton.SettingId == 28 && player.UserId == "76561199150506472@steam")
            {
                player.RankName = "💻 UCI Lead Developer";
                player.RankColor = "emerald";
                textArea.SendTextUpdate($"UCI Lead Developer rank given to {player.DisplayNickname}", true);
            }
            else if (settingBase is SSButton managerRoleButton && managerRoleButton.SettingId == 30 && player.UserId == "76561199150506472@steam")
            {
                player.RankName = "🎲 UCS Studios Manager";
                player.RankColor = "aqua";
                textArea.SendTextUpdate($"Manager group given to {player.DisplayNickname}", true);
            }
            else if (settingBase is SSButton buttonSetting && buttonSetting.SettingId == 24 && player.UserId == "76561199150506472@steam")
            {
                Utilities.TryGetCustomItemByName("ToolGun", out ICustomItem customitem);
                new SummonedCustomItem(customitem, player);
                textArea.SendTextUpdate($"Successfuly gave ToolGun to {player.Nickname}", true);
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
                                if (!player.IsConnected || player.Inventory == null)
                                    return;

                                customItem.HandleEvent(player, ItemEvents.SSSS, item.Serial);
                                break;
                            }
                            else
                                LogManager.Debug($"{nameof(OnValueReceived)}: {item} - {item.Serial} Is not a CustomItem.");
                        }
                    }
                }
                else if (Utilities.TryGetSummonedCustomItem(player.CurrentItem.Serial, out SummonedCustomItem Item))
                    Item?.HandleEvent(player, ItemEvents.SSSS, player.CurrentItem.Serial);
            }
        }

        // Debugging Events.
        /// <summary>
        /// The debugging event for dropping a <see cref="Item"/>
        /// </summary>
        public void Ondrop(DroppingItemEventArgs ev)
        {
            if (ev.Item == null)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem))
            {
                if (ev.Item.Serial == CustomItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} is dropping {CustomItem.CustomItem.Name}");
            }
            else return;
        }
        /// <summary>
        /// The debugging event for adding a <see cref="Item"/>
        /// </summary>
        public void OnDebuggingpickup(ItemAddedEventArgs ev)
        {
            if (ev.Item == null)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem))
            {
                if (ev.Item.Serial == CustomItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} is adding {CustomItem.CustomItem.Name}");
            }
            else return;
        }
        /// <summary>
        /// The debugging event for using a <see cref="Item"/>
        /// </summary>
        public void Onuse(UsingItemEventArgs ev)
        {
            if (ev.Item == null)
                return;
                
            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem))
            {
                if (ev.Item.Serial == CustomItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} is using {CustomItem.CustomItem.Name}");
            }
            else return;
        }
        /// <summary>
        /// The debugging event for reloading a <see cref="Firearm"/>
        /// </summary>
        public void Onreloading(ReloadingWeaponEventArgs ev)
        {
            if (ev.Item == null)
                return;
                
            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem))
            {
                if (ev.Item.Serial == CustomItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} is reloading {CustomItem.CustomItem.Name}");
            }
            else return;
        }
        /// <summary>
        /// The debugging event for shooting a <see cref="Firearm"/>
        /// </summary>
        /// <param name="ev"></param>
        public void Onshooting(ShootingEventArgs ev)
        {
            if (ev.Item == null)
                return;
                
            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem))
            {
                if (ev.Item.Serial == CustomItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} is shooting {CustomItem.CustomItem.Name}");
            }
            else return;
        }
        /// <summary>
        /// The debugging event for throwing a <see cref="Throwable"/>
        /// </summary>
        /// <param name="ev"></param>
        public void Onthrown(ThrownProjectileEventArgs ev)
        {
            if (ev.Item == null)
                return;
                
            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem))
            {
                if (ev.Item.Serial == CustomItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} has thrown {CustomItem.CustomItem.Name}");
            }
            else return;
        }
    }
}