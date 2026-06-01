using System;
using System.Reflection;
using HarmonyLib;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Manager;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.Integrations
{
    internal static class ECIIntegration
    {
        private static bool _isPatched = false;
        private static bool _isECIFound = false;

        public static void Init()
        {
            if (_isECIFound)
                return;

            if (!Plugin.Instance.Config.EnableECIIntegration)
            {
                LogManager.Debug("The ECI integration is disabled!");
                return;
            }

            if (TryPatchECIIntegration())
            {
                _isECIFound = true;
                LogManager.Silent("ECI Integration patched successfully.");
            }

            if (_isECIFound)
                LogManager.Debug("ECI found and integrated! :D");
        }

        private static bool TryPatchECIIntegration()
        {
            if (_isPatched)
                return true;

            try
            {
                MethodBase? targetMethod = GetTargetMethod();
                if (targetMethod == null)
                {
                    LogManager.Debug("ECI target method not found - ECI may not be installed.");
                    return false;
                }

                MethodInfo prefixMethod = AccessTools.Method(typeof(ECIIntegration), nameof(Prefix));
                if (prefixMethod == null)
                {
                    LogManager.Error("Failed to find Prefix method for ECI patching.");
                    return false;
                }

                Plugin.Instance._harmony.Patch(targetMethod, new HarmonyMethod(prefixMethod));
                _isPatched = true;
                LogManager.Silent("Successfully patched ECI's 'Give' method.");
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to patch ECI Integration: {ex}");
                return false;
            }
        }

        private static MethodBase? GetTargetMethod()
        {
            try
            {
                Type customItemType = AccessTools.TypeByName("Exiled.CustomItems.API.Features.CustomItem");
                if (customItemType == null)
                {
                    LogManager.Debug("Type 'Exiled.CustomItems.API.Features.CustomItem' not found in loaded assemblies.");
                    return null;
                }

                Type exiledPlayerType = AccessTools.TypeByName("Exiled.API.Features.Player");
                if (exiledPlayerType == null)
                {
                    LogManager.Error("Could not load Exiled player type: Exiled.API.Features.Player");
                    return null;
                }

                Type exiledItemType = AccessTools.TypeByName("Exiled.API.Features.Items.Item");
                if (exiledItemType == null)
                {
                    LogManager.Error("Could not load Exiled Item type: Exiled.API.Features.Items.Item");
                    return null;
                }

                MethodInfo method = AccessTools.Method(customItemType, "Give", [exiledPlayerType, exiledItemType, typeof(bool)]);
                if (method == null)
                    LogManager.Debug("Method 'Give' not found on type 'Exiled.CustomItems.API.Features.CustomItem'.");

                return method;
            }
            catch (Exception ex)
            {
                LogManager.Error($"Error finding target method: {ex.Message}");
                return null;
            }
        }

        public static bool Prefix(object __instance, object player, object item)
        {
            try
            {
                PropertyInfo nameProperty = AccessTools.Property(__instance.GetType(), "Name");
                string itemName = nameProperty?.GetValue(__instance)?.ToString() ?? "Unknown";
                LogManager.Debug($"ECI Integration intercepted item '{item}' for item '{itemName}'");

                Player? labPlayer = ECRIntegration.GetLabPlayerFromExiledPlayer(player);
                if (labPlayer == null)
                {
                    LogManager.Debug("Failed to convert Exiled player to LabAPI player.");
                    return true;
                }

                Item? labitem = GetLabItemFromExiledItem(item);
                if (labitem == null)
                {
                    LogManager.Debug("Failed to convert Exiled item to LabAPI item.");
                    return true;
                }

                if (Utilities.TryGetCustomItem(labitem.Serial, out ICustomItem customItem))
                {
                    LogManager.Debug($"Giving UCI custom item '{customItem.Name}' to {labPlayer.Nickname}");
                    new SummonedCustomItem(customItem, labPlayer);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error($"Error in ECI Integration prefix patch: {ex}");
                return true;
            }
        }

        internal static Item? GetLabItemFromExiledItem(object exiledItem)
        {
            if (exiledItem == null)
                return null;

            PropertyInfo serialProperty = AccessTools.Property(exiledItem.GetType(), "Serial");
            if (serialProperty == null)
                return null;

            ushort serial = (ushort)serialProperty.GetValue(exiledItem);
            return Item.Get(serial);
        }
    }
}