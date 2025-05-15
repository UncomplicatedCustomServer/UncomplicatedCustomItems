/*using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features.Core.UserSettings;
using Exiled.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch]
    public static class SettingIdUniquenessPatches
    {
        [HarmonyPatch(typeof(SettingBase), nameof(SettingBase.Register), new Type[] { typeof(IEnumerable<SettingBase>), typeof(Func<Player, bool>) })]
        public static class SettingBase_Register_Predicate_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref IEnumerable<SettingBase> settings, Func<Player, bool> predicate)
            {
                if (settings == null)
                    return true;

                List<SettingBase> originalAsList = settings.ToList();
                List<SettingBase> uniqueSettingsToAdd = new List<SettingBase>();
                List<string> problematicSettings = new List<string>();

                HashSet<int> existingGlobalIds = new HashSet<int>(SettingBase.List.Select(s => s.Id));

                foreach (SettingBase setting in originalAsList)
                {
                    if (setting == null) continue;

                    if (existingGlobalIds.Contains(setting.Id) || uniqueSettingsToAdd.Any(s => s.Id == setting.Id))
                        problematicSettings.Add($"ID: {setting.Id}, Label: '{setting.Label}'");
                    else
                        uniqueSettingsToAdd.Add(setting);
                }

                if (problematicSettings.Any())
                    LogManager.Debug($"{nameof(SettingBase_Register_Predicate_Patch)}:  Attempted to register settings with duplicate IDs. Skipping duplicates:\n" + string.Join("\n", problematicSettings));

                settings = uniqueSettingsToAdd;

                if (!settings.Any())
                {
                    LogManager.Debug($"{nameof(SettingBase_Register_Predicate_Patch)}: No unique settings left to register in batch.");
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(SettingBase), nameof(SettingBase.Register), new Type[] { typeof(Player), typeof(IEnumerable<SettingBase>) })]
        public static class SettingBase_Register_Player_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(Player player, ref IEnumerable<SettingBase> settings)
            {
                if (settings == null)
                    return true;

                List<SettingBase> originalAsList = settings.ToList();
                List<SettingBase> uniqueSettingsToAdd = new List<SettingBase>();
                List<string> problematicSettings = new List<string>();

                HashSet<int> existingGlobalIds = new HashSet<int>(SettingBase.List.Select(s => s.Id));

                foreach (SettingBase setting in originalAsList)
                {
                    if (setting == null) continue;

                    if (existingGlobalIds.Contains(setting.Id) || uniqueSettingsToAdd.Any(s => s.Id == setting.Id))
                        problematicSettings.Add($"ID: {setting.Id}, Label: '{setting.Label}' (for player {player.Nickname})");
                    else
                        uniqueSettingsToAdd.Add(setting);
                }

                if (problematicSettings.Any())
                    LogManager.Debug($"{nameof(SettingBase_Register_Player_Patch)}: Attempted to register settings with duplicate IDs for player {player.Nickname}. Skipping duplicates:\n" + string.Join("\n", problematicSettings));

                settings = uniqueSettingsToAdd;

                if (!settings.Any())
                {
                    LogManager.Debug($"{nameof(SettingBase_Register_Player_Patch)}: No unique settings left to register for player {player.Nickname}.");
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(SettingBase), "set_Id")]
        public static class SettingBase_SetId_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(SettingBase __instance, int value)
            {
                SettingBase conflictingSetting = SettingBase.List.FirstOrDefault(s => s != null && s.Id == value && s != __instance);
                if (conflictingSetting != null)
                {
                    LogManager.Debug($"{nameof(SettingBase_SetId_Patch)}:  Attempt to set ID of setting '{__instance.Label}' (current ID: {__instance.Id}) to {value}, but this ID is already in use by another setting ('{conflictingSetting.Label}'). Operation aborted.");
                    return false;
                }
                return true;
            }
        }
    }
}*/