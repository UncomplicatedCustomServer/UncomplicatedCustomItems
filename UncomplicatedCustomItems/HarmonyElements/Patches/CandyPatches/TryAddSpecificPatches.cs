using System;
using HarmonyLib;
using InventorySystem.Items.Usables.Scp330;
using UncomplicatedCustomItems.API.Features.CandySerialization;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UncomplicatedCustomItems.API.Interfaces;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.CandyPatches
{
    [HarmonyPatch(typeof(Scp330Bag), nameof(Scp330Bag.TryAddSpecific))]
    public static class TryAddSpecificPatches
    {
        public static CandyKindID LastDesiredCandy = CandyKindID.None;
        public static uint LastCustomItemId = 0;
        public static ICustomItem CustomItem = null;

        [HarmonyPrefix]
        public static void TryAddSpecificPrefix(Scp330Bag __instance, ref CandyKindID kind)
        {
            try
            {
                LogManager.Debug($"{nameof(TryAddSpecificPrefix)}: START - LastCustomItemId: {LastCustomItemId}, CustomItem: {CustomItem?.Name ?? "null"}");
                LogManager.Debug($"{nameof(TryAddSpecificPrefix)}: Original candy type: {kind}");

                if (LastCustomItemId == 0 && CustomItem == null)
                {
                    SelectCustomCandyItem(kind);
                }
                else
                {
                    LogManager.Debug($"Skipping SelectCustomCandyItem - already have LastCustomItemId: {LastCustomItemId}");
                }

                if (LastCustomItemId != 0 && CustomItem != null && CustomItem.CustomData is ICandyData candyData)
                {
                    LogManager.Debug($"{nameof(TryAddSpecificPrefix)}: Using custom candy type {candyData.CandyType} for item {CustomItem.Name}");
                    kind = candyData.CandyType;
                    LastDesiredCandy = candyData.CandyType;
                    return;
                }

                if (LastDesiredCandy != CandyKindID.None && kind != LastDesiredCandy)
                {
                    LogManager.Debug($"{nameof(TryAddSpecificPrefix)}: Replacing {kind} with desired {LastDesiredCandy}");
                    kind = LastDesiredCandy;
                    LastDesiredCandy = CandyKindID.None;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"{nameof(TryAddSpecificPrefix)}:: {ex}");
                ResetCustomCandyState();
            }
        }

        [HarmonyPostfix]
        public static void TryAddSpecificPostfix(Scp330Bag __instance, CandyKindID kind, bool __result)
        {
            try
            {
                if (!__result)
                {
                    ResetCustomCandyState();
                    return;
                }

                LogManager.Debug($"{nameof(TryAddSpecificPostfix)}: Adding {kind} to {__instance.Owner.nicknameSync.DisplayName} Inventory");
                CandySerializationManager.AddCandyToBag(__instance, kind);

                if (LastCustomItemId != 0 && CustomItem != null)
                {
                    CandyKindID finalCandyType = LastDesiredCandy != CandyKindID.None ? LastDesiredCandy : kind;

                    SerializedCandy sc = new()
                    {
                        Id = Guid.NewGuid(),
                        CandyType = finalCandyType,
                        AddedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        BagSerial = __instance.ItemSerial,
                        IsCustom = true,
                        CustomItemId = LastCustomItemId
                    };

                    if (CustomItem.CustomData is ICandyData candyData && candyData is CandyData candy)
                        candy.SerializedCandy = sc;

                    LogManager.Debug($"{nameof(TryAddSpecificPostfix)}: Attaching custom candy id {LastCustomItemId} to bag {__instance.ItemSerial} with candy type {finalCandyType}");
                    CandySerializationManager.ReplaceLastCandyInBag(__instance, sc);
                }

                ResetCustomCandyState();
            }
            catch (Exception ex)
            {
                LogManager.Error($"{nameof(TryAddSpecificPostfix)} error: {ex}");
                ResetCustomCandyState();
            }
        }

        private static void SelectCustomCandyItem(CandyKindID kind)
        {
            LogManager.Debug($"{nameof(SelectCustomCandyItem)}: Attempting to select custom candy for type {kind}");

            foreach (ICustomItem item in API.Features.CustomItem.List)
            {
                if (item.Item is not ItemType.SCP330)
                    continue;

                if (item.CustomData is not ICandyData data)
                    continue;

                if (!item.Spawn.DoSpawn)
                    continue;

                if (kind != data.CandyType)
                    continue;

                float chance = UnityEngine.Random.Range(0f, 101f);
                LogManager.Debug($"{nameof(SelectCustomCandyItem)}: Rolling chance for {item.Name}: {chance}% (need < {data.Chance}%)");

                if (chance >= data.Chance)
                    continue;

                LastCustomItemId = item.Id;
                CustomItem = item;
                LogManager.Debug($"{nameof(SelectCustomCandyItem)}: Selected custom candy {item.Name} (ID: {item.Id}) with candy type {data.CandyType}");
                return;
            }

            LogManager.Debug($"{nameof(SelectCustomCandyItem)}: No custom candy selected for type {kind}");
        }

        private static void ResetCustomCandyState()
        {
            LastDesiredCandy = CandyKindID.None;
            LastCustomItemId = 0;
            CustomItem = null;
        }
    }
}