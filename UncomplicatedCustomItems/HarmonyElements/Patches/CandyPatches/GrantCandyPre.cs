using HarmonyLib;
using InventorySystem;
using InventorySystem.Items.Usables.Scp330;
using InventorySystem.Items;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Interfaces;
using System;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.CandyPatches
{
    [HarmonyPatch(typeof(InventoryExtensions), nameof(InventoryExtensions.GrantCandy))]
    public static class GrantCandyPre
    {
        [HarmonyPrefix]
        public static bool Prefix(ReferenceHub hub, CandyKindID candyId, ItemAddReason itemAddReason, ref Scp330Bag __result)
        {
            try
            {
                LogManager.Debug($"{nameof(GrantCandyPre)}: Attempting to grant {candyId}");

                foreach (ICustomItem custom in CustomItem.List)
                {
                    if (custom.Item is not ItemType.SCP330)
                        continue;

                    if (custom.CustomData is not ICandyData data)
                        continue;

                    if (!custom.Spawn.DoSpawn)
                        continue;

                    float chance = UnityEngine.Random.Range(0f, 100f);
                    LogManager.Debug($"{nameof(GrantCandyPre)}: custom {custom.Name} chance roll {chance} vs allowed {data.Chance}");
                    if (chance <= data.Chance)
                    {
                        TryAddSpecificPatches.CustomItem = custom;
                        TryAddSpecificPatches.LastCustomItemId = custom.Id;
                        LogManager.Debug($"{nameof(GrantCandyPre)}: Selected custom item id {TryAddSpecificPatches.LastCustomItemId} to attach");
                        break;
                    }
                }

                TryAddSpecificPatches.LastDesiredCandy = candyId;

                bool flag = false;
                if (!Scp330Bag.TryGetBag(hub, out Scp330Bag bag))
                {
                    bag = hub.inventory.ServerAddItem(ItemType.SCP330, itemAddReason, 0) as Scp330Bag;
                    flag = true;
                }

                if (bag == null)
                {
                    LogManager.Debug($"{nameof(GrantCandyPre)}: Failed to get/create bag");
                    __result = null;
                    return false;
                }

                if (flag)
                {
                    LogManager.Debug($"{nameof(GrantCandyPre)}: New bag created, setting candies to [{candyId}]");
                    bag.Candies = [candyId];
                    bag.ServerRefreshBag();
                }
                else
                {
                    LogManager.Debug($"{nameof(GrantCandyPre)}: Existing bag found, attempting to add {candyId}");
                    if (bag.Candies.Count < Scp330Bag.MaxCandies)
                    {
                        bag.Candies.Add(candyId);
                        bag.ServerRefreshBag();
                        LogManager.Debug($"{nameof(GrantCandyPre)}: Successfully added {candyId}");
                    }
                    else
                        LogManager.Debug($"{nameof(GrantCandyPre)}: Bag is full, cannot add candy");
                }

                __result = bag;
                LogManager.Debug($"{nameof(GrantCandyPre)}: Final bag contains: [{string.Join(", ", bag.Candies)}]");
                return false;
            }
            catch (Exception ex)
            {
                LogManager.Error($"[CandySerialization] GrantCandyPre error: {ex}");
                TryAddSpecificPatches.LastCustomItemId = 0;
                return true;
            }
        }
    }
}
