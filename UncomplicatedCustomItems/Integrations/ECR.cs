using HarmonyLib;
using System;
using System.Reflection;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Manager;
using UncomplicatedCustomItems.API;
using LabApi.Features.Wrappers;
using UnityEngine;
using UncomplicatedCustomItems.API.Interfaces;
using System.Collections.Generic;

namespace UncomplicatedCustomItems.Integrations
{
    /// <summary>
    /// Provides integration with Exiled Custom Roles (ECR) plugin to handle custom item distribution.
    /// Patches ECR's TryAddItem method to intercept and handle UCI custom items.
    /// </summary>
    internal static class ECRIntegration
    {
        private static bool _isPatched = false;
        private static bool _isEcrFound = false;

        private static readonly Dictionary<Type, PropertyInfo?> NamePropertyCache = [];
        private static readonly Dictionary<Type, PropertyInfo?> GameObjectPropertyCache = [];

        /// <summary>
        /// Initializes the ECR integration by attempting to patch the ECR plugin.
        /// </summary>
        public static void Init()
        {
            if (_isPatched)
                return;

            if (!Plugin.Instance.Config.EnableECRIntegration)
            {
                LogManager.Debug("The ECR integration is disabled!");
                return;
            }

            if (TryPatchECRIntegration())
            {
                _isEcrFound = true;
                LogManager.Silent("ECI Integration patched successfully.");
            }

            if (_isEcrFound)
                LogManager.Debug("ECR found and integrated! :D");
        }

        /// <summary>
        /// Attempts to patch the ECR TryAddItem method with a Harmony prefix.
        /// </summary>
        /// <returns>True if patching succeeded or was already applied; false otherwise.</returns>
        private static bool TryPatchECRIntegration()
        {
            if (_isPatched)
                return true;

            try
            {
                MethodBase? targetMethod = GetTargetMethod();
                if (targetMethod == null)
                {
                    LogManager.Debug("ECR target method not found - ECR may not be installed.");
                    return false;
                }

                MethodInfo prefixMethod = AccessTools.Method(typeof(ECRIntegration), nameof(Prefix));
                if (prefixMethod == null)
                {
                    LogManager.Error("Failed to find Prefix method for ECR patching.");
                    return false;
                }

                Plugin.Instance._harmony.Patch(targetMethod, new HarmonyMethod(prefixMethod));
                _isPatched = true;
                LogManager.Silent("Successfully patched ECR's 'TryAddItem' method.");
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to patch ECR Integration: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Locates the ECR CustomRole.TryAddItem method via reflection.
        /// </summary>
        /// <returns>The MethodBase of the target method, or null if not found.</returns>
        private static MethodBase? GetTargetMethod()
        {
            try
            {
                Type customRoleType = AccessTools.TypeByName("Exiled.CustomRoles.API.Features.CustomRole");
                if (customRoleType == null)
                {
                    LogManager.Debug("Type 'Exiled.CustomRoles.API.Features.CustomRole' not found in loaded assemblies.");
                    return null;
                }

                Type exiledPlayerType = AccessTools.TypeByName("Exiled.API.Features.Player");
                if (exiledPlayerType == null)
                {
                    LogManager.Error("Could not load Exiled player type: Exiled.API.Features.Player");
                    return null;
                }

                MethodInfo method = AccessTools.Method(customRoleType, "TryAddItem", [exiledPlayerType, typeof(string)]);
                if (method == null)
                    LogManager.Debug("Method 'TryAddItem' not found on type 'Exiled.CustomRoles.API.Features.CustomRole'.");

                return method;
            }
            catch (Exception ex)
            {
                LogManager.Error($"Error finding target method: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Harmony prefix patch for ECR's TryAddItem method.
        /// Intercepts custom item additions and handles UCI custom items.
        /// </summary>
        /// <param name="__instance">The CustomRole instance calling TryAddItem.</param>
        /// <param name="player">The Exiled player object receiving the item.</param>
        /// <param name="itemName">The name of the item to add.</param>
        /// <param name="__result">The result to return to the original method.</param>
        /// <returns>False to skip original method; true to continue to original method.</returns>
        public static bool Prefix(object __instance, object player, string itemName, ref bool __result)
        {
            try
            {
                Type type = __instance.GetType();
                PropertyInfo? nameProperty = NamePropertyCache.GetOrAdd(type, () => AccessTools.Property(type, "GameObject"));
                string roleName = nameProperty?.GetValue(__instance)?.ToString() ?? "Unknown";
                LogManager.Debug($"ECR Integration intercepted item '{itemName}' for role '{roleName}'");

                Player? labPlayer = GetLabPlayerFromExiledPlayer(player);
                if (labPlayer == null)
                {
                    LogManager.Error("Failed to convert Exiled player to LabAPI player.");
                    return true;
                }

                if (uint.TryParse(itemName, out uint id) && Utilities.TryGetCustomItem(id, out ICustomItem item))
                {
                    LogManager.Debug($"Giving UCI custom item '{item.Name}' to {labPlayer.Nickname}");
                    new SummonedCustomItem(item, labPlayer);
                    __result = true;
                    return false;
                }
                else if (Utilities.TryGetCustomItemByName(itemName, out ICustomItem customItem))
                {
                    LogManager.Debug($"Giving UCI custom item '{customItem.Name}' to {labPlayer.Nickname}");
                    new SummonedCustomItem(customItem, labPlayer);
                    __result = true;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error($"Error in ECR Integration prefix patch: {ex}");
                return true;
            }
        }

        /// <summary>
        /// Converts an Exiled player object to a LabAPI Player object.
        /// </summary>
        /// <param name="exiledPlayer">The Exiled player object.</param>
        /// <returns>The corresponding LabAPI Player, or null if conversion fails.</returns>
        internal static Player? GetLabPlayerFromExiledPlayer(object exiledPlayer)
        {
            try
            {
                Type type = exiledPlayer.GetType();
                PropertyInfo? gameObjectProperty = GameObjectPropertyCache.GetOrAdd(type, () => AccessTools.Property(type, "GameObject"));
                if (gameObjectProperty == null)
                {
                    LogManager.Error("Could not find 'GameObject' property on Exiled Player object.");
                    return null;
                }

                GameObject? gameObject = gameObjectProperty.GetValue(exiledPlayer) as GameObject;
                if (gameObject == null)
                {
                    LogManager.Error("GameObject property returned null.");
                    return null;
                }

                if (!Player.TryGet(gameObject, out Player? labPlayer))
                {
                    LogManager.Error($"GameObject '{gameObject.name}' does not correspond to a valid player.");
                    return null;
                }

                return labPlayer;
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to convert Exiled player to LabAPI player: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Cleans up the integration state. Should be called when the plugin is disabled.
        /// </summary>
        public static void Cleanup()
        {
            _isPatched = false;
            _isEcrFound = false;
            LogManager.Debug("ECR Integration cleaned up.");
        }
    }
}