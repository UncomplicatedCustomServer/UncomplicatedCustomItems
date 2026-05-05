using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Extensions;
using UserSettings.ServerSpecific;

namespace UncomplicatedCustomItems.Events
{
    internal class SSSHandler
    {
        public static void Register()
        {
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnValueReceived;
        }

        public static void Unregister()
        {
            ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnValueReceived;
        }
        
        private static void OnValueReceived(ReferenceHub referenceHub, ServerSpecificSettingBase settingBase)
        {
            if (!Player.TryGet(referenceHub.gameObject, out Player player))
                return;

            if (settingBase is SSKeybindSetting keybindSetting && keybindSetting.SettingId == Plugin.Instance.Config.KeybindSettingId && keybindSetting.SyncIsPressed)
            {
                if (player.CurrentItem == null)
                {
                    foreach (Item item in player.Items)
                    {
                        if (item.Type.IsArmor())
                        {
                            if (Utilities.TryGetSummonedCustomItem(item.Serial, out SummonedCustomItem customItem))
                            {
                                customItem.HandleEvent(player, ItemEvents.SSSS, item.Serial);
                                break;
                            }
                            else
                                LogManager.Debug($"{item.Serial} Is not a CustomItem.");
                        }
                    }
                }
                else if (Utilities.TryGetSummonedCustomItem(player.CurrentItem.Serial, out SummonedCustomItem item))
                    item.HandleEvent(player, ItemEvents.SSSS, player.CurrentItem.Serial);
            }
        }
    }
}