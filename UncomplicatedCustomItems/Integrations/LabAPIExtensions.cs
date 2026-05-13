using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using LabApi.Features.Wrappers;
using PlayerRoles;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.Integrations
{
    internal static class LabAPIExtensions
    {
        private static Assembly? LabAPIExtension;
        private static Type? DisguiseType;
        private static MethodInfo? Disguise;
        private static bool Found;

        public static void Init()
        {
            if (Found)
                return;
                
            GetDependencies();
        }

        public static void GetDependencies()
        {
            Task.Run(() =>
            {
                foreach (Assembly asm in LabApi.Loader.PluginLoader.Dependencies)
                {
                    if (asm.GetName().Name?.ToLower().Contains("labapiextensions") is true)
                    {
                        MainThreadDispatcher.Dispatch(() => LogManager.Silent($"{nameof(LabAPIExtensions)}: Found LabAPIExtensions Dependency!"));
                        LabAPIExtension = asm;
                        Found = true;
                        break;
                    }
                }

                if (!Found)
                {
                    MainThreadDispatcher.Dispatch(() => LogManager.Silent($"{nameof(LabAPIExtensions)} was not found!"));
                    return;
                }

                DisguiseType = LabAPIExtension?.GetType("LabApiExtensions.Managers.FakeRoleManager");
                if (DisguiseType == null)
                {
                    MainThreadDispatcher.Dispatch(() => LogManager.Silent("Failed to find LabApiExtensions.Managers.FakeRoleManager type."));
                    return;
                }

                Type[] paramTypes = [
                    typeof(Player),
                typeof(RoleTypeId)
                ];

                Disguise = AccessTools.Method(DisguiseType, "AddFakeRole", paramTypes);

                if (Disguise == null)
                {
                    Disguise = DisguiseType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .FirstOrDefault(m =>
                        {
                            if (m.Name != "AddFakeRole")
                                return false;

                            ParameterInfo[] ps = m.GetParameters();
                            bool match0 = ps[0].ParameterType.FullName == typeof(Player).FullName;
                            bool match1 = ps[1].ParameterType == typeof(RoleTypeId);

                            return match0 && match1;
                        });
                }

                if (Disguise == null)
                    MainThreadDispatcher.Dispatch(() => LogManager.Silent("Could not find AddFakeRole method in FakeRoleManager."));
            });
        }

        /// <summary>
        /// Disguises the player as a RoleType.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="role"></param>
        public static void DisguisePlayer(this Player player, RoleTypeId role)
        {
            if (Disguise == null)
            {
                LogManager.Warn("Disguise method was not found.");
                return;
            }

            object[] args = [player, role, false, (byte)0];
            Disguise.Invoke(null, args);
        }
    }
}
