using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.Scp914Events;
using LabApi.Events.Handlers;
using Scp914;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.Events;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class Craftable : CustomModuleBase
    {
        public override string Name => "Craftable";
        public override List<string> RequiredArguments =>
        [
            "KnobSetting",
            "OriginalItem",
            "Chance",
        ];

        public Scp914KnobSetting? KnobSetting { get; set; }
        public ItemType? OriginalItem { get; set; }
        public float? Chance { get; set; }

        public override void OnAdded(SummonedCustomItem item)
        {
            base.OnAdded(item);
            foreach (Dictionary<object, object> args in Arguments)
            {
                if (!args.TryGetValue<Scp914KnobSetting>("KnobSetting", out var knobSetting))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} KnobSetting is not a valid enum value! {string.Join(", ", Enum.GetNames(typeof(Scp914KnobSetting)))}");
                    return;
                }

                if (!args.TryGetValue<ItemType>("OriginalItem", out var originalItem))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} OriginalItem is not a valid enum value! {string.Join(", ", Enum.GetNames(typeof(Scp914KnobSetting)))}!");
                    return;
                }

                if (!args.TryGetValue<float>("Chance", out var chance))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} Chance is not a valid float!");
                    return;
                }

                KnobSetting = knobSetting;
                OriginalItem = originalItem;
                Chance = chance;
            }
        }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;
                
            if (eventArgs is Scp914ProcessingPickupEventArgs processingPickup)
            {
                LogManager.Debug($"Triggered");
                foreach (CustomItem customItem in Features.CustomItem.List)
                {
                    LogManager.Debug($"{customItem.Name}");
                    if (customItem.TryGetModule<Craftable>(out var data))
                    {
                        LogManager.Debug($"{Name} has Craftable CustomFlag");
                        LogManager.Debug($"Checking settings on {Name}");
                        if (UnityEngine.Random.Range(0f, 101f) >= data.Chance)
                        {
                            LogManager.Debug($"{Name} Passed chance");
                            try
                            {
                                LogManager.Debug($"Checking if {data.OriginalItem} equals {processingPickup.Pickup} and {data.KnobSetting} equals {processingPickup.KnobSetting}");
                                if (processingPickup.Pickup.Type == data.OriginalItem && processingPickup.KnobSetting == data.KnobSetting)
                                {
                                    LogManager.Debug($"Check passed!");
                                    LogManager.Debug($"Spawning {Name} at {processingPickup.Pickup.Position}...");
                                    try
                                    {
                                        processingPickup.Pickup.Destroy();
                                        new SummonedCustomItem(customItem, processingPickup.NewPosition);
                                        LogManager.Debug($"CustomItem created successfully at {processingPickup.NewPosition}");
                                    }
                                    catch (Exception ex)
                                    {
                                        LogManager.Error($"Error during CustomItem creation: {ex.Message}\n{ex.StackTrace}");
                                    }
                                }
                                else
                                    LogManager.Debug($"{processingPickup.KnobSetting} != {data.KnobSetting} or {processingPickup.Pickup} != {data.OriginalItem}");
                            }
                            catch (Exception ex)
                            {
                                LogManager.Error($"Exception: {ex.Message}\n{ex.StackTrace}");
                            }
                        }
                    }
                }
            }

            if (eventArgs is Scp914ProcessingInventoryItemEventArgs processingInventoryItem)
            {
                foreach (CustomItem customItem in Features.CustomItem.List)
                {
                    if (customItem.TryGetModule<Craftable>(out var data))
                    {
                        if (UnityEngine.Random.Range(0f, 101f) >= Chance)
                        {
                            if (processingInventoryItem.Player.CurrentItem.Type == data.OriginalItem && processingInventoryItem.KnobSetting == data.KnobSetting)
                            {
                                processingInventoryItem.Player.RemoveItem(processingInventoryItem.Item);
                                new SummonedCustomItem(customItem, processingInventoryItem.Player);
                                LogManager.Debug($"Gave {Name} to {processingInventoryItem.Player.Nickname}...");
                            }
                        }
                    }
                }
            }
        }

        public override void RegisterEvents()
        {
            base.RegisterEvents();
            Scp914Events.ProcessingPickup += Run;
            Scp914Events.ProcessingInventoryItem += Run;
        }

        public override void UnregisterEvents()
        {
            Scp914Events.ProcessingPickup -= Run;
            Scp914Events.ProcessingInventoryItem -= Run;
        }
    }
}