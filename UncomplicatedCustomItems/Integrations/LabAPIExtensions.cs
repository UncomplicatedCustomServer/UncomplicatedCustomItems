using System;
using System.Linq;
using System.Reflection;
using LabApi.Features.Wrappers;
using PlayerRoles;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.Integrations
{
#nullable enable
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
            foreach (Assembly asm in LabApi.Loader.PluginLoader.Dependencies)
            {
                if (asm.GetName().Name?.ToLower().Contains("labapiextensions") is true)
                {
                    LogManager.Silent($"{nameof(LabAPIExtensions)}: Found LabAPIExtensions Dependency!");
                    LabAPIExtension = asm;
                    Found = true;
                    break;
                }
            }

            if (!Found)
            {
                LogManager.Silent($"{nameof(LabAPIExtensions)} was not found!");
                return;
            }

            DisguiseType = LabAPIExtension?.GetType("LabApiExtensions.Extensions.AppearanceExtension");
            if (DisguiseType is null)
            {
                LogManager.Silent("Could not find LabApiExtensions.Extensions.AppearanceExtension type.");
                return;
            }

            Type[] paramTypes = [
                typeof(Player),
                typeof(RoleTypeId),
                typeof(bool),
                typeof(byte)
            ];

            Disguise = DisguiseType.GetMethod("ChangeAppearance", BindingFlags.Public | BindingFlags.Static, null, paramTypes, null);

            if (Disguise is null)
            {
                Disguise = DisguiseType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m =>
                    {
                        if (m.Name != "ChangeAppearance")
                            return false;

                        ParameterInfo[] ps = m.GetParameters();
                        if (ps.Length < 2)
                            return false;

                        bool match0 = ps[0].ParameterType.FullName == typeof(Player).FullName;
                        bool match1 = ps[1].ParameterType == typeof(RoleTypeId) || ps[1].ParameterType.FullName == typeof(RoleTypeId).FullName;
                        return match0 && match1;
                    });
            }

            if (Disguise == null)
                LogManager.Silent("Could not find ChangeAppearance method on AppearanceExtension.");
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
                LogManager.Warn("Disguise method not found.");
                return;
            }

            object[] args = [player, role, false, (byte)0];
            Disguise.Invoke(null, args);
        }
    }
}
