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
    [HarmonyPatch(typeof(Scp127MagazineModule))]
    public static class MagazineModulePatch
    {
        [HarmonyPatch("ActiveSettings", MethodType.Getter)]
        public static void Postfix(Scp127MagazineModule __instance, ref object __result)
        {
            if (__result is not Scp127MagazineModule.RegenerationSettings original)
                return;
                
            if (APICustomItem.TryGet(__instance.Item.ItemSerial, out var item2) && item2 is CustomSCP127 custom127)
            {
                try
                {
                    Scp127Tier tier = Scp127TierManagerModule.GetTierForItem(__instance.Item);
                    __result = new Scp127MagazineModule.RegenerationSettings
                    {
                        BulletsPerSecond = tier
                        switch
                        {
                            Scp127Tier.Tier1 => custom127.Tier1BulletRegenRate,
                            Scp127Tier.Tier2 => custom127.Tier2BulletRegenRate,
                            Scp127Tier.Tier3 => custom127.Tier3BulletRegenRate,
                            _ => original.BulletsPerSecond
                        },
                        PostFireDelay = tier
                        switch
                        {
                            Scp127Tier.Tier1 => custom127.Tier1BulletRegenPostFireDelay,
                            Scp127Tier.Tier2 => custom127.Tier2BulletRegenPostFireDelay,
                            Scp127Tier.Tier3 => custom127.Tier3BulletRegenPostFireDelay,
                            _ => original.PostFireDelay
                        }
                    };
                }
                catch (Exception ex)
                {
                    LogManager.Error($"{nameof(MagazineModulePatch)}: {ex.Message}\n{ex.StackTrace}");
                }
            }

            if (Utilities.TryGetSummonedCustomItem(__instance.Item.ItemSerial, out var item) && item.CustomItem.CustomItemType == CustomItemType.SCPItem && item.CustomItem.Item == ItemType.GunSCP127)
            {
                try
                {
                    Scp127Tier tier = Scp127TierManagerModule.GetTierForItem(__instance.Item);
                    SCP127Data data = item.CustomItem.CustomData as SCP127Data;
                    __result = new Scp127MagazineModule.RegenerationSettings
                    {
                        BulletsPerSecond = tier
                        switch
                        {
                            Scp127Tier.Tier1 => data.Tier1BulletRegenRate,
                            Scp127Tier.Tier2 => data.Tier2BulletRegenRate,
                            Scp127Tier.Tier3 => data.Tier3BulletRegenRate,
                            _ => original.BulletsPerSecond
                        },
                        PostFireDelay = tier
                        switch
                        {
                            Scp127Tier.Tier1 => data.Tier1BulletRegenPostFireDelay,
                            Scp127Tier.Tier2 => data.Tier2BulletRegenPostFireDelay,
                            Scp127Tier.Tier3 => data.Tier3BulletRegenPostFireDelay,
                            _ => original.PostFireDelay
                        }
                    };
                }
                catch (Exception ex)
                {
                    LogManager.Error($"{nameof(MagazineModulePatch)}: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }
    }
}