using System;
using HarmonyLib;
using InventorySystem.Items.Usables.Scp330;
using UncomplicatedCustomItems.API.Features.CandySerialization;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
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
        public static object CustomItemobj = null;

        [HarmonyPrefix]
        public static void TryAddSpecificPrefix(Scp330Bag __instance, ref CandyKindID kind)
        {
            try
            {
                LogManager.Debug($"{nameof(TryAddSpecificPrefix)}: START - LastCustomItemId: {LastCustomItemId}, CustomItemobj: {GetItemName(CustomItemobj)}");
                LogManager.Debug($"{nameof(TryAddSpecificPrefix)}: Original candy type: {kind}");

                if (LastCustomItemId == 0 && CustomItemobj == null)
                {
                    SelectCustomCandyItem(kind);
                }
                else
                {
                    LogManager.Debug($"Skipping SelectCustomCandyItem - already have LastCustomItemId: {LastCustomItemId}");
                }

                var candyType = GetCandyTypeFromItem(CustomItemobj);
                if (candyType.HasValue)
                {
                    LogManager.Debug($"{nameof(TryAddSpecificPrefix)}: Using custom candy type {candyType.Value} for item {GetItemName(CustomItemobj)}");
                    kind = candyType.Value;
                    LastDesiredCandy = candyType.Value;
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

                if (LastCustomItemId != 0 && CustomItemobj != null)
                {
                    ProcessCustomCandy(__instance, kind);
                }

                ResetCustomCandyState();
            }
            catch (Exception ex)
            {
                LogManager.Error($"{nameof(TryAddSpecificPostfix)} error: {ex}");
                ResetCustomCandyState();
            }
        }

        private static void ProcessCustomCandy(Scp330Bag bag, CandyKindID kind)
        {
            CandyKindID finalCandyType = LastDesiredCandy != CandyKindID.None ? LastDesiredCandy : kind;

            SerializedCandy sc = new()
            {
                Id = Guid.NewGuid(),
                CandyType = finalCandyType,
                AddedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                BagSerial = bag.ItemSerial,
                IsCustom = true,
                CustomItemId = LastCustomItemId
            };

            switch (CustomItemobj)
            {
                case ICustomItem customItem when customItem.CustomData is ICandyData candyData && candyData is CandyData candy:
                    candy.SerializedCandy = sc;
                    break;
                case APICustomItem baseCustomItem when baseCustomItem is CustomCandy customCandy:
                    customCandy.SerializedCandy = sc;
                    break;
            }

            LogManager.Debug($"{nameof(ProcessCustomCandy)}: Attaching custom candy id {LastCustomItemId} to bag {bag.ItemSerial} with candy type {finalCandyType}");
            CandySerializationManager.ReplaceLastCandyInBag(bag, sc);
        }

        private static CandyKindID? GetCandyTypeFromItem(object item)
        {
            return item switch
            {
                ICustomItem customItem when customItem.CustomData is ICandyData candyData => candyData.CandyType,
                APICustomItem baseCustomItem when baseCustomItem is CustomCandy customCandy => customCandy.CandyType,
                _ => null
            };
        }

        private static string GetItemName(object item)
        {
            return item switch
            {
                ICustomItem customItem => customItem.Name,
                APICustomItem baseCustomItem => baseCustomItem.Name,
                _ => "null"
            };
        }

        private static void SelectCustomCandyItem(CandyKindID kind)
        {
            LogManager.Debug($"{nameof(SelectCustomCandyItem)}: Attempting to select custom candy for type {kind}");

            foreach (APICustomItem baseCustomItem in APICustomItem.List)
            {
                if (baseCustomItem.Item is not ItemType.SCP330)
                    continue;

                if (baseCustomItem is not CustomCandy customCandy)
                    continue;

                if (!baseCustomItem.Spawn)
                    continue;

                if (kind != customCandy.CandyType)
                    continue;

                float chance = UnityEngine.Random.Range(0f, 101f);
                LogManager.Debug($"{nameof(SelectCustomCandyItem)}: Rolling chance for {baseCustomItem.Name}: {chance}% (need < {customCandy.Chance}%)");

                if (chance >= customCandy.Chance)
                    continue;

                LastCustomItemId = baseCustomItem.Id;
                CustomItemobj = baseCustomItem;
                LogManager.Debug($"{nameof(SelectCustomCandyItem)}: Selected custom candy {baseCustomItem.Name} (ID: {baseCustomItem.Id}) with candy type {customCandy.CandyType}");
                return;
            }

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
                CustomItemobj = item;
                LogManager.Debug($"{nameof(SelectCustomCandyItem)}: Selected custom candy {item.Name} (ID: {item.Id}) with candy type {data.CandyType}");
                return;
            }

            LogManager.Debug($"{nameof(SelectCustomCandyItem)}: No custom candy selected for type {kind}");
        }

        private static void ResetCustomCandyState()
        {
            LastDesiredCandy = CandyKindID.None;
            LastCustomItemId = 0;
            CustomItemobj = null;
        }
    }
}