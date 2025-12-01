using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API;
using LabApi.Features.Wrappers;
using UnityEngine;
using UncomplicatedCustomItems.API.Interfaces;

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

        /// <summary>
        /// Initializes the ECR integration by attempting to patch the ECR plugin.
        /// </summary>
        public static void Init()
        {
            if (!Plugin.Instance.Config.EnableECRIntegration)
            {
                LogManager.Debug("The ECR integration is disabled!");
                return;
            }

            if (TryPatchECRIntegration())
            {
                _isEcrFound = true;
                LogManager.Silent("ECR Integration patched successfully.");
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
                MethodBase targetMethod = GetTargetMethod();
                if (targetMethod == null)
                {
                    LogManager.Debug("ECR target method not found - ECR may not be installed.");
                    return false;
                }

                MethodInfo prefixMethod = typeof(ECRIntegration).GetMethod(nameof(Prefix), BindingFlags.Static | BindingFlags.Public);

                if (prefixMethod == null)
                {
                    LogManager.Error("Failed to find Prefix method for ECR patching.");
                    return false;
                }

                Plugin.Instance._harmony.Patch(targetMethod, new HarmonyMethod(prefixMethod));
                _isPatched = true;
                LogManager.Silent($"Successfully patched ECR's 'TryAddItem' method.");
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
        private static MethodBase GetTargetMethod()
        {
            try
            {
                Type customRoleType = FindTypeInLoadedAssemblies("Exiled.CustomRoles.API.Features.CustomRole");
                if (customRoleType == null)
                {
                    LogManager.Debug($"Type 'Exiled.CustomRoles.API.Features.CustomRole' not found in loaded assemblies.");
                    return null;
                }

                Type exiledPlayerType = Type.GetType("Exiled.API.Features.Player, Exiled.API");
                if (exiledPlayerType == null)
                {
                    LogManager.Error($"Could not load Exiled player type: Exiled.API.Features.Player, Exiled.API");
                    return null;
                }

                Type[] parameterTypes = [exiledPlayerType, typeof(string)];
                MethodInfo method = customRoleType.GetMethod("TryAddItem", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, parameterTypes, null);

                if (method == null)
                    LogManager.Debug($"Method 'TryAddItem' not found on type 'Exiled.CustomRoles.API.Features.CustomRole'.");

                return method;
            }
            catch (Exception ex)
            {
                LogManager.Error($"Error finding target method: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Searches all loaded assemblies for a type with the specified full name.
        /// </summary>
        /// <param name="fullTypeName">The full name of the type to find.</param>
        /// <returns>The Type if found; null otherwise.</returns>
        private static Type FindTypeInLoadedAssemblies(string fullTypeName) => AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetTypesFromAssembly).FirstOrDefault(t => t.FullName == fullTypeName);

        /// <summary>
        /// Safely retrieves all types from an assembly, handling ReflectionTypeLoadException.
        /// </summary>
        /// <param name="assembly">The assembly to get types from.</param>
        /// <returns>Array of types from the assembly.</returns>
        private static Type[] GetTypesFromAssembly(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null).ToArray();
            }
            catch (Exception ex)
            {
                LogManager.Debug($"Failed to load types from assembly '{assembly.FullName}': {ex.Message}");
                return Array.Empty<Type>();
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
                string roleName = __instance.GetType().GetProperty("Name")?.GetValue(__instance)?.ToString() ?? "Unknown";
                LogManager.Debug($"ECR Integration intercepted item '{itemName}' for role '{roleName}'");

                Player labPlayer = GetLabPlayerFromExiledPlayer(player);
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
        internal static Player GetLabPlayerFromExiledPlayer(object exiledPlayer)
        {
            try
            {
                Type playerType = exiledPlayer.GetType();
                PropertyInfo gameObjectProperty = playerType.GetProperty("GameObject", BindingFlags.Public | BindingFlags.Instance);

                if (gameObjectProperty == null)
                {
                    LogManager.Error("Could not find 'GameObject' property on Exiled Player object.");
                    return null;
                }

                GameObject gameObject = gameObjectProperty.GetValue(exiledPlayer) as GameObject;
                if (gameObject == null)
                {
                    LogManager.Error("GameObject property returned null.");
                    return null;
                }

                if (!Player.TryGet(gameObject, out Player labPlayer))
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