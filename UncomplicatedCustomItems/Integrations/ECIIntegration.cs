using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.Integrations
{
    internal static class ECIIntegration
    {
        private static bool _isPatched = false;
        private static bool _isECIFound = false;

        /// <summary>
        /// Initializes the ECI integration by attempting to patch the ECI plugin.
        /// </summary>
        public static void Init()
        {
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

        /// <summary>
        /// Attempts to patch the ECI Give method with a Harmony prefix.
        /// </summary>
        /// <returns>True if patching succeeded or was already applied; false otherwise.</returns>
        private static bool TryPatchECIIntegration()
        {
            if (_isPatched)
                return true;

            try
            {
                MethodBase targetMethod = GetTargetMethod();
                if (targetMethod == null)
                {
                    LogManager.Debug("ECI target method not found - ECI may not be installed.");
                    return false;
                }

                MethodInfo prefixMethod = typeof(ECIIntegration).GetMethod(nameof(Prefix), BindingFlags.Static | BindingFlags.Public);

                if (prefixMethod == null)
                {
                    LogManager.Error("Failed to find Prefix method for ECI patching.");
                    return false;
                }

                Plugin.Instance._harmony.Patch(targetMethod, new HarmonyMethod(prefixMethod));
                _isPatched = true;
                LogManager.Silent($"Successfully patched ECI's 'Give' method.");
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to patch ECI Integration: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Locates the ECI CustomItem.Give method via reflection.
        /// </summary>
        /// <returns>The MethodBase of the target method, or null if not found.</returns>
        private static MethodBase GetTargetMethod()
        {
            try
            {
                Type customItemType = FindTypeInLoadedAssemblies("Exiled.CustomItems.API.Features.CustomItem");
                if (customItemType == null)
                {
                    LogManager.Debug($"Type 'Exiled.CustomItems.API.Features.CustomItem' not found in loaded assemblies.");
                    return null;
                }

                Type exiledPlayerType = Type.GetType("Exiled.API.Features.Player, Exiled.API");
                if (exiledPlayerType == null)
                {
                    LogManager.Error($"Could not load Exiled player type: Exiled.API.Features.Player, Exiled.API");
                    return null;
                }

                Type exiledItemType = Type.GetType("Exiled.API.Features.Items.Item, Exiled.API");
                if (exiledItemType == null)
                {
                    LogManager.Error($"Could not load Exiled Item type: Exiled.API.Features.Items.Item, Exiled.API");
                    return null;
                }

                Type[] parameterTypes = [exiledPlayerType, exiledItemType, typeof(bool)];
                MethodInfo method = customItemType.GetMethod("Give", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, parameterTypes, null);

                if (method == null)
                    LogManager.Debug($"Method 'Give' not found on type 'Exiled.CustomItems.API.Features.CustomItem'.");

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
        /// Harmony prefix patch for ECI's Give method.
        /// Intercepts custom item additions and handles UCI custom items.
        /// </summary>
        /// <param name="__instance">The CustomItem instance calling Give.</param>
        /// <param name="player">The Exiled player object receiving the item.</param>
        /// <param name="item">the item to add.</param>
        /// <returns>False to skip original method; true to continue to original method.</returns>
        public static bool Prefix(object __instance, object player, object item)
        {
            try
            {
                string itemName = __instance.GetType().GetProperty("Name")?.GetValue(__instance)?.ToString() ?? "Unknown";
                LogManager.Debug($"ECI Integration intercepted item '{item}' for item '{itemName}'");

                Player labPlayer = ECRIntegration.GetLabPlayerFromExiledPlayer(player);
                if (labPlayer == null)
                {
                    LogManager.Error("Failed to convert Exiled player to LabAPI player.");
                    return true;
                }

                if (Utilities.TryGetCustomItem(GetLabItemFromExiledItem(item).Serial, out ICustomItem customItem))
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

        internal static Item GetLabItemFromExiledItem(object exiledItem)
        {
            Type itemType = exiledItem.GetType();
            PropertyInfo serialProperty = itemType.GetProperty("Serial", BindingFlags.Public | BindingFlags.Instance);
            ushort serial = (ushort)serialProperty.GetValue(exiledItem);
            return Item.Get(serial);
        }
    }
}