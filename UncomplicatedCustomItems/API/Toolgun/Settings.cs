using System.Collections.Generic;
using System.Linq;
using InventorySystem.Items;
using LabApi.Features.Wrappers;
using MEC;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UserSettings.ServerSpecific;

namespace UncomplicatedCustomItems.API.ToolGun
{
    internal static class Settings
    {
        private static ServerSpecificSettingBase[] PreviousDefinedSettings { get; set; } = [];
        public static List<ServerSpecificSettingBase> _settings =
        [
            new SSGroupHeader("UCI ToolGun Settings", hint: "If multiple are created any will work"),
            new SSPlaintextSetting(21, "Primitive Color", placeholder: "255, 0, 0, -1", hint: "The color of the primitives spawned by the ToolGun"),
            new SSTwoButtonsSetting(22, "Deletion Mode", "ADS", "FlashLight Toggle", hint: "Sets the deletion mode of the ToolGun"),
            new SSTwoButtonsSetting(23, "Delete Primitives when unequipped?", "Yes", "No")
        ];

        public static void GiveToPlayers()
        {
            if (PreviousDefinedSettings.Length == 0)
                PreviousDefinedSettings = ServerSpecificSettingsSync.DefinedSettings;

            ServerSpecificSettingsSync.SendOnJoinFilter = (_) => false;
            ServerSpecificSettingsSync.DefinedSettings = _settings.ToArray();

            if (Player.ReadyList.Count() > 0)
                Timing.CallDelayed(1, () => ServerSpecificSettingsSync.SendToPlayersConditionally(player =>
                {
                    ItemBase itemBase = player.inventory?.CurInstance;
                    if (itemBase == null)
                        return false;

                    bool hasSummoned = Utilities.TryGetSummonedCustomItem(itemBase.ItemSerial, out var summoned) && summoned is not null && summoned.HasModule(CustomFlags.ToolGun);
                    bool hasBase = SummonedBaseCustomItem.TryGet(itemBase.ItemSerial, out var baseSummoned) && baseSummoned is not null && baseSummoned.CustomItem is Features.CustomItemAPI.ToolGun;

                    return hasSummoned || hasBase;
                }));
        }

        internal static IEnumerator<float> ResetSettings()
        {
            for (; ; )
            {
                ResetPlayer();
                yield return 1f;
            }
        }

        private static void ResetPlayer()
        {
            ServerSpecificSettingsSync.SendOnJoinFilter = (_) => false;
            ServerSpecificSettingsSync.DefinedSettings = PreviousDefinedSettings;

            if (Player.ReadyList.Count() > 0)
                ServerSpecificSettingsSync.SendToPlayersConditionally(player =>
                {
                    ItemBase itemBase = player.inventory?.CurInstance;
                    if (itemBase == null)
                        return true;

                    bool hasSummoned = Utilities.TryGetSummonedCustomItem(itemBase.ItemSerial, out var summoned) && summoned is not null && summoned.HasModule(CustomFlags.ToolGun);
                    bool hasBase = SummonedBaseCustomItem.TryGet(itemBase.ItemSerial, out var baseSummoned) && baseSummoned is not null && baseSummoned.CustomItem is Features.CustomItemAPI.ToolGun;

                    return !(hasSummoned || hasBase);
                });
        }
    }
}