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
    internal static class ECRIntegration
    {
        private static bool _isPatched = false;
        private static bool Found;

        public static void Init()
        {
            if (TryPatchECRIntegration())
            {
                LogManager.Silent("ECR Integration patched.");
                Found = true;
                return;
            }

            if (Found)
                LogManager.Silent($"ECR found! :D");
        }

        private static bool TryPatchECRIntegration()
        {
            if (_isPatched)
                return true;

            try
            {
                MethodBase targetMethod = GetTargetMethod();
                if (targetMethod == null)
                {
                    LogManager.Silent("ECR target method not found.");
                    return false;
                }

                MethodInfo prefixMethod = typeof(ECRIntegration).GetMethod(nameof(Prefix), BindingFlags.Static | BindingFlags.Public);

                Plugin.Instance._harmony.Patch(targetMethod, new HarmonyMethod(prefixMethod));
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
                Type customRoleType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetTypesFromAssembly).FirstOrDefault(t => t.FullName == "Exiled.CustomRoles.API.Features.CustomRole");
                if (customRoleType == null)
                    return null;

                Type[] paramTypes = [
                    Type.GetType("Exiled.API.Features.Player, Exiled.API"),
                    typeof(string)
                ];

                MethodInfo method = customRoleType.GetMethod("TryAddItem", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, paramTypes, null);
                return method;
            }
            catch (Exception ex)
            {
                LogManager.Debug($"{nameof(GetTargetMethod)}: {ex.Message}");
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
                LogManager.Error($"{nameof(ECRIntegration)}: {ex}");
                return true;
            }
        }

        public static void Cleanup()
        {
            _isPatched = false;
        }
    }
}