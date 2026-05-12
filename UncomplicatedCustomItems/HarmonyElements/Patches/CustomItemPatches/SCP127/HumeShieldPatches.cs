using System;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules.Scp127;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Features.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(Scp127HumeModule))]
    public static class HumeShieldPatches
    {
        [HarmonyPatch("HsMax", MethodType.Getter)]
        public static void Postfix(Scp127HumeModule __instance, ref float __result)
        {
            if (__instance != null || __instance?.Item != null)
                return;

            if (APICustomItem.TryGet(__instance?.Item.ItemSerial ?? 0, out var item2) && item2 is CustomSCP127 custom127)
            {
                try
                {
                    if (custom127.GiveHumeShield)
                    {
                        __result = Scp127TierManagerModule.GetTierForItem(__instance?.Item)
                        switch
                        {
                            Scp127Tier.Tier1 => custom127.Tier1HumeShieldAmount,
                            Scp127Tier.Tier2 => custom127.Tier2HumeShieldAmount,
                            Scp127Tier.Tier3 => custom127.Tier3HumeShieldAmount,
                            _ => __result
                        };
                    }
                    else
                        __result = 0f;
                }
                catch (Exception ex)
                {
                    LogManager.Error($"{nameof(HumeShieldPatches)}: {ex.Message}\n{ex.StackTrace}");
                }
            }

            if (Utilities.TryGetSummonedCustomItem(__instance?.Item.ItemSerial ?? 0, out var itemhume) && itemhume?.CustomItem.CustomItemType == CustomItemType.SCPItem && itemhume?.Item?.Type == ItemType.GunSCP127)
            {
                try
                {
                    if (itemhume.CustomItem.CustomData is not SCP127Data data)
                        return;

                    if (data.GiveHumeShield)
                    {
                        __result = Scp127TierManagerModule.GetTierForItem(__instance?.Item)
                        switch
                        {
                            Scp127Tier.Tier1 => data.Tier1HumeShieldAmount,
                            Scp127Tier.Tier2 => data.Tier2HumeShieldAmount,
                            Scp127Tier.Tier3 => data.Tier3HumeShieldAmount,
                            _ => __result
                        };
                    }
                    else
                        __result = 0f;
                }
                catch (Exception ex)
                {
                    LogManager.Error($"{nameof(HumeShieldPatches)}: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }
    }
}