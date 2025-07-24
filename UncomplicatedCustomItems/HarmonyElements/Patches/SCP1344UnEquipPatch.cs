using HarmonyLib;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.Usables.Scp1344;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(ItemBase), nameof(ItemBase.ServerDropItem), typeof(bool))]
    public static class SCP1344UnEquipPatch
    {
        public static void Postfix(ItemBase __instance, bool spawn, ref ItemPickupBase __result)
        {
            if (__result == null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var summonedCustomItem))
                return;

            if (summonedCustomItem.CustomItem.CustomItemType is CustomItemType.SCPItem && summonedCustomItem.CustomItem.Item is ItemType.SCP1344)
            {
                ISCP1344Data data = summonedCustomItem.CustomItem.CustomData as ISCP1344Data;
                if (data.AllowUnequip)
                {
                    if (!Item.TryGet(__instance, out Item item))
                        return;
                    if (item is not LabApi.Features.Wrappers.Scp1344Item scp1344)
                    {
                        LogManager.Debug($"{nameof(Postfix)}: {item.Type} is not SCP1344!");
                        return;
                    }
                    if (scp1344.Status != Scp1344Status.Deactivating)
                        return;

                    __result.PreviousOwner.Hub.inventory.ServerAddItem(__result.ItemId.TypeId, ItemAddReason.AdminCommand, __result.Info.Serial, __result);
                    __result.DestroySelf();
                }
            }
        }
    }
}
