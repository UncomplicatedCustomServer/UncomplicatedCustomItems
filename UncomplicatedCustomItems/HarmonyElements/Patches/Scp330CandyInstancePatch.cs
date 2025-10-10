using HarmonyLib;
using System.Collections.Generic;
using InventorySystem.Items.Usables.Scp330;
using System.Runtime.CompilerServices;
using UncomplicatedCustomItems.API.Features.CandySerialization;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using System.Linq;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Features.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch]
    public static class Scp330CandyInstancePatch
    {
        /// <summary>
        /// All available <see cref="CandyInstance"/>s by their Id.
        /// </summary>
        public static readonly Dictionary<int, CandyInstance> CandyRegistry = [];

        internal static readonly ConditionalWeakTable<Scp330Bag, List<CandyInstance>> BagInstances = new();

        internal static List<CandyInstance> GetInstances(Scp330Bag bag) =>
            BagInstances.GetOrCreateValue(bag);

        public static bool GiveRandom = true;

        /// <summary>
        /// Predefined CustomItem object.
        /// </summary>
        public static object Item;

        [HarmonyPatch(typeof(Scp330Bag), nameof(Scp330Bag.TryAddSpecific))]
        [HarmonyPostfix]
        public static void TryAddSpecificPostfix(Scp330Bag __instance, CandyKindID kind, bool __result)
        {
            if (!__result)
                return;

            object item = Item ?? (TryGetRandomCandyItem(out object rand) ? rand : null);
            float chance = UnityEngine.Random.Range(0f, 100f);
            CandyInstance inst = null;

            if (item is CustomItem customItem && customItem.CustomData is CandyData candyData && candyData.CandyType == kind)
            {
                if (!GiveRandom)
                {
                    inst = new(kind, item);
                    GetInstances(__instance).Add(inst);
                    CandyRegistry[inst.Id] = inst;
                    LogManager.Debug($"Instanced Candy {inst.Id} has been created with {inst.CustomItem.Name}");
                }
                else if (chance <= candyData.Chance)
                {
                    LogManager.Debug($"Chance: {chance}");
                    inst = new(kind, item);
                    GetInstances(__instance).Add(inst);
                    CandyRegistry[inst.Id] = inst;
                    LogManager.Debug($"Instanced Candy {inst.Id} has been created with {inst.CustomItem.Name}");
                }
            }
            else if (item is CustomCandy customCandy && customCandy.CandyType == kind)
            {
                if (!GiveRandom)
                {
                    inst = new(kind, item);
                    GetInstances(__instance).Add(inst);
                    CandyRegistry[inst.Id] = inst;
                    LogManager.Debug($"Instanced Candy {inst.Id} has been created with {inst.APICustomItem.Name}");
                }
                else if (chance <= customCandy.Chance)
                {
                    LogManager.Debug($"Chance: {chance}");
                    inst = new(kind, item);
                    GetInstances(__instance).Add(inst);
                    CandyRegistry[inst.Id] = inst;
                    LogManager.Debug($"Instanced Candy {inst.Id} has been created with {inst.APICustomItem.Name}");
                }
            }
            else
                LogManager.Debug($"Candy isnt a CustomItem");

            Item = null;
            GiveRandom = true;
        }

        [HarmonyPatch(typeof(Scp330Bag), nameof(Scp330Bag.TryRemove))]
        [HarmonyPostfix]
        public static void TryRemovePostfix(Scp330Bag __instance, int index, CandyKindID __result)
        {
            if (__result != CandyKindID.None)
            {
                List<CandyInstance> list = GetInstances(__instance);
                if (index >= 0 && index < list.Count)
                {
                    CandyInstance inst = list[index];

                    if (inst.CustomItem != null)
                        LogManager.Debug($"Instanced Candy {inst.Id} has been destroyed removing {inst.CustomItem.Name}");
                    if (inst.APICustomItem != null)
                        LogManager.Debug($"Instanced Candy {inst.Id} has been destroyed removing {inst.APICustomItem.Name}");

                    list.RemoveAt(index);
                    CandyRegistry.Remove(inst.Id);
                }
            }
        }

        public static bool TryGetCandyInstances(Scp330Bag bag, out List<CandyInstance> instances)
        {
            instances = GetInstances(bag);
            return instances != null && instances.Count > 0;
        }

        /// <summary>
        /// Attempts to retrieve a randomly selected candy item
        /// the <paramref name="item"/> output parameter.
        /// </summary>
        /// <param name="item">
        /// When this method returns <c>true</c>, contains the randomly selected
        /// candy item; otherwise <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if a random candy item was successfully retrieved;
        /// otherwise, <c>false</c>.
        /// </returns>
        public static bool TryGetRandomCandyItem(out object item)
        {
            if (GetRandomCandyItem() != null)
            {
                item = GetRandomCandyItem();
                return true;
            }
            else
            {
                item = null;
                return false;
            }
        }

#nullable enable

        /// <summary>
        /// Retrieves a randomly selected candy item
        /// </summary>
        /// <returns>
        /// A randomly chosen candy item object if available; otherwise <c>null</c>.
        /// </returns>
        public static object? GetRandomCandyItem()
        {
            List<APICustomItem> apiItems = APICustomItem.List.Where(A => A is CustomCandy).ToList();
            List<CustomItem> items = CustomItem.List.ConvertAll(x => (CustomItem)x).Where(I => I.CustomItemType is CustomItemType.Candy).ToList();
            List<object> combined = [];
            combined.AddRange(apiItems);
            combined.AddRange(items);

            return combined.RandomItem();
        }

#nullable disable

        public static bool TryResolveCustomCandy(Scp330Bag bag, int index, out CustomCandy customCandy, out CandyInstance instance)
        {
            customCandy = null;
            instance = null;

            if (TryGetCandyInstances(bag, out List<CandyInstance> instances) && index >= 0 && index < instances.Count)
            {
                CandyInstance inst = instances[index];
                if (inst.APICustomItem != null && APICustomItem.CustomItems.TryGetValue(inst.APICustomItem.Id, out APICustomItem baseItem) && baseItem is CustomCandy cc)
                {
                    customCandy = cc;
                    instance = inst;
                    return true;
                }
            }

            return false;
        }

        public static CandyInstance GetCandyById(int id)
        {
            CandyRegistry.TryGetValue(id, out CandyInstance inst);
            return inst;
        }
    }
}
