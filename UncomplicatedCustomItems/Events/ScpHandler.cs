using LabApi.Events.Arguments.Scp914Events;
using System;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using Scp914Event = LabApi.Events.Handlers.Scp914Events;

namespace UncomplicatedCustomItems.Events
{
    internal class ScpHandler
    {
        public static void Register()
        {
            Scp914Event.ProcessingPickup += OnPickupUpgrade;
            Scp914Event.ProcessingInventoryItem += OnItemUpgrade;
        }

        public static void Unregister()
        {
            Scp914Event.ProcessingPickup -= OnPickupUpgrade;
            Scp914Event.ProcessingInventoryItem -= OnItemUpgrade;
        }

        public static void OnPickupUpgrade(Scp914ProcessingPickupEventArgs ev)
        {
            if (ev.Pickup.IsCustomItem())
            {
                ev.IsAllowed = false;
                ev.Pickup.Position = ev.NewPosition;
            }

            LogManager.Debug($"{nameof(OnPickupUpgrade)}: Triggered");
            foreach (CustomItem customItem in CustomItem.List)
            {
                LogManager.Debug($"{nameof(OnPickupUpgrade)}: {customItem.Name}");
                if (customItem.HasModule(CustomFlags.Craftable))
                {
                    LogManager.Debug($"{nameof(OnPickupUpgrade)}: {customItem.Name} has Craftable CustomFlag");
                    foreach (CraftableSettings craftableSettings in customItem.FlagSettings.CraftableSettings)
                    {
                        LogManager.Debug($"{nameof(OnPickupUpgrade)}: Checking settings on {customItem.Name}");
                        if (craftableSettings.OriginalItem == null || craftableSettings.KnobSetting == null || craftableSettings.Chance == null)
                        {
                            LogManager.Warn($"{nameof(OnPickupUpgrade)}: {customItem.Name} - {customItem.Id} has OriginalItem, KnobSetting, or chance equal null. Aborting... \n Values: {craftableSettings.OriginalItem} {craftableSettings.KnobSetting} {craftableSettings.Chance}");
                            continue;
                        }
                        else if (UnityEngine.Random.Range(0f, 101f) >= craftableSettings.Chance)
                        {
                            LogManager.Debug($"{nameof(OnPickupUpgrade)}: {customItem.Name} Passed chance");
                            try
                            {
                                LogManager.Debug($"{nameof(OnPickupUpgrade)}: Checking if {craftableSettings.OriginalItem} equals {ev.Pickup} and {craftableSettings.KnobSetting} equals {ev.KnobSetting}");
                                if (ev.Pickup.Type == craftableSettings.OriginalItem && ev.KnobSetting == craftableSettings.KnobSetting)
                                {
                                    LogManager.Debug($"{nameof(OnPickupUpgrade)}: Check passed!");
                                    LogManager.Debug($"{nameof(OnPickupUpgrade)}: Spawning {customItem.Name} at {ev.Pickup.Position}...");
                                    try
                                    {
                                        ev.Pickup.Destroy();
                                        new SummonedCustomItem(customItem, ev.NewPosition);
                                        LogManager.Debug($"{nameof(OnPickupUpgrade)}: CustomItem created successfully at {ev.NewPosition}");
                                    }
                                    catch (Exception ex)
                                    {
                                        LogManager.Error($"{nameof(OnPickupUpgrade)}: Error during CustomItem creation: {ex.Message}\n{ex.StackTrace}");
                                    }
                                }
                                else
                                    LogManager.Debug($"{nameof(OnPickupUpgrade)}: {ev.KnobSetting} != {craftableSettings.KnobSetting} or {ev.Pickup} != {craftableSettings.OriginalItem}");
                            }
                            catch (Exception ex)
                            {
                                LogManager.Error($"{nameof(OnPickupUpgrade)}: Exception: {ex.Message}\n{ex.StackTrace}");
                            }
                        }
                    }
                }
            }
        }

        public static void OnItemUpgrade(Scp914ProcessingInventoryItemEventArgs ev)
        {
            if (ev.Item.IsCustomItem())
                ev.IsAllowed = false;

            foreach (CustomItem customItem in CustomItem.List)
            {
                if (customItem.HasModule(CustomFlags.Craftable))
                {
                    foreach (CraftableSettings craftableSettings in customItem.FlagSettings.CraftableSettings)
                    {
                        if (UnityEngine.Random.Range(0f, 101f) >= craftableSettings.Chance)
                        {
                            if (ev.Player.CurrentItem.Type == craftableSettings.OriginalItem && ev.KnobSetting == craftableSettings.KnobSetting)
                            {
                                ev.Player.RemoveItem(ev.Item);
                                new SummonedCustomItem(customItem, ev.Player);
                                LogManager.Debug($"{nameof(OnItemUpgrade)}: Gave {customItem.Name} to {ev.Player.Nickname}...");
                            }
                        }
                    }
                }
            }
        }
    }
}