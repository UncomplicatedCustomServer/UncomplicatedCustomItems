using HarmonyLib;
using InventorySystem.Items;
using InventorySystem.Items.Usables;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UncomplicatedCustomItems.API;

namespace UncomplicatedCustomItems.Patches
{
    [HarmonyPatch(typeof(PlayerEffectsController), nameof(PlayerEffectsController.UseMedicalItem))]
    public class PlayerEffectsControllerPatch
    {
        private static bool Prefix(PlayerEffectsController __instance, ItemBase item)
        {
            var customItem = Utilities.GetCustomItem(itemBase.ItemSerial);

            if (customItem is null)
            {
                return true;
            }

            if (itemBase.ItemTypeId is not ItemType.Painkillers or ItemType.Medkit)
            {
                return true;
            }

            if (itemBase.ItemTypeId is ItemType.Medkit)
            {
                //65 is amount of medkit health
                itemBase.Owner.playerStats.GetModule<HealthStat>().CurValue -= 65;
            }
            else
            {
                var handler = UsableItemsController.GetHandler(itemBase.Owner);

                handler.ActiveRegenerations.RemoveAt(handler.ActiveRegenerations.Count - 1);
            }

            return true;
        }
    }
}
