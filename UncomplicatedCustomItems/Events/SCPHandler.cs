using Exiled.Events.EventArgs.Scp914;
using System;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.Events
{
    internal class SCPHandler
    {
        public static void Register()
        {
            Exiled.Events.Handlers.Scp914.UpgradingPickup += OnPickupUpgrade;
            Exiled.Events.Handlers.Scp914.UpgradingInventoryItem += OnItemUpgrade;
        }

        public static void Unregister() 
        {
            Exiled.Events.Handlers.Scp914.UpgradingPickup -= OnPickupUpgrade;
            Exiled.Events.Handlers.Scp914.UpgradingInventoryItem -= OnItemUpgrade;
        }

        public static void OnPickupUpgrade(UpgradingPickupEventArgs ev)
        {
            foreach (CustomItem customItem in CustomItem.List)
            {
                if (customItem.HasModule(CustomFlags.Craftable))
                {
                    foreach (CraftableSettings craftableSettings in customItem.FlagSettings.CraftableSettings)
                    {
                        if (craftableSettings.OriginalItem == null || craftableSettings.KnobSetting == null || craftableSettings.Chance == null || craftableSettings == null)
                            break;

                        else if (UnityEngine.Random.Range(0, 100) <= craftableSettings.Chance)
                        {
                            try
                            {
                                if (ev.Pickup.Type == craftableSettings.OriginalItem && ev.KnobSetting == craftableSettings.KnobSetting)
                                {
                                    try
                                    {
                                        ev.Pickup.Destroy();
                                        new SummonedCustomItem(customItem, ev.OutputPosition);
                                    }
                                    catch (Exception ex)
                                    {
                                        LogManager.Error($"{nameof(OnPickupUpgrade)}: Error during CustomItem creation: {ex.Message}\n{ex.StackTrace}");
                                    }
                                }
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

        public static void OnItemUpgrade(UpgradingInventoryItemEventArgs ev)
        {
            foreach (CustomItem customItem in CustomItem.List)
            {
                if (customItem.HasModule(CustomFlags.Craftable))
                {
                    foreach (CraftableSettings craftableSettings in customItem.FlagSettings.CraftableSettings)
                    {
                        if (craftableSettings.OriginalItem == null || craftableSettings.KnobSetting == null || craftableSettings.Chance == null || craftableSettings == null)
                            break;

                        else if (UnityEngine.Random.Range(0, 100) <= craftableSettings.Chance)
                        {
                            try
                            {
                                if (ev.Player.CurrentItem.Type == craftableSettings.OriginalItem && ev.KnobSetting == craftableSettings.KnobSetting)
                                {
                                    try
                                    {
                                        ev.Player.RemoveItem(ev.Item);
                                        new SummonedCustomItem(customItem, ev.Player);
                                    }
                                    catch (Exception ex)
                                    {
                                        LogManager.Error($"{nameof(OnItemUpgrade)}: Error during CustomItem creation: {ex.Message}\n{ex.StackTrace}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogManager.Error($"{nameof(OnItemUpgrade)}: Exception: {ex.Message}\n{ex.StackTrace}");
                            }
                        }
                    }
                }
            }
        }
    }
}
