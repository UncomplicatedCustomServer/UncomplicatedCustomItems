using HarmonyLib;
using PlayerRoles;
using System.Collections.Generic;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.Interfaces;
using Exiled.API.Features;
using System.Linq;
using System;
using MEC;
using UncomplicatedCustomItems.API.Features.Helper;
using System.Reflection;
using Exiled.Loader;
using System.Collections;

// This code is probably terrible. But it works ¯\_(ツ)_/¯

namespace UncomplicatedCustomItems.Integration
{
    internal class CommonUtilitiesPatch
    {
        private static Assembly commonUtilitiesAssembly;
        private static MethodInfo _targetMethod;
        private static Type _commonUtilsPlugin;
        private static FieldInfo _instanceProperty;
        private static PropertyInfo _configProperty;
        private static Type _itemChanceType;
        private static PropertyInfo _itemChanceGroupProperty;
        private static PropertyInfo _itemChanceItemProperty;
        private static PropertyInfo _itemChanceChanceProperty;
        private static FieldInfo _commonUtilsRandomField;

        internal static void Initialize()
        {
            commonUtilitiesAssembly = Loader.Plugins.FirstOrDefault(p => p.Name == "Common Utilities")?.Assembly;

            if (commonUtilitiesAssembly == null)
            {
                LogManager.Silent("Common Utilities plugin not found, aborting integration.");
                return;
            }

            _commonUtilsPlugin = commonUtilitiesAssembly.GetType("Common_Utilities.Plugin");

            if (_commonUtilsPlugin == null)
            {
                LogManager.Warn("Could not find Common Utilities Plugin type, aborting integration.");
                return;
            }

            _instanceProperty = _commonUtilsPlugin.GetField("Instance", BindingFlags.Public | BindingFlags.Static);

            if (_instanceProperty == null)
            {
                LogManager.Warn("Could not find Common Utilities Instance field, aborting integration.");
                return;
            }

            _configProperty = _commonUtilsPlugin.GetProperty("Config");

            if (_configProperty == null)
            {
                LogManager.Warn("Could not find Common Utilities Config property, aborting integration.");
                return;
            }

            _itemChanceType = commonUtilitiesAssembly.GetType("Common_Utilities.ConfigObjects.ItemChance");
            if (_itemChanceType == null)
            {
                LogManager.Warn("Could not find Common Utilities ItemChance type, aborting integration.");
                return;
            }

            _itemChanceGroupProperty = _itemChanceType.GetProperty("Group");
            _itemChanceItemProperty = _itemChanceType.GetProperty("ItemName");
            _itemChanceChanceProperty = _itemChanceType.GetProperty("Chance");

            if (_itemChanceGroupProperty == null || _itemChanceItemProperty == null || _itemChanceChanceProperty == null)
            {
                LogManager.Warn("Could not find required properties on Common Utilities ItemChance type, aborting integration.");
                return;
            }

            _commonUtilsRandomField = _commonUtilsPlugin.GetField("Random", BindingFlags.Public | BindingFlags.Static);
            if (_commonUtilsRandomField == null)
            {
                LogManager.Warn("Could not find Common Utilities Random field, will use System.Random as fallback.");
            }

            _targetMethod = commonUtilitiesAssembly.GetType("Common_Utilities.EventHandlers.PlayerHandlers")?.GetMethod("GetStartingInventory");

            if (_targetMethod == null)
            {
                LogManager.Warn("Could not find Common Utilities target method, aborting integration.");
                return;
            }

            Plugin.Instance._harmony.Patch(_targetMethod, prefix: new HarmonyMethod(typeof(CommonUtilitiesPatch).GetMethod(nameof(Prefix), BindingFlags.Public | BindingFlags.Static)));

            LogManager.Silent("Common Utilities integration patch applied successfully.");
        }

        public static void Prefix(RoleTypeId role, Player player)
        {
            if (!Plugin.Instance.Config.EnableCommonUtilitiesIntergration)
            {
                LogManager.Warn("CommonUtilities Intergration disabled aborting CommonUtilities patch.");
                return;
            }

            if (_instanceProperty == null || _configProperty == null || _itemChanceType == null || _itemChanceGroupProperty == null || _itemChanceItemProperty == null || _itemChanceChanceProperty == null)
            {
                LogManager.Error("CommonUtilities integration reflection failed during Initialize, aborting patch execution.");
                return;
            }

            Object pluginInstance = _instanceProperty.GetValue(null);
            if (pluginInstance == null)
            {
                LogManager.Error("Failed to get Common Utilities plugin instance.");
                return;
            }

            Object config = _configProperty.GetValue(pluginInstance);
            if (config == null)
            {
                LogManager.Error("Failed to get Common Utilities config.");
                return;
            }

            PropertyInfo startingInventoriesProperty = config.GetType().GetProperty("StartingInventories");
            if (startingInventoriesProperty == null)
            {
                LogManager.Error("Failed to find StartingInventories property in Common Utilities config.");
                return;
            }
            Object startingInventories = startingInventoriesProperty.GetValue(config);
            if (startingInventories == null)
            {
                 LogManager.Error("StartingInventories property returned null.");
                 return;
            }

            Type startingInventoriesType = startingInventories.GetType();
            PropertyInfo indexerProperty = startingInventoriesType.GetProperty("Item", new[] { typeof(RoleTypeId) });
            if (indexerProperty == null)
            {
                LogManager.Error("Failed to find indexer for StartingInventories.");
                return;
            }

            Object roleInventory = indexerProperty.GetValue(startingInventories, new object[] { role });
            if (roleInventory == null)
            {
                return;
            }

            PropertyInfo usedSlotsProperty = roleInventory.GetType().GetProperty("UsedSlots");
            if (usedSlotsProperty == null)
            {
                LogManager.Error("Failed to find UsedSlots property.");
                return;
            }
            int usedSlots = (int)usedSlotsProperty.GetValue(roleInventory);

            PropertyInfo slotIndexerProperty = roleInventory.GetType().GetProperty("Item", new[] { typeof(int) });
            if (slotIndexerProperty == null)
            {
                LogManager.Error("Failed to find slot indexer.");
                return;
            }

            PropertyInfo additiveProperty = config.GetType().GetProperty("AdditiveProbabilities");
            bool additiveProbabilities = additiveProperty != null && (bool)additiveProperty.GetValue(config);

            for (int i = 0; i < usedSlots; i++)
            {
                Object slot = slotIndexerProperty.GetValue(roleInventory, new object[] { i });
                IEnumerable slotItemsEnumerable = slot as IEnumerable;

                if (slotItemsEnumerable == null)
                {
                    LogManager.Error($"Slot {i} data is not an IEnumerable.");
                    continue;
                }

                var filteredItems = new List<object>();
                foreach (Object itemChanceObj in slotItemsEnumerable)
                {
                    if (itemChanceObj == null || itemChanceObj.GetType() != _itemChanceType)
                    {
                        LogManager.Warn($"Unexpected object type found in slot {i}. Expected {_itemChanceType.FullName}, found {itemChanceObj?.GetType().FullName ?? "null"}. Skipping.");
                        continue;
                    }

                    string group = (string)_itemChanceGroupProperty.GetValue(itemChanceObj);

                    if (player == null ||
                        string.IsNullOrEmpty(group) ||
                        group.Equals("none", StringComparison.OrdinalIgnoreCase) ||
                        (player.Group != null && group == player.Group.BadgeText))
                    {
                        filteredItems.Add(itemChanceObj);
                    }
                }

                if (!filteredItems.Any())
                    continue;

                double rolledChance = CalculateChance(filteredItems, additiveProbabilities);

                foreach (Object itemChanceObj in filteredItems)
                {
                    string item = (string)_itemChanceItemProperty.GetValue(itemChanceObj);
                    double chance = (double)_itemChanceChanceProperty.GetValue(itemChanceObj);

                    if (rolledChance <= chance)
                    {
                        if (Enum.TryParse(item, true, out ItemType _))
                            continue;

                        if (Exiled.CustomItems.API.Features.CustomItem.TryGet(item, out _))
                            continue;

                        if (Utilities.TryGetCustomItemByName(item, out ICustomItem customItem))
                        {
                            LogManager.Silent($"Granting UCI item '{customItem.Name}' to {player.Nickname} via CommonUtilities integration.");
                            Timing.RunCoroutine(ItemAddCoroutine(player, customItem));
                            return;
                        }
                    }

                    if (additiveProbabilities)
                        rolledChance -= chance;
                }
            }
        }

        public static IEnumerator<float> ItemAddCoroutine(Player player, ICustomItem customItem)
        {
            yield return Timing.WaitForSeconds(0.5f);
            if (player?.IsConnected == true)
            {
                 new SummonedCustomItem(customItem, player);
            }
        }

        public static double CalculateChance(IEnumerable<object> itemChances, bool additiveProbabilities)
        {
            Object randomInstanceProvider = _commonUtilsRandomField?.GetValue(null);
            Random random = (randomInstanceProvider as Random) ?? new Random();

            double rolledChance = random.NextDouble();

            if (additiveProbabilities)
            {
                double chanceSum = 0;
                foreach (var itemChanceObj in itemChances)
                {
                    if (itemChanceObj != null && itemChanceObj.GetType() == _itemChanceType)
                    {
                        chanceSum += (double)_itemChanceChanceProperty.GetValue(itemChanceObj);
                    }
                }
                rolledChance *= chanceSum;
            }
            else
            {
                rolledChance *= 100;
            }

            return rolledChance;
        }
    }
}