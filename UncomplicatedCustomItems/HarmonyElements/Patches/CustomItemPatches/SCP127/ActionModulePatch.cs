using System;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules.Scp127;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.Manager;
using UncomplicatedCustomItems.API.Features.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(Scp127ActionModule))]
    public static class ActionModulePatch
    {
        [HarmonyPatch("BaseFireRate", MethodType.Getter)]
        public static void Postfix(Scp127ActionModule __instance, ref float __result)
        {
            if (APICustomItem.TryGet(__instance.Item.ItemSerial, out var item2) && item2 is CustomSCP127 custom127)
            {
                __result = Scp127TierManagerModule.GetTierForItem(__instance.Item)
                switch
                {
                    Scp127Tier.Tier1 => custom127.Tier1BulletFireRate,
                    Scp127Tier.Tier2 => custom127.Tier2BulletFireRate,
                    Scp127Tier.Tier3 => custom127.Tier3BulletFireRate,
                    _ => __result
                };
                return;
            }

            if (Utilities.TryGetSummonedCustomItem(__instance.Item.ItemSerial, out var item) && item?.CustomItem.CustomItemType == CustomItemType.SCPItem && item?.Item?.Type == ItemType.GunSCP127)
            {
                try
                {
                    if (item.CustomItem.CustomData is SCP127Data data)

                        __result = Scp127TierManagerModule.GetTierForItem(__instance.Item)
                        switch
                        {
                            Scp127Tier.Tier1 => data.Tier1BulletFireRate,
                            Scp127Tier.Tier2 => data.Tier2BulletFireRate,
                            Scp127Tier.Tier3 => data.Tier3BulletFireRate,
                            _ => __result
                        };
                }
                catch (Exception ex)
                {
                    LogManager.Error($"{nameof(ActionModulePatch)}: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }
    }
}