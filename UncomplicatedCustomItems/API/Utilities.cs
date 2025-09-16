using MEC;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Interfaces;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using UnityEngine;
using UncomplicatedCustomItems.API.Features.Helper;
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
                    if (!item.Item.IsScp())
                    {
                        error = $"The Item has been flagged as 'SCPItem' but the item {item.Item} is not an SCPItem!";
                        return false;
                    }
                    break;

                case CustomItemType.MicroHID:
                    if (item.CustomData is not IMicroHIDData)
                    {
                        error = $"The item has been flagged as 'MicroHID' but the CustomData class is not 'IMicroHIDData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/null";
                        return false;
                    }

                    if (item.Item is not ItemType.MicroHID)
                    {
                        error = $"The Item has been flagged as 'MicroHID' but the item {item.Item} is not a MicroHID!";
                        return false;
                    }
                    
                    break;

                case CustomItemType.ParticleDisruptor:
                    if (item.CustomData is not IParticleDisruptorData)
                    {
                        error = $"The item has been flagged as 'ParticalDisruptor' but the CustomData class is not 'IParticalDisruptorData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/null";
                        return false;
                    }

                    if (item.Item is not ItemType.ParticleDisruptor)
                    {
                        error = $"The Item has been flagged as 'ParticalDisruptor' but the item {item.Item} is not a Partical Disruptor!";
                        return false;
                    }

                    break;

                case CustomItemType.Light:
                    if (item.CustomData is not IFlashlightData)
                    {
                        error = $"The item has been flagged as 'Light' but the CustomData class is not 'IFlashlightData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/null";
                        return false;
                    }

                    if (!item.Item.IsLightItem())
                    {
                        error = $"The Item has been flagged as 'Light' but the item {item.Item} is not a Flashlight or Lantern!";
                        return false;
                    }

                    break;

                case CustomItemType.Candy:
                    if (item.CustomData is not ICandyData)
                    {
                        error = $"The item has been flagged as 'Candy' with SCP330 but the CustomData class is not 'ICandyData', found '{item.CustomData.GetType().Name}' \n The CustomData formatting is incorrect. Please follow the format found here: https://discord.com/channels/1170301876990914631/[CANDY_DOCUMENTATION_LINK]";
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

        public static bool CustomActionValidator(ICustomAction action, out string error)
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
        /// Check if a <see cref="ICustomItem"/> is valid and can be registered.
        /// Does not return the error as text!
        /// </summary>
        /// <param name="item"></param>
        /// <returns><see cref="bool"/> <see langword="false"/> if there's any problem.</returns>
        public static bool CustomItemValidator(ICustomItem item) => CustomItemValidator(item, out _);

        /// <summary>
        /// Parse a <see cref="object"/> as response to a <see cref="Player"/>
        /// </summary>
        /// <param name="player"></param>
        /// <param name="response"></param>
        public static void ParseResponse(Player player, IItemData response)
        {
            foreach (ItemDataList data in response.Data)
            {
                if (data.ConsoleMessage is not null && data.ConsoleMessage.Length > 1) // FUCK 1 char messages!
                {
                    player.SendConsoleMessage(data.ConsoleMessage, string.Empty);
                }

                if (data.BroadcastMessage.Length > 1 && data.BroadcastDuration > 0)
                {
                    player.SendBroadcast( data.BroadcastMessage, data.BroadcastDuration);
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

        private static Dictionary<PedestalLocker, ICustomItem> UsedLockers { get; set; } = [];

        /// <summary>
        /// Summon a <see cref="CustomItem"/>
        /// </summary>
        /// <param name="customItem"></param>
        internal static void SummonCustomItem(ICustomItem customItem)
        {
            foreach (SpawnData spawn in customItem.Spawn.SpawnSettings)
            {
                float chance = UnityEngine.Random.Range(0f, 101f);
                if (chance >= spawn.Chance)
                    continue;

                SummoningCustomItemEventArgs args = new(customItem);
                Events.Handlers.CustomItemEvents.OnSummoningCustomItem(args);
                if (!args.IsAllowed)
                    continue;

                if (spawn.Coords != Vector3.zero)
                {
                    SpawnAtCoordinate(customItem, spawn);
                    continue;
                }

                if (spawn.DynamicSpawn.Count() > 0)
                {
                    HandleDynamicSpawn(customItem, spawn);
                    continue;
                }

                if (spawn.Zones.Count() > 0)
                {
                    HandleZoneSpawn(customItem, spawn);
                }
                
                SummonedCustomItemEventArgs args1 = new(customItem);
                Events.Handlers.CustomItemEvents.OnSummonedCustomItem(args1);
            }
        }

        private static void SpawnAtCoordinate(ICustomItem customItem, SpawnData spawn)
        {
            if (spawn.Rotation != Vector4.zero)
            {
                spawn.Rotation.Normalize();
                Quaternion rotation = new(spawn.Rotation.x, spawn.Rotation.y, spawn.Rotation.z, spawn.Rotation.w);
                new SummonedCustomItem(customItem, spawn.Coords, rotation);
            }
            else
                new SummonedCustomItem(customItem, spawn.Coords);
        }

        private static void HandleDynamicSpawn(ICustomItem customItem, SpawnData spawn)
        {
            foreach (DynamicSpawn dynamicSpawn in spawn.DynamicSpawn)
            {
                Room room = GetRoomFromDynamicSpawn(dynamicSpawn.Room);
                if (room == null)
                    continue;

                spawn.Rotation.Normalize();
                Quaternion rotation = new(spawn.Rotation.x, spawn.Rotation.y, spawn.Rotation.z, spawn.Rotation.w);

                Vector3 spawnPosition = GetDynamicSpawnPosition(room, dynamicSpawn, spawn, customItem);
                if (spawnPosition != Vector3.zero && spawn.Rotation != Vector4.zero)
                    new SummonedCustomItem(customItem, spawnPosition, rotation);
                else if (spawnPosition != Vector3.zero)
                    new SummonedCustomItem(customItem, spawnPosition);
            }
        }

        internal static Room GetRoomFromDynamicSpawn(string room)
        {
            if (Enum.TryParse(room, out RoomName roomName))
            {
                return Room.Get(roomName).FirstOrDefault();
            }
            
            return Room.List.GetByGameObjectName($"{room}");
        }

        private static Vector3 GetDynamicSpawnPosition(Room room, DynamicSpawn dynamicSpawn, SpawnData spawn, ICustomItem customItem)
        {
            if (dynamicSpawn.Coords != Vector3.zero)
            {
                return room.WorldPosition(dynamicSpawn.Coords);
            }

            if (spawn.ReplaceExistingPickup)
            {
                Pickup targetPickup = FindTargetPickupInRoom(room, spawn, customItem);
                if (targetPickup != null)
                {
                    new SummonedCustomItem(customItem, targetPickup);
                    return Vector3.zero;
                }
            }

            return room.Position;
        }

        private static void HandleZoneSpawn(ICustomItem customItem, SpawnData spawn)
        {
            FacilityZone zone = spawn.Zones.RandomItem();

            if (spawn.ReplaceExistingPickup)
            {
                Pickup targetPickup = FindTargetPickupInZone(zone, spawn, customItem);
                if (targetPickup != null)
                {
                    new SummonedCustomItem(customItem, targetPickup);
                    return;
                }
            }
            
            Room randomRoom = Room.List.Where(room => room.Zone == zone).ToList().RandomItem();
            new SummonedCustomItem(customItem, randomRoom.Position);
        }

        private static Pickup FindTargetPickupInRoom(Room room, SpawnData spawn, ICustomItem customItem)
        {
            List<Pickup> pickupsInRoom = Pickup.List.Where(pickup => 
                pickup.Room == room && 
                !IsSummonedCustomItem(pickup.Serial))
                .ToList();

            return FilterAndSelectPickup(pickupsInRoom, spawn, customItem);
        }

        private static Pickup FindTargetPickupInZone(FacilityZone zone, SpawnData spawn, ICustomItem customItem)
        {
            List<Pickup> pickupsInZone = Pickup.List.Where(pickup => 
                pickup.Room != null && 
                pickup.Room.Zone == zone && 
                !IsSummonedCustomItem(pickup.Serial))
                .ToList();

            if (!(spawn.ReplaceItemsInPedestals ?? false))
            {
                List<ushort> pedestalItemSerials = PedestalLocker.List
                .Select(locker => locker.GetAllItems().FirstOrDefault())
                .Where(pickup => pickup != null)
                .Select(pickup => pickup.Serial)
                .ToList();

                pickupsInZone = pickupsInZone.Where(pickup => 
                    !pedestalItemSerials.Contains(pickup.Serial))
                    .ToList();
            }

            return FilterAndSelectPickup(pickupsInZone, spawn, customItem);
        }

        private static Pickup FilterAndSelectPickup(List<Pickup> pickups, SpawnData spawn, ICustomItem customItem)
        {
            if (spawn.ForceItem)
                pickups = pickups.Where(pickup => pickup.Type == customItem.Item).ToList();

            return pickups.Count > 0 ? pickups.RandomItem() : null;
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