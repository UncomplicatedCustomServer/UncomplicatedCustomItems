using Common_Utilities.ConfigObjects;
using HarmonyLib;
using PlayerRoles;
using System.Collections.Generic;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.Interfaces;
using Common_Utilities.EventHandlers;
using Exiled.API.Features;
using System.Linq;
using System;
using MEC;

[HarmonyPatch(typeof(PlayerHandlers), nameof(PlayerHandlers.GetStartingInventory))]
public class CommonUtilitiesPatch
{
    [HarmonyPrefix]
    public static void Prefix(RoleTypeId role, Player player)
    {
        var config = Common_Utilities.Plugin.Instance.Config;

        for (int i = 0; i < config.StartingInventories[role].UsedSlots; i++)
        {
            List<ItemChance> list2 = config.StartingInventories[role][i]
                .Where(x =>
                    player == null ||
                    string.IsNullOrEmpty(x.Group) ||
                    x.Group.Equals("none", StringComparison.OrdinalIgnoreCase) ||
                    x.Group == player.Group.BadgeText)
                .ToList();

            double rolledChance = CalculateChance(list2);

            foreach ((string item, double chance) in list2)
            {
                if (rolledChance <= chance)
                {
                    if (Enum.TryParse(item, true, out ItemType _))
                        continue;

                    if (Exiled.CustomItems.API.Features.CustomItem.TryGet(item, out _))
                        continue;

                    if (Utilities.TryGetCustomItemByName(item, out ICustomItem customItem))
                    {
                        var instance = new CommonUtilitiesPatch();
                        Timing.RunCoroutine(instance.ItemAddCoroutine(player, customItem));
                        return;
                    }
                }

                if (config.AdditiveProbabilities)
                    rolledChance -= chance;
            }
        }
    }
    public IEnumerator<float> ItemAddCoroutine(Player player, ICustomItem CustomItem)
    {
        yield return Timing.WaitForSeconds(1);
        new SummonedCustomItem(CustomItem, player);
    }

    public static double CalculateChance(List<ItemChance> itemChances)
    {
        double rolledChance = Common_Utilities.Plugin.Random.NextDouble();
        if (Common_Utilities.Plugin.Instance.Config.AdditiveProbabilities)
            rolledChance *= itemChances.Sum(x => x.Chance);
        else
            rolledChance *= 100;
        return rolledChance;
    }
}