using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using MEC;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features.Items;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Interfaces;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using UnityEngine;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.API
{
    /// <summary>
    /// Handles all the <see cref="Utilities"/> needed for UCI.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Check if a <see cref="ICustomItem"/> is valid and can be registered
        /// </summary>
        /// <param name="item"></param>
        /// <param name="error"></param>
        /// <returns><see cref="bool"/> <see langword="false"/> if there's any problem. Every error will be outputted with <paramref name="error"/></returns>
        public static bool CustomItemValidator(ICustomItem item, out string error)
        {
            if (CustomItem.CustomItems.ContainsKey(item.Id))
            {
                uint OldId = item.Id;
                uint NewId = CustomItem.GetFirstFreeId(1);
                item.Id = NewId;
                LogManager.Warn($"{item.Name} - {OldId} ID is already used asigning new ID...\n{item.Name} new ID is {NewId}");
                CustomItem.Register(item);
            }
            
            switch (item.CustomItemType)
            {
                case CustomItemType.Item:
                    if (item.CustomData is null)
                    {
                    error = $"The item has been flagged as 'Item' but the CustomData class is not 'IData', found '{item.CustomData.GetType().Name}' The CustomData formatting is incorrect. \n Please follow the format found here: https://discord.com/channels/1170301876990914631/1339667038750244979";
                        return false;
                    }

                    break;

                case CustomItemType.Weapon:
                    if (item.CustomData is not IWeaponData)
                    {
                        error = $"The item has been flagged as 'Weapon' but the CustomData class is not 'IWeaponData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/1170301876990914631/1339666579251793960";
                        return false;
                    }

                    if (!item.Item.IsWeapon())
                    {
                        error = $"The item has been flagged as 'Weapon' but the item {item.Item} is not a weapon in the game!";
                        return false;
                    }

                    break;

                case CustomItemType.Keycard:
                    if (item.CustomData is not IKeycardData)
                    {
                        error = $"The item has been flagged as 'Keycard' but the CustomData class is not 'IKeycardData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/1170301876990914631/1339667184435073074";
                        return false;
                    }

                    if (!item.Item.IsKeycard())
                    {
                        error = $"The item has been flagged as 'Keycard' but the item {item.Item} is not a keycard in the game!";
                        return false;
                    }

                    break;

                case CustomItemType.Armor:
                    if (item.CustomData is not IArmorData)
                    {
                        error = $"The item has been flagged as 'Armor' but the CustomData class is not 'IArmorData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/1170301876990914631/1339666435491762197";
                        return false;
                    }

                    if (!item.Item.IsArmor())
                    {
                        error = $"The item has been flagged as 'Armor' but the item {item.Item} is not a armor in the game!";
                        return false;
                    }

                    break;

                case CustomItemType.ExplosiveGrenade:
                    if (item.CustomData is not IExplosiveGrenadeData)
                    {
                        error = $"The item has been flagged as 'ExplosiveGrenade' but the CustomData class is not 'IExplosiveGrenadeData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/1170301876990914631/1339667358398152798";
                        return false;
                    }

                    if (item.Item is not ItemType.GrenadeHE)
                    {
                        error = $"The Item has been flagged as 'ExplosiveGrenade' but the item {item.Item} is not a GrenadeHE";
                        return false;
                    }

                    break;

                case CustomItemType.FlashGrenade:
                    if (item.CustomData is not IFlashGrenadeData)
                    {
                        error = $"The item has been flagged as 'FlashGrenade' but the CustomData class is not 'IFlashGrenadeData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/1170301876990914631/1339666785313755156";
                        return false;
                    }

                    if (item.Item is not ItemType.GrenadeFlash)
                    {
                        error = $"The Item has been flagged as 'FlashGrenade' but the item {item.Item} is not a GrenadeFlash";
                        return false;
                    }

                    break;

                case CustomItemType.Jailbird:
                    if (item.CustomData is not IJailbirdData)
                    {
                        error = $"The item has been flagged as 'Jailbird' but the CustomData class is not 'IJailbirdData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/1170301876990914631/1342257093629182002";
                        return false;
                    }

                    if (item.Item is not ItemType.Jailbird)
                    {
                        error = $"The Item has been flagged as 'Jailbird' but the item {item.Item} is not a Jailbird";
                        return false;
                    }

                    break;

                case CustomItemType.Medikit:
                    if (item.CustomData is not IMedikitData)
                    {
                        error = $"The item has been flagged as 'Medikit' but the CustomData class is not 'IMedikitData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/1170301876990914631/1339667529366372443";
                        return false;
                    }

                    if (item.Item is not ItemType.Medkit)
                    {
                        error = $"The Item has been flagged as 'Medikit' but the item {item.Item} is not a Medikit";
                        return false;
                    }

                    break;

                case CustomItemType.Painkillers:
                    if (item.CustomData is not IPainkillersData)
                    {
                        error = $"The item has been flagged as 'Painkillers' but the CustomData class is not 'IPainkillersData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/1170301876990914631/1354116780846612711";
                        return false;
                    }

                    if (item.Item is not ItemType.Painkillers)
                    {
                        error = $"The Item has been flagged as 'Painkillers' but the item {item.Item} is not a Painkillers";
                        return false;
                    }

                    break;

                case CustomItemType.Adrenaline:
                    if (item.CustomData is not IAdrenalineData)
                    {
                        error = $"The item has been flagged as 'Adrenaline' but the CustomData class is not 'IAdrenalineData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/null";
                        return false;
                    }

                    if (item.Item is not ItemType.Adrenaline)
                    {
                        error = $"The Item has been flagged as 'Adrenaline' but the item {item.Item} is not a Adrenaline";
                        return false;
                    }

                    break;

                case CustomItemType.SCPItem:

                    if (item.Item is not ItemType.SCP500)
                    {
                        error = $"The item has been flagged as 'SCPItem' but the item {item.Item} is not SCP500!";
                    }
                    else if (item.Item is not ItemType.SCP207)
                    {
                        error = $"The item has been flagged as 'SCPItem' but the item {item.Item} is not SCP207!";
                    }
                    else if (item.Item is not ItemType.AntiSCP207)
                    {
                        error = $"The item has been flagged as 'SCPItem' but the item {item.Item} is not AntiSCP207!";
                    }
                    else if (item.Item is not ItemType.SCP018)
                    {
                        error = $"The item has been flagged as 'SCPItem' but the item {item.Item} is not SCP018!";
                    }
                    else if (item.Item is not ItemType.SCP330)
                    {
                        error = $"The item has been flagged as 'SCPItem' but the item {item.Item} is not SCP330!";
                    }
                    else if (item.Item is not ItemType.SCP2176)
                    {
                        error = $"The item has been flagged as 'SCPItem' but the item {item.Item} is not SCP2176!";
                    }
                    else if (item.Item is not ItemType.SCP244a)
                    {
                        error = $"The item has been flagged as 'SCPItem' but the item {item.Item} is not SCP244A!";
                    }
                    else if (item.Item is not ItemType.SCP244b)
                    {
                        error = $"The item has been flagged as 'SCPItem' but the item {item.Item} is not SCP244B!";
                    }
                    else if (item.Item is not ItemType.SCP1853)
                    {
                        error = $"The item has been flagged as 'SCPItem' but the item {item.Item} is not SCP1853!";
                    }
                    else if (item.Item is not ItemType.SCP1576)
                    {
                        error = $"The item has been flagged as 'SCPItem' but the item {item.Item} is not SCP1576!";
                    }
                    else if (item.Item is not ItemType.GunSCP127)
                    {
                        error = $"The item has been flagged as 'SCPItem' but the item {item.Item} is not GunSCP127!";
                    }
                    else
                    {
                        error = $"The item has been flagged as 'SCPItem' but the item {item.Item} is not a modifiable SCP Item!";
                        return false;
                    }

                    break;

                default:
                    error = "Unknown error how did this happen? Anyway please report it on our discord server! D:\nhttps://discord.gg/5StRGu8EJV";
                    return false;
            }

            error = "";
            return true;
        }

        /// <summary>
        /// Check if a <see cref="ICustomItem"/> is valid and can be registered.
        /// Does not return the error as text!
        /// </summary>
        /// <param name="item"></param>
        /// <returns><see cref="bool"/> <see langword="false"/> if there's any problem.</returns>
        public static bool CustomItemValidator(ICustomItem item)
        {
            return CustomItemValidator(item, out _);
        }

        /// <summary>
        /// Parse a <see cref="object"/> as response to a <see cref="Player"/>
        /// </summary>
        /// <param name="player"></param>
        /// <param name="response"></param>
        public static void ParseResponse(Player player, IItemData response)
        {
            if (response.ConsoleMessage is not null && response.ConsoleMessage.Length > 1) // FUCK 1 char messages!
            {
                player.SendConsoleMessage(response.ConsoleMessage, string.Empty);
            }

            if (response.BroadcastMessage.Length > 1 && response.BroadcastDuration > 0)
            {
                player.Broadcast(response.BroadcastDuration, response.BroadcastMessage);
            }

            if (response.HintMessage.Length > 1 && response.HintDuration > 0)
            {
                player.ShowHint(response.HintMessage, response.HintDuration);
            }
        }

        /// <summary>
        /// Try to get a <see cref="SummonedCustomItem"/> by it's <see cref="Item.Serial"/>
        /// </summary>
        /// <param name="serial"></param>
        /// <param name="item"></param>
        /// <returns><see cref="bool"/> <see langword="true"/> if succeeded</returns>
        public static bool TryGetSummonedCustomItem(ushort serial, out SummonedCustomItem item) => SummonedCustomItem.TryGet(serial, out item);

        /// <summary>
        /// Get a <see cref="SummonedCustomItem"/> by it's serial
        /// </summary>
        /// <param name="serial"></param>
        /// <returns><see cref="SummonedCustomItem"/> if succeeded, <c>default</c> if not</returns>
        public static SummonedCustomItem GetSummonedCustomItem(ushort serial) => SummonedCustomItem.Get(serial);

        /// <summary>
        /// Check if an item is a <see cref="SummonedCustomItem"/> by it's <see cref="Item.Serial"/>
        /// </summary>
        /// <param name="serial"></param>
        /// <returns><see cref="bool"/> <see langword="true"/> if it is</returns>
        public static bool IsSummonedCustomItem(ushort serial) => SummonedCustomItem.Get(serial) is not null;

        /// <summary>
        /// Try to get a <see cref="ICustomItem"/> by it's <see cref="ICustomItem.Id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <returns><see cref="bool"/> <see langword="true"/> if the item exists and <paramref name="item"/> is not <see langword="null"/> or <see langword="default"/></returns>
        public static bool TryGetCustomItem(uint id, out ICustomItem item) => CustomItem.CustomItems.TryGetValue(id, out item);

        /// <summary>
        /// Try to get a <see cref="ICustomItem"/> by it's <see cref="ICustomItem.Name"/>
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="item"></param>
        /// <returns><see cref="bool"/> <see langword="true"/> if the item exists and <paramref name="item"/> is not <see langword="null"/> or <see langword="default"/></returns>
        public static bool TryGetCustomItemByName(string Name, out ICustomItem item)
        {
            item = CustomItem.List.FirstOrDefault(i => i.Name == Name);
            return item != null;
        }

        /// <summary>
        /// Get a <see cref="ICustomItem"/> by it's <see cref="ICustomItem.Id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns><see cref="ICustomItem"/> if it exists, otherwhise a <see langword="default"/> will be returned</returns>
        public static ICustomItem GetCustomItem(uint id) => CustomItem.CustomItems[id];

        /// <summary>
        /// Check if the given <see cref="ICustomItem.Id"/> is already registered as a <see cref="ICustomItem"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsCustomItem(uint id) => CustomItem.CustomItems.ContainsKey(id);

        private static Dictionary<LabApi.Features.Wrappers.PedestalLocker, ICustomItem> usedLockers { get; set; } = [];

        /// <summary>
        /// Summon a <see cref="CustomItem"/>
        /// </summary>
        /// <param name="CustomItem"></param>
        internal static void SummonCustomItem(ICustomItem CustomItem)
        {
            ISpawn Spawn = CustomItem.Spawn;

            if (Spawn.PedestalSpawn ?? false)
            {
                if (Spawn.ReplaceExistingPickup)
                {
                    foreach (LabApi.Features.Wrappers.PedestalLocker pedestalLocker in LabApi.Features.Wrappers.PedestalLocker.List)
                    {
                        LabApi.Features.Wrappers.Pickup pedestalLockerPickup = pedestalLocker.GetAllItems().FirstOrDefault();
                        if (pedestalLockerPickup != null && pedestalLockerPickup.Type == CustomItem.Item)
                        {
                            LogManager.Debug($"Removed {pedestalLockerPickup.Type} from {pedestalLockerPickup.Position}");
                            pedestalLocker.RemoveItem(pedestalLockerPickup);
                            LabApi.Features.Wrappers.Pickup pickup = pedestalLocker.AddItem(CustomItem.Item);
                            if (pickup.Type == CustomItem.Item && !usedLockers.ContainsKey(pedestalLocker))
                            {
                                usedLockers.Add(pedestalLocker, CustomItem);
                                LogManager.Debug($"Summoned {CustomItem.Name} to {pedestalLocker.Room.Zone} - {pedestalLocker.Room} - {pedestalLocker.Position}");
                                SummonedCustomItem summonedCustomItem = new(CustomItem, Pickup.Get(pickup.Serial));
                                break;
                            }
                            else if (usedLockers.ContainsKey(pedestalLocker))
                            {
                                LogManager.Debug($"Aborting spawn, locker used already.");
                            }
                        }
                    }
                }
                else 
                {
                    foreach (LabApi.Features.Wrappers.PedestalLocker pedestalLocker in LabApi.Features.Wrappers.PedestalLocker.List)
                    {
                        LabApi.Features.Wrappers.Pickup pedestalLockerPickup = pedestalLocker.GetAllItems().FirstOrDefault();
                        if (pedestalLockerPickup != null && !usedLockers.ContainsKey(pedestalLocker))
                        {
                            usedLockers.Add(pedestalLocker, CustomItem);
                            LogManager.Debug($"Removed {pedestalLockerPickup.Type} from {pedestalLockerPickup.Position}");
                            pedestalLocker.RemoveItem(pedestalLockerPickup);
                            LabApi.Features.Wrappers.Pickup pickup = pedestalLocker.AddItem(CustomItem.Item);
                            LogManager.Debug($"Summoned {CustomItem.Name} to {pedestalLocker.Room.Zone} - {pedestalLocker.Room} - {pedestalLocker.Position}");
                            SummonedCustomItem summonedCustomItem = new(CustomItem, Pickup.Get(pickup.Serial));
                            break;
                        }
                        else if (usedLockers.ContainsKey(pedestalLocker))
                        {
                            LogManager.Debug($"Aborting spawn, locker used already.");
                        }
                    }
                }
            }

            else if (Spawn.Coords.Count() > 0)
            {
                new SummonedCustomItem(CustomItem, Spawn.Coords.RandomItem());
                return;
            }

            else if (Spawn.DynamicSpawn.Count() > 0)
            {
                foreach (DynamicSpawn DynamicSpawn in Spawn.DynamicSpawn)
                {
                    int Chance = Random.Range(0, 100);

                    if (Chance <= DynamicSpawn.Chance)
                    {
                        RoomType Room = DynamicSpawn.Room;
                        if (DynamicSpawn.Coords == Vector3.zero)
                        {
                            if (Spawn.ReplaceExistingPickup)
                            {
                                List<Pickup> FilteredPickups = Pickup.List.Where(pickup => pickup.Room.Type == Room && !IsSummonedCustomItem(pickup.Serial)).ToList();

                                if (Spawn.ForceItem)
                                    FilteredPickups = FilteredPickups.Where(pickup => pickup.Type == CustomItem.Item).ToList();

                                if (FilteredPickups.Count() > 0)
                                    new SummonedCustomItem(CustomItem, FilteredPickups.RandomItem());

                                return;
                            }
                            else
                                new SummonedCustomItem(CustomItem, Exiled.API.Features.Room.Get(Room).Position);
                        }
                        else
                            new SummonedCustomItem(CustomItem, Exiled.API.Features.Room.Get(Room).WorldPosition(DynamicSpawn.Coords));
                    }
                }
            }
            else if (Spawn.Zones.Count() > 0)
            {
                ZoneType Zone = Spawn.Zones.RandomItem();
                if (Spawn.ReplaceExistingPickup)
                {
                    List<Pickup> FilteredPickups = Pickup.List.Where(pickup => pickup.Room != null && pickup.Room.Zone == Zone && !IsSummonedCustomItem(pickup.Serial)).ToList();

                    if (Spawn.ForceItem)
                        FilteredPickups = FilteredPickups.Where(pickup => pickup.Type == CustomItem.Item).ToList();

                    if (FilteredPickups.Count() > 0)
                    {
                        new SummonedCustomItem(CustomItem, FilteredPickups.RandomItem());
                    }
                    return;
                }
                else
                {
                    new SummonedCustomItem(CustomItem, Room.List.Where(room => room.Zone == Zone).ToList().RandomItem().Position);
                }
            }
        }
        
        /// <summary>
        /// Reproduce the SCP:SL <see cref="ItemType.Painkillers"/> healing process but with custom things :)
        /// </summary>
        /// <param name="player"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        internal static IEnumerator<float> PainkillersCoroutine(Player player, IPainkillersData Data)
        {
            float TotalHealed = 0;
            yield return Timing.WaitForSeconds(Data.TimeBeforeStartHealing);
            while (TotalHealed < Data.TotalHealing && player.IsAlive)
            {
                player.Heal(Data.TickHeal);
                TotalHealed += Data.TickHeal;
                yield return Timing.WaitForSeconds(Data.TickTime);
            }
        }
    }
}
