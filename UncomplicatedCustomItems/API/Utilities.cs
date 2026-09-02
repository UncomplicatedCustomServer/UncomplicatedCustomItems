using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UnityEngine;
using UncomplicatedCustomItems.API.Features.Manager;
using UncomplicatedCustomItems.API.Extensions;
using LabApi.Features.Wrappers;
using MapGeneration;
using System;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.Events.Arguments.CustomItemEvents;

namespace UncomplicatedCustomItems.API
{
    /// <summary>
    /// Handles all the <see cref="Utilities"/> needed for UCI.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Check if a <see cref="CustomItem"/> is valid and can be registered
        /// </summary>
        /// <param name="item"></param>
        /// <param name="error"></param>
        /// <returns><see cref="bool"/> <see langword="false"/> if there's any problem. Every error will be outputted with <paramref name="error"/></returns>
        public static bool CustomItemValidator(CustomItem item, out string error)
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
                        error = $"The item has been flagged as 'Item' but the CustomData class is not 'Data', found '{item.CustomData?.GetType().Name}' The CustomData formatting is incorrect. \n Please follow the format found here: https://discord.com/channels/1170301876990914631/1339667038750244979";
                        return false;
                    }

                    break;

                case CustomItemType.Weapon:
                    if (item.CustomData is not WeaponData)
                    {
                        error = $"The item has been flagged as 'Weapon' but the CustomData class is not 'WeaponData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/1170301876990914631/1339666579251793960";
                        return false;
                    }

                    if (!item.Item.IsWeapon())
                    {
                        error = $"The item has been flagged as 'Weapon' but the item {item.Item} is not a weapon in the game!";
                        return false;
                    }

                    break;

                case CustomItemType.Keycard:
                    if (item.CustomData is not KeycardData)
                    {
                        error = $"The item has been flagged as 'Keycard' but the CustomData class is not 'KeycardData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/1170301876990914631/1339667184435073074";
                        return false;
                    }

                    if (!item.Item.IsKeycard())
                    {
                        error = $"The item has been flagged as 'Keycard' but the item {item.Item} is not a keycard in the game!";
                        return false;
                    }

                    break;

                case CustomItemType.Armor:
                    if (item.CustomData is not ArmorData)
                    {
                        error = $"The item has been flagged as 'Armor' but the CustomData class is not 'ArmorData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/1170301876990914631/1339666435491762197";
                        return false;
                    }

                    if (!item.Item.IsArmor())
                    {
                        error = $"The item has been flagged as 'Armor' but the item {item.Item} is not a armor in the game!";
                        return false;
                    }

                    break;

                case CustomItemType.ExplosiveGrenade:
                    if (item.CustomData is not ExplosiveGrenadeData)
                    {
                        error = $"The item has been flagged as 'ExplosiveGrenade' but the CustomData class is not 'ExplosiveGrenadeData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/1170301876990914631/1339667358398152798";
                        return false;
                    }

                    if (item.Item is not ItemType.GrenadeHE)
                    {
                        error = $"The Item has been flagged as 'ExplosiveGrenade' but the item {item.Item} is not a GrenadeHE";
                        return false;
                    }

                    break;

                case CustomItemType.FlashGrenade:
                    if (item.CustomData is not FlashGrenadeData)
                    {
                        error = $"The item has been flagged as 'FlashGrenade' but the CustomData class is not 'FlashGrenadeData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/1170301876990914631/1339666785313755156";
                        return false;
                    }

                    if (item.Item is not ItemType.GrenadeFlash)
                    {
                        error = $"The Item has been flagged as 'FlashGrenade' but the item {item.Item} is not a GrenadeFlash";
                        return false;
                    }

                    break;

                case CustomItemType.Jailbird:
                    if (item.CustomData is not JailbirdData)
                    {
                        error = $"The item has been flagged as 'Jailbird' but the CustomData class is not 'JailbirdData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/1170301876990914631/1342257093629182002";
                        return false;
                    }

                    if (item.Item is not ItemType.Jailbird)
                    {
                        error = $"The Item has been flagged as 'Jailbird' but the item {item.Item} is not a Jailbird";
                        return false;
                    }

                    break;

                case CustomItemType.Medikit:
                    if (item.CustomData is not MedikitData)
                    {
                        error = $"The item has been flagged as 'Medikit' but the CustomData class is not 'MedikitData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/1170301876990914631/1339667529366372443";
                        return false;
                    }

                    if (item.Item is not ItemType.Medkit)
                    {
                        error = $"The Item has been flagged as 'Medikit' but the item {item.Item} is not a Medikit";
                        return false;
                    }

                    break;

                case CustomItemType.Painkillers:
                    if (item.CustomData is not PainkillersData)
                    {
                        error = $"The item has been flagged as 'Painkillers' but the CustomData class is not 'PainkillersData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/1170301876990914631/1354116780846612711";
                        return false;
                    }

                    if (item.Item is not ItemType.Painkillers)
                    {
                        error = $"The Item has been flagged as 'Painkillers' but the item {item.Item} is not a Painkillers";
                        return false;
                    }

                    break;

                case CustomItemType.Adrenaline:
                    if (item.CustomData is not AdrenalineData)
                    {
                        error = $"The item has been flagged as 'Adrenaline' but the CustomData class is not 'AdrenalineData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/null";
                        return false;
                    }

                    if (item.Item is not ItemType.Adrenaline)
                    {
                        error = $"The Item has been flagged as 'Adrenaline' but the item {item.Item} is not a Adrenaline";
                        return false;
                    }

                    break;

                case CustomItemType.SCPItem:
                    if (!item.Item.IsScp())
                    {
                        error = $"The Item has been flagged as 'SCPItem' but the item {item.Item} is not an SCPItem!";
                        return false;
                    }
                    break;

                case CustomItemType.MicroHID:
                    if (item.CustomData is not MicroHIDData)
                    {
                        error = $"The item has been flagged as 'MicroHID' but the CustomData class is not 'MicroHIDData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/null";
                        return false;
                    }

                    if (item.Item is not ItemType.MicroHID)
                    {
                        error = $"The Item has been flagged as 'MicroHID' but the item {item.Item} is not a MicroHID!";
                        return false;
                    }

                    break;

                case CustomItemType.ParticleDisruptor:
                    if (item.CustomData is not ParticleDisruptorData)
                    {
                        error = $"The item has been flagged as 'ParticalDisruptor' but the CustomData class is not 'ParticleDisruptorData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/null";
                        return false;
                    }

                    if (item.Item is not ItemType.ParticleDisruptor)
                    {
                        error = $"The Item has been flagged as 'ParticalDisruptor' but the item {item.Item} is not a Partical Disruptor!";
                        return false;
                    }

                    break;

                case CustomItemType.Light:
                    if (item.CustomData is not FlashlightData)
                    {
                        error = $"The item has been flagged as 'Light' but the CustomData class is not 'FlashlightData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/null";
                        return false;
                    }

                    if (!item.Item.IsLightItem())
                    {
                        error = $"The Item has been flagged as 'Light' but the item {item.Item} is not a Flashlight or Lantern!";
                        return false;
                    }

                    break;

                case CustomItemType.Candy:
                    if (item.CustomData is not CandyData)
                    {
                        error = $"The item has been flagged as 'Candy' with SCP330 but the CustomData class is not 'CandyData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/1170301876990914631/[CANDY_DOCUMENTATION_LINK]";
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

        internal static bool CustomActionValidator(CustomAction action, out string error)
        {
            if (CustomAction.CustomActions.ContainsKey(action.Id))
            {
                uint OldId = action.Id;
                uint NewId = CustomAction.GetFirstFreeId(1);
                action.Id = NewId;
                LogManager.Warn($"{action.Name} - {OldId} ID is already used asigning new ID...\n{action.Name} new ID is {NewId}");
                CustomAction.Register(action);
            }

            error = "";
            return true;
        }

        /// <summary>
        /// Check if a <see cref="CustomItem"/> is valid and can be registered.
        /// Does not return the error as text!
        /// </summary>
        /// <param name="item"></param>
        /// <returns><see cref="bool"/> <see langword="false"/> if there's any problem.</returns>
        public static bool CustomItemValidator(CustomItem item) => CustomItemValidator(item, out _);

        /// <summary>
        /// Parse a <see cref="object"/> as response to a <see cref="Player"/>
        /// </summary>
        /// <param name="player"></param>
        /// <param name="response"></param>
        public static void ParseResponse(Player player, ItemData response)
        {
            foreach (ItemDataList data in response.Data)
            {
                if (data.ConsoleMessage is not null && data.ConsoleMessage.Length > 1) // FUCK 1 char messages!
                {
                    player.SendConsoleMessage(data.ConsoleMessage, string.Empty);
                }

                if (data.BroadcastMessage.Length > 1 && data.BroadcastDuration > 0)
                {
                    player.SendBroadcast(data.BroadcastMessage, data.BroadcastDuration);
                }

                if (data.HintMessage.Length > 1 && data.HintDuration > 0)
                {
                    player.SendHint(data.HintMessage, data.HintDuration);
                }
            }
        }

        /// <summary>
        /// Try to get a <see cref="SummonedCustomItem"/> by it's <see cref="Item.Serial"/>
        /// </summary>
        /// <param name="serial"></param>
        /// <param name="item"></param>
        /// <returns><see langword="true"/> if succeeded</returns>
        public static bool TryGetSummonedCustomItem(ushort serial, out SummonedCustomItem? item) => SummonedCustomItem.TryGet(serial, out item);

        /// <summary>
        /// Try to get a <see cref="SummonedCustomItem"/> by it's <see cref="Item"/>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="customItem"></param>
        /// <returns><see langword="true"/> if succeeded</returns>
        public static bool TryGetSummonedCustomItem(Item item, out SummonedCustomItem? customItem) => SummonedCustomItem.TryGet(item.Serial, out customItem);

        /// <summary>
        /// Get a <see cref="SummonedCustomItem"/> by it's serial
        /// </summary>
        /// <param name="serial"></param>
        /// <returns><see cref="SummonedCustomItem"/> if succeeded, <see langword="null"/> if not</returns>
        public static SummonedCustomItem? GetSummonedCustomItem(ushort serial) => SummonedCustomItem.Get(serial);

        /// <summary>
        /// Get a <see cref="SummonedCustomItem"/> by it's <see cref="Item"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns><see cref="SummonedCustomItem"/> if succeeded, <see langword="null"/> if not</returns>
        public static SummonedCustomItem? GetSummonedCustomItem(Item item) => SummonedCustomItem.Get(item.Serial);

        /// <summary>
        /// Check if an item is a <see cref="SummonedCustomItem"/> by it's <see cref="Item.Serial"/>
        /// </summary>
        /// <param name="serial"></param>
        /// <returns><see cref="bool"/> <see langword="true"/> if it is</returns>
        public static bool IsSummonedCustomItem(ushort serial) => SummonedCustomItem.Get(serial) is not null;

        /// <summary>
        /// Try to get a <see cref="CustomItem"/> by it's <see cref="CustomItem.Id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <returns><see cref="bool"/> <see langword="true"/> if the item exists and <paramref name="item"/> is not <see langword="null"/> or <see langword="default"/></returns>
        public static bool TryGetCustomItem(uint id, out CustomItem item) => CustomItem.CustomItems.TryGetValue(id, out item);

        /// <summary>
        /// Try to get a <see cref="CustomItem"/> by it's <see cref="CustomItem.Name"/>
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="item"></param>
        /// <returns><see cref="bool"/> <see langword="true"/> if the item exists and <paramref name="item"/> is not <see langword="null"/> or <see langword="default"/></returns>
        public static bool TryGetCustomItemByName(string Name, out CustomItem item)
        {
            item = CustomItem.List.FirstOrDefault(i => i.Name == Name);
            return item != null;
        }

        /// <summary>
        /// Get a <see cref="CustomItem"/> by it's <see cref="CustomItem.Id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns><see cref="CustomItem"/> if it exists, otherwhise a <see langword="default"/> will be returned</returns>
        public static CustomItem GetCustomItem(uint id) => CustomItem.CustomItems[id];

        /// <summary>
        /// Check if the given <see cref="CustomItem.Id"/> is already registered as a <see cref="CustomItem"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsCustomItem(uint id) => CustomItem.CustomItems.ContainsKey(id);

        private static Dictionary<PedestalLocker, CustomItem> UsedLockers { get; set; } = [];

        /// <summary>
        /// Summon a <see cref="CustomItem"/>
        /// </summary>
        /// <param name="customItem"></param>
        internal static void SummonCustomItem(CustomItem customItem, bool ignoreChance = false)
        {
            foreach (SpawnData spawn in customItem.Spawn.SpawnSettings)
            {
                if (!ignoreChance)
                {
                    float roll = UnityEngine.Random.Range(0f, 101f);
                    if (roll >= spawn.Chance)
                        continue;
                }

                SummoningCustomItemEventArgs args = new(customItem);
                Events.Handlers.CustomItemEvents.OnSummoningCustomItem(args);
                if (!args.IsAllowed)
                    continue;

                if (spawn.Coords != Vector3.zero)
                {
                    SpawnAtCoordinate(customItem, spawn);
                    continue;
                }

                if (spawn.DynamicSpawn.Count > 0)
                {
                    HandleDynamicSpawn(customItem, spawn);
                    continue;
                }

                if (spawn.Zones.Count > 0)
                    HandleZoneSpawn(customItem, spawn);

                Events.Handlers.CustomItemEvents.OnSummonedCustomItem(new(customItem));
            }
        }

        private static void SpawnAtCoordinate(CustomItem customItem, SpawnData spawn)
        {
            if (spawn.Rotation != Vector3.zero)
            {
                spawn.Rotation.Normalize();
                Quaternion rotation = Quaternion.Euler(spawn.Rotation);
                new SummonedCustomItem(customItem, spawn.Coords, rotation);
            }
            else
                new SummonedCustomItem(customItem, spawn.Coords);
        }

        private static void HandleDynamicSpawn(CustomItem customItem, SpawnData spawn)
        {
            foreach (DynamicSpawn dynamicSpawn in spawn.DynamicSpawn)
            {
                Room room = GetRoomFromName(dynamicSpawn.Room);
                if (room == null)
                    continue;

                LogManager.Debug($"Got room {room.Name} from DynamicSpawn {customItem.Name} - {customItem.Id}");
                spawn.Rotation.Normalize();
                Quaternion rotation = Quaternion.Euler(spawn.Rotation);

                Vector3 spawnPosition = GetDynamicSpawnPosition(room, dynamicSpawn, spawn, customItem);
                if (spawn.Rotation != Vector3.zero)
                {
                    new SummonedCustomItem(customItem, spawnPosition, rotation);
                }
                else
                    new SummonedCustomItem(customItem, spawnPosition);
            }
        }

        internal static Room GetRoomFromName(string room)
        {
            if (Enum.TryParse(room, out RoomName roomName))
                return Room.Get(roomName).FirstOrDefault();

            return Room.List.GetByGameObjectName(room);
        }

        private static Vector3 GetDynamicSpawnPosition(Room room, DynamicSpawn dynamicSpawn, SpawnData spawn, CustomItem customItem)
        {
            if (spawn.ReplaceExistingPickup)
            {
                Pickup? targetPickup = FindTargetPickupInRoom(room, spawn, customItem);
                if (targetPickup != null)
                {
                    new SummonedCustomItem(customItem, targetPickup);
                    return Vector3.zero;
                }
            }
            else
                return room.WorldPosition(dynamicSpawn.Coords);

            return room.Position;
        }

        private static void HandleZoneSpawn(CustomItem customItem, SpawnData spawn)
        {
            FacilityZone zone = spawn.Zones.RandomItem();

            if (spawn.ReplaceExistingPickup)
            {
                Pickup? targetPickup = FindTargetPickupInZone(zone, spawn, customItem);
                if (targetPickup != null)
                {
                    new SummonedCustomItem(customItem, targetPickup);
                    return;
                }
            }

            Room randomRoom = Room.List.Where(room => room.Zone == zone).ToList().RandomItem();
            new SummonedCustomItem(customItem, randomRoom.Position);
        }

        private static Pickup? FindTargetPickupInRoom(Room room, SpawnData spawn, CustomItem customItem)
        {
            List<Pickup> pickupsInRoom = Pickup.List.Where(pickup => pickup.Room == room && !IsSummonedCustomItem(pickup.Serial)).ToList();
            return FilterAndSelectPickup(pickupsInRoom, spawn, customItem);
        }

        private static Pickup? FindTargetPickupInZone(FacilityZone zone, SpawnData spawn, CustomItem customItem)
        {
            List<Pickup> pickupsInZone = Pickup.List.Where(pickup => pickup.Room != null && pickup.Room.Zone == zone && !IsSummonedCustomItem(pickup.Serial)).ToList();
            if (!(spawn.ReplaceItemsInPedestals ?? false))
            {
                List<ushort> pedestalItemSerials = PedestalLocker.List.Select(locker => locker.GetAllItems().FirstOrDefault()).Where(pickup => pickup != null).Select(pickup => pickup.Serial).ToList();
                pickupsInZone = pickupsInZone.Where(pickup => !pedestalItemSerials.Contains(pickup.Serial)).ToList();
            }

            return FilterAndSelectPickup(pickupsInZone, spawn, customItem);
        }

        private static Pickup? FilterAndSelectPickup(List<Pickup> pickups, SpawnData spawn, CustomItem customItem)
        {
            if (spawn.ForceItem)
                pickups = pickups.Where(pickup => pickup.Type == customItem.Item).ToList();

            return pickups.Count > 0 ? pickups.RandomItem() : null;
        }
    }
}