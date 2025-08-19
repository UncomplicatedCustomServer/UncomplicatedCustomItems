using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API;
using LabApi.Features.Wrappers;
using UnityEngine;
using MEC;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.Integrations
{
    public static class ECRIntegration
    {
        private static bool _isPatched = false;
        private static Harmony _harmonyInstance;

        public static void Initialize(Harmony harmonyInstance)
        {
            _harmonyInstance = harmonyInstance;

            if (TryPatchECRIntegration())
            {
                LogManager.Silent("ECR Integration patched successfully on initialization.");
                return;
            }

            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
            LogManager.Silent("ECR Integration waiting for Exiled.CustomRoles to load...");
        }

        private static void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            if (_isPatched) return;

            if (args.LoadedAssembly.GetName().Name.Contains("Exiled.CustomRoles"))
            {
                LogManager.Silent($"Detected Exiled.CustomRoles assembly load: {args.LoadedAssembly.FullName}");

                Timing.CallDelayed(1f, () =>
                {
                    if (TryPatchECRIntegration())
                    {
                        LogManager.Silent("ECR Integration patched successfully after assembly load.");
                        AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;
                    }
                });
            }
        }

        private static bool TryPatchECRIntegration()
        {
            if (_isPatched) return true;

            try
            {
                MethodBase targetMethod = GetTargetMethod();
                if (targetMethod == null)
                {
                    LogManager.Silent("ECR target method not found - Exiled.CustomRoles may not be loaded yet.");
                    return false;
                }

                MethodInfo prefixMethod = typeof(ECRIntegration).GetMethod(nameof(Prefix), BindingFlags.Static | BindingFlags.Public);

                _harmonyInstance.Patch(targetMethod, new HarmonyMethod(prefixMethod));
                _isPatched = true;
                LogManager.Silent("ECR Integration successfully patched TryAddItem method.");
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to patch ECR Integration: {ex}");
                return false;
            }
        }

        private static MethodBase GetTargetMethod()
        {
            try
            {
                Type customRoleType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(GetTypesFromAssembly)
                    .FirstOrDefault(t => t.FullName == "Exiled.CustomRoles.API.Features.CustomRole");

                if (customRoleType == null)
                    return null;

                MethodInfo method = customRoleType.GetMethod("TryAddItem",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                    null,
                    [
                        Type.GetType("Exiled.API.Features.Player, Exiled.API"),
                        typeof(string)
                    ],
                    null);

                return method;
            }
            catch (Exception ex)
            {
                LogManager.Debug($"Error in GetTargetMethod(): {ex.Message}");
                return null;
            }
        }

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
            catch (Exception)
            {
                return [];
            }
        }

        public static bool Prefix(object __instance, object player, string itemName, ref bool __result)
        {
            try
            {
                LogManager.Debug($"ECR Integration triggered: {__instance.GetType().GetProperty("Name")?.GetValue(__instance)}");
                GameObject gameObject = null;

                try
                {
                    Type playerType = player.GetType();
                    PropertyInfo gameobjectProp = playerType.GetProperty("GameObject", BindingFlags.Public | BindingFlags.Instance);
                    if (gameobjectProp != null)
                        gameObject = gameobjectProp.GetValue(player) as GameObject;
                    else
                        LogManager.Error("Could not find 'GameObject' property on Exiled Player object.");
                }
                catch (Exception ex)
                {
                    LogManager.Error($"Failed to get GameObject from Exiled player: {ex}");
                }

                if (!Player.TryGet(gameObject, out Player labplayer))
                {
                    LogManager.Error($"{gameObject} is not a player!");
                    return true;
                }

                if (Utilities.TryGetCustomItemByName(itemName, out ICustomItem customItem))
                {
                    LogManager.Debug($"Giving CustomItem '{customItem.Name}' to {labplayer.Nickname}");
                    new SummonedCustomItem(customItem, labplayer);
                    __result = true;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error($"Error in ECRIntegration.Prefix(): {ex}");
                return true;
            }
        }

        public static void Cleanup()
        {
            AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;
            _isPatched = false;
        }
    }
}