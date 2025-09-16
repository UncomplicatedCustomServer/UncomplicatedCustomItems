using System.Collections.Generic;
using MEC;
using UncomplicatedCustomItems.API.Enums;
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
            Timing.CallDelayed(1, () => ServerSpecificSettingsSync.SendToPlayersConditionally(x => Utilities.TryGetSummonedCustomItem(x.inventory.CurInstance.ItemSerial, out var item) && item.HasModule(CustomFlags.ToolGun)));
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
            ServerSpecificSettingsSync.SendToPlayersConditionally(x => !Utilities.TryGetSummonedCustomItem(x.inventory.CurInstance.ItemSerial, out var item) || !item.HasModule(CustomFlags.ToolGun));
        }
    }
}