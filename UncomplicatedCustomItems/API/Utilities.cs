using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.CustomRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Elements;
using UncomplicatedCustomItems.Elements.SpecificData;
using UncomplicatedCustomItems.Interfaces;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using UncomplicatedCustomItems.Manager;
using ExiledItem = Exiled.API.Features.Items.Item;

namespace UncomplicatedCustomItems.API
{
    public static class Utilities
    {
        /// <summary>
        /// A more easy-to-use dictionary to store every registered <see cref="ICustomItem"/>
        /// </summary>
        internal static Dictionary<int, ICustomItem> CustomItems { get; set; } = new();

        /// <summary>
        /// Get a list of every <see cref="ICustomItem"/> registered.
        /// </summary>
        public static List<ICustomItem> List => CustomItems.Values.ToList();

        /// <summary>
        /// Gets a list of not loaded custom items.
        /// The data is the Id, the item path, the error type and the error name
        /// </summary>
        internal static List<Tuple<string, string, string, string>> NotLoadedItems { get; } = new();

        internal static List<SummonedCustomItem> SummonedItems = new();

        /// <summary>
        /// Check if a <see cref="ICustomItem"/> is valid and can be registered
        /// </summary>
        /// <param name="item"></param>
        /// <param name="error"></param>
        /// <returns><see cref="false"/> if there's any problem. Every error will be outputted with <paramref name="error"/></returns>
        public static bool CustomItemValidator(ICustomItem item, out string error)
        {
            if (Manager.Items.ContainsKey(item.Id))
            {
                error = $"({item}) is using a id thats already used by another CustomItem ({item.Id})! Assigning new Id. ({item.Id++})";
                item.Id++;
            }

            switch (item.CustomItemType)
            {
                case CustomItemType.Item:
                    if (item.CustomData is not IItemData)
                    {
                        error = $"The item has been flagged as 'Item' but the CustomData class is not 'IItemData', found '{item.CustomData.GetType().Name}'";
                        return false;
                    }

                    break;

                case CustomItemType.Weapon:
                    if (item.CustomData is not IWeaponData)
                    {
                        error = $"The item has been flagged as 'Weapon' but the CustomData class is not 'IWeaponData', found '{item.CustomData.GetType().Name}'";
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
                        error = $"The item has been flagged as 'Keycard' but the CustomData class is not 'IKeycardData', found '{item.CustomData.GetType().Name}'";
                        return false;
                    }

                    if (!item.Item.IsKeycard())
                    {
                        error = $"The item has been flagged as 'Keycard' but the item {item.Item} is not a keycard in the game!";
                        return false;
                    }

                    break;

                case CustomItemType.Throwable:
                    if (item.CustomData is not IThrowableData)
                    {
                        error = $"The item has been flagged as 'throwable' but the CustomData class is not 'IThrowableData', found '{item.CustomData.GetType().Name}'";
                        return false;
                    }

                    if (!item.Item.IsThrowable())
                    {
                        error = $"The item has been flagged as 'throwable' but the item {item.Item} is not a throwable object in the game!";
                        return false;
                    }

                    break;


                case CustomItemType.Armor:
                    if (item.CustomData is not IArmorData)
                    {
                        error = $"The item has been flagged as 'Armor' but the CustomData class is not 'IArmorData', found '{item.CustomData.GetType().Name}'";
                        return false;
                    }

                    if (!item.Item.IsArmor())
                    {
                        error = $"The item has been flagged as 'Armor' but the item {item.Item} is not a armor in the game!";
                        return false;
                    }

                    break;

                default:
                    error = "Unknown error? Uhm please report it on our discord server!";
                    return false;
            }

            error = "";
            return true;
        }

        internal static Dictionary<uint, ICustomItem> Items = new();
        public static void Register(ICustomItem item)
        {
            if (!Utilities.CustomItemValidator(item, out string error))
            {
                Log.Warn($"Unable to register the ICustomItem with the Id {item.Id} and name '{item.Name}':\n{error}\nError code: 0x029");
                return;
            }
            Items.Add(item.Id, item);
            Log.Info($"Successfully registered ICustomItem '{item.Name}' (Id: {item.Id}) into the plugin!");
        }

        /// <summary>
        /// Unregister a <see cref="ICustomItem"/> from the plugin by it's class
        /// </summary>
        /// <param name="item"></param>
        public static void Unregister(ICustomItem item)
        {
            if (Items.ContainsKey(item.Id))
            {
                Items.Remove(item.Id);
            }
        }

        /// <summary>
        /// Unregister a <see cref="ICustomItem"/> from the plugin by it's Id
        /// </summary>
        /// <param name="item"></param>
        public static void Unregister(uint item)
        {
            if (Items.ContainsKey(item))
            {
                Items.Remove(item);
            }
        }

        /// <summary>
        /// Get the first free id to register a new custom item
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static uint GetFirstFreeID(uint Id)
        {
            while (Manager.Items.ContainsKey(Id))
                Id++;

            return Id;
        }

        /// <summary>
        /// Check if a <see cref="ICustomItem"/> is valid and can be registered.
        /// Does not return the error as text!
        /// </summary>
        /// <param name="item"></param>
        /// <returns><see cref="false"/> if there's any problem.</returns>
        public static bool CustomItemValidator(ICustomItem item)
        {
            return CustomItemValidator(item, out _);
        }

        /// <summary>
        /// Parse a <see cref="IResponse"/> object as response to a player
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
        /// Try to get a <see cref="SummonedCustomItem"/> by it's serial
        /// </summary>
        /// <param name="serial"></param>
        /// <param name="item"></param>
        /// <returns><see cref="true"/> if succeeded</returns>
        public static bool TryGetSummonedCustomItem(ushort serial, out SummonedCustomItem item)
        {
            item = Manager.SummonedItems.Where(item => item.Serial == serial).FirstOrDefault();
            if (item == default)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get a <see cref="SummonedCustomItem"/> by it's serial
        /// </summary>
        /// <param name="serial"></param>
        /// <returns><see cref="SummonedCustomItem"/> if succeeded, <see cref="default"/> if not</returns>
        public static SummonedCustomItem GetSummonedCustomItem(ushort serial)
        {
            if (!TryGetSummonedCustomItem(serial, out SummonedCustomItem Item))
            {
                return default;
            }
            return Item;
        }

        /// <summary>
        /// Check if an item is a <see cref="SummonedCustomItem"/> by it's serial
        /// </summary>
        /// <param name="serial"></param>
        /// <returns><see cref="true"/> if it is</returns>
        public static bool IsSummonedCustomItem(ushort serial)
        {
            return Manager.SummonedItems.Where(item => item.Serial == serial).Count() > 0;
        }

        /// <summary>
        /// Try to get a <see cref="ICustomItem"/> by it's Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <returns><see cref="true"/> if the item exists and <paramref name="item"/> is not <see cref="null"/> or <see cref="default"/></returns>
        public static bool TryGetCustomItem(uint id, out ICustomItem item)
        {
            //foxworn is furry, hehehehehehehehehehe
            if (Manager.Items.TryGetValue(id, out item))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get a <see cref="ICustomItem"/> by it's Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns><see cref="ICustomItem"/> if it exists, otherwhise a <see cref="default"/> will be returned</returns>
        public static ICustomItem GetCustomItem(uint id)
        {
            TryGetCustomItem(id, out ICustomItem Item);
            return Item;
        }

        /// <summary>
        /// Check if the given Id is already registered as a <see cref="ICustomItem"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsCustomItem(uint id)
        {
            return Manager.Items.ContainsKey(id);
        }

        internal static void SummonCustomItem(ICustomItem CustomItem)
        {
            ISpawn Spawn = CustomItem.Spawn;

            if (Spawn.Coords.Count() > 0)
            {
                SummonedCustomItem.Summon(CustomItem, Spawn.Coords.RandomItem());
                return;
            }

            else if (Spawn.Rooms.Count() > 0)
            {
                RoomType Room = Spawn.Rooms.RandomItem();
                if (Spawn.ReplaceExistingPickup)
                {
                    List<Pickup> FilteredPickups = Pickup.List.Where(pickup => pickup.Room.Type == Room && !IsSummonedCustomItem(pickup.Serial)).ToList();

                    if (Spawn.ForceItem)
                    {
                        FilteredPickups = FilteredPickups.Where(pickup => pickup.Type == CustomItem.Item).ToList();
                    }

                    if (FilteredPickups.Count() > 0)
                    {
                        Pickup Pickup = FilteredPickups.RandomItem();
                        SummonedCustomItem.Summon(CustomItem, Pickup.Position, Pickup.Rotation);
                        Pickup.Destroy();
                    }
                    return;
                }
                else
                {
                    SummonedCustomItem.Summon(CustomItem, Exiled.API.Features.Room.Get(Room).Position);
                }
            }
            else if (Spawn.Zones.Count() > 0)
            {
                ZoneType Zone = Spawn.Zones.RandomItem();
                if (Spawn.ReplaceExistingPickup)
                {
                    List<Pickup> FilteredPickups = Pickup.List.Where(pickup => pickup.Room.Zone == Zone && !IsSummonedCustomItem(pickup.Serial)).ToList();

                    if (Spawn.ForceItem)
                    {
                        FilteredPickups = FilteredPickups.Where(pickup => pickup.Type == CustomItem.Item).ToList();
                    }

                    if (FilteredPickups.Count() > 0)
                    {
                        Pickup Pickup = FilteredPickups.RandomItem();
                        SummonedCustomItem.Summon(CustomItem, Pickup.Position, Pickup.Rotation);
                        Pickup.Destroy();
                    }
                    return;
                }
                else
                {
                    SummonedCustomItem.Summon(CustomItem, Room.List.Where(room => room.Zone == Zone).ToList().RandomItem().Position);
                }
            }
        }
    }
}
