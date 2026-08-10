using System;
using LabApi.Events.Arguments.Scp914Events;
using LabApi.Events.Handlers;
using Scp914;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Manager;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class Craftable : CustomModuleBase
    {
        public override string Name => "Craftable";

        public Scp914KnobSetting KnobSetting { get; set; }
        public ItemType OriginalItem { get; set; }
        public float Chance { get; set; } = 100f;

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;
                
            if (eventArgs is Scp914ProcessingPickupEventArgs processingPickup)
            {
                if (processingPickup.Pickup == null)
                    return;

                LogManager.Debug($"Triggered Craftable pickup processing");
                foreach (CustomItem customItem in Features.CustomItem.CustomItems.Values)
                {
                    if (customItem.TryGetModule<Craftable>(out var data) && data != null)
                    {
                        if (UnityEngine.Random.Range(0f, 100f) < data.Chance)
                        {
                            try
                            {
                                if (processingPickup.Pickup.Type == data.OriginalItem && processingPickup.KnobSetting == data.KnobSetting)
                                {
                                    processingPickup.Pickup.Destroy();
                                    new SummonedCustomItem(customItem, processingPickup.NewPosition);
                                    LogManager.Debug($"CustomItem created successfully at {processingPickup.NewPosition}");
                                }
                            }
                            catch (Exception ex)
                            {
                                LogManager.Error($"Error during Craftable CustomItem creation: {ex.Message}\n{ex.StackTrace}");
                            }
                        }
                    }
                }
            }

            if (eventArgs is Scp914ProcessingInventoryItemEventArgs processingInventoryItem)
            {
                if (processingInventoryItem.Item == null)
                    return;

                foreach (CustomItem customItem in Features.CustomItem.CustomItems.Values)
                {
                    if (customItem.TryGetModule<Craftable>(out var data) && data != null)
                    {
                        if (UnityEngine.Random.Range(0f, 100f) < data.Chance)
                        {
                            if (processingInventoryItem.Item.Type == data.OriginalItem && processingInventoryItem.KnobSetting == data.KnobSetting)
                            {
                                processingInventoryItem.Player.RemoveItem(processingInventoryItem.Item);
                                new SummonedCustomItem(customItem, processingInventoryItem.Player);
                                LogManager.Debug($"Gave {customItem.Name} to {processingInventoryItem.Player.Nickname}...");
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