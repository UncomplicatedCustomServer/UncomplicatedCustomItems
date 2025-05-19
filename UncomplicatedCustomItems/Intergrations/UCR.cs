using Exiled.API.Features;
using Exiled.Loader;
using System;
using System.Linq;
using System.Reflection;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.Integrations
{
    internal class UCR
    {
        public static Assembly Assembly => Loader.Plugins.FirstOrDefault(p => p.Name is "UncomplicatedCustomRoles")?.Assembly;

        public static Type CustomRole => Assembly?.GetType("UncomplicatedCustomRoles.API.Features.CustomRole");

        public static Type SummonedCustomRole => Assembly?.GetType("UncomplicatedCustomRoles.API.Features.SummonedCustomRole");

        public static bool Available => CustomRole is not null && SummonedCustomRole is not null;

        public static bool TryGetCustomRole(int id, out object customRole)
        {
            customRole = null;

            if (!Available)
            {
                LogManager.Silent($"{CustomRole} or {SummonedCustomRole} is not found. Aborting UCR integration...");
                return false;
            }
               

            LogManager.Silent($"UCR found, trying check if the role {id} exists...");

            try
            {
                MethodInfo TryGetCustomRole = CustomRole.GetMethod("TryGet", BindingFlags.Public | BindingFlags.Static);
                if (TryGetCustomRole is not null)
                {
                    object[] parameters = new object[] { id, null };
                    bool success = (bool)TryGetCustomRole.Invoke(null, parameters);

                    if (success)
                    {
                        customRole = parameters[1];

                        LogManager.Silent($"returning {customRole}");
                        return customRole is not null;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                LogManager.Error($"{e.Message}\n{e.HResult}");
                return false;
            }
        }

        public static void GiveCustomRole(int id, Player player)
        {
            MethodInfo GiveCustomRole = SummonedCustomRole.GetMethod("Summon", BindingFlags.Public | BindingFlags.Static);

            if (!Available)
                return;

            LogManager.Silent($"UCR role found, trying to give the role {id} to {player}");

            try
            {
                if (TryGetCustomRole(id, out object customRole) && customRole is not null)
                    GiveCustomRole.Invoke(null, new object[] { player, customRole });
            }
            catch (Exception e)
            {
                LogManager.Error($"{e.Message}\n{e.HResult}");
            }
        }
    }
}