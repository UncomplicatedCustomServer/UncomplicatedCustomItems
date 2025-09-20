using LabApi.Features.Wrappers;
using MapGeneration;
using Org.BouncyCastle.Math.EC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Interfaces;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    /// <summary>
    /// The API version of <see cref="CustomItem"/>
    /// Aims to simplify the process of making CustomItems in C#
    /// </summary>
    public abstract class BaseCustomItem
    {
        /// <summary>
        /// Gets a list of every registered <see cref="BaseCustomItem"/>
        /// </summary>
        public static IReadOnlyCollection<BaseCustomItem> List => CustomItems.Values.ToList();

        internal static Dictionary<uint, BaseCustomItem> CustomItems { get; set; } = [];

        public static void Register(BaseCustomItem item)
        {
            if (CustomItems.ContainsKey(item.Id) || CustomItem.CustomItems.ContainsKey(item.Id))
            {
                uint id = GetFirstFreeId();
                item.Id = id;
                CustomItems.TryAdd(item.Id, item);
                LogManager.Info($"{nameof(BaseCustomItem)}: Successfully registered CustomItem '{item.Name}' (Id: {item.Id}) into the plugin!");
            }
            else
            {
                CustomItems.TryAdd(item.Id, item);
                LogManager.Info($"{nameof(BaseCustomItem)}: Successfully registered CustomItem '{item.Name}' (Id: {item.Id}) into the plugin!");
            }
        }

        /// <summary>
        /// Unregister a <see cref="BaseCustomItem"/> from the plugin by its class
        /// </summary>
        /// <param name="item"></param>
        public static void Unregister(BaseCustomItem item) => Unregister(item.Id);

        /// <summary>
        /// Unregister a <see cref="BaseCustomItem"/> from the plugin by its Id
        /// </summary>
        /// <param name="item"></param>
        public static void Unregister(uint item)
        {
            if (CustomItems.ContainsKey(item))
                CustomItems.Remove(item);
        }

        /// <summary>
        /// Gets the first free Id for a custom item
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public static uint GetFirstFreeId(uint from = 1)
        {
            for (uint i = from; i < uint.MaxValue; i++)
            {
                if (!CustomItems.ContainsKey(i) && !CustomItem.CustomItems.ContainsKey(i))
                    return i;
            }

            return 0;
        }
        
        public static void SummonItem(BaseCustomItem item)
        {
            if (item.SpawnLocations.Count() >= 1)
            {
                foreach (var dic in item.SpawnLocations)
                {
                    Vector3 pos;
                    Room room = Utilities.GetRoomFromDynamicSpawn(dic.Key);

                    if (dic.Value != Vector3.zero)
                        pos = room.WorldPosition(dic.Value);

                    if (item.ReplaceExistingPickup)
                    {
                        Pickup targetPickup = CustomItemUtils.FindTargetPickupInRoom(room, item);
                        if (targetPickup != null)
                            new SummonedBaseCustomItem(item, targetPickup);
                    }

                }
            }
            if (item.Zones.Count() >= 1)
            {
                foreach (FacilityZone zone in item.Zones)
                {
                    List<Room> rooms = Room.List.Where(r => r != null && r.Zone == zone).ToList();
                    if (rooms.Count() > 1)
                    {
                        Room room = rooms.RandomItem();
                        if (item.ReplaceExistingPickup && !item.ForceSameItemType)
                        {
                            Pickup targetPickup = CustomItemUtils.FindTargetPickupInRoom(room, item);
                            if (targetPickup != null)
                                new SummonedBaseCustomItem(item, targetPickup);
                        }
                    }
                    else
                    {
                        Room room = rooms.FirstOrDefault();
                    }
                }
            }
            if (item.Coordinates.Count() >= 1)
            {
                foreach (Vector3 coords in item.Coordinates.Where(c => c != Vector3.zero))
                {
                    if (item.Rotation != Vector4.zero)
                    {
                        item.Rotation.Normalize();
                        Quaternion rotation = new(item.Rotation.x, item.Rotation.y, item.Rotation.z, item.Rotation.w);
                        new SummonedBaseCustomItem(item, coords, rotation);
                    }
                    else
                        new SummonedBaseCustomItem(item, coords);
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="BaseCustomItem"/> by its unique Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An <see cref="BaseCustomItem"/></returns>
        public static BaseCustomItem? Get(uint id)
        {
            if (!CustomItems.ContainsKey(id))
                return null;

            return CustomItems[id];
        }

        /// <summary>
        /// Gets the <see cref="BaseCustomItem"/> by its class type
        /// </summary>
        /// <param name="t"></param>
        /// <returns><see cref="IEnumerable{BaseCustomItem}"/></returns>
        public static IEnumerable<BaseCustomItem> Get(Type t) => List.Where(i => i.GetType() == t);

        /// <summary>
        /// Tries to get a list of <see cref="BaseCustomItem"/> by its class type
        /// </summary>
        /// <param name="t"></param>
        /// <param name="items"></param>
        /// <returns><see langword="true"/> if found otherwise <see langword="false"/> if not found</returns>
        public static bool TryGet(Type t, out IEnumerable<BaseCustomItem> items)
        {
            items = Get(t);

            return items.Any();
        }

        /// <summary>
        /// Tries to get a <see cref="BaseCustomItem"/> by its unique Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <returns><see cref="BaseCustomItem"/> instance</returns>
        public static bool TryGet(uint id, out BaseCustomItem? item)
        {
            item = null;
            if (CustomItems.ContainsKey(id))
            {
                item = CustomItems[id];
                return true;
            }

            return false;
        }

        /// <summary>
        /// The unique Id of the Custom Item. Can't be less than 1
        /// </summary>
        public abstract uint Id { get; set; }

        /// <summary>
        /// The Name of the object. Can appears when for example you pick it up
        /// </summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// The description.
        /// </summary>
        public abstract string Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string ExtendedDescription { get; set; }

        /// <summary>
        /// Gets or sets the badge name
        /// </summary>
        public virtual string BadgeName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the badge color
        /// </summary>
        public virtual string BadgeColor { get; set; } = string.Empty;

        /// <summary>
        /// The weight of the item
        /// </summary>
        public abstract float Weight { get; set; }

        /// <summary>
        /// Gets or sets the scale of the <see cref="BaseCustomItem"/> as a <see cref="Pickup"/>
        /// </summary>
        public virtual Vector3 Scale { get; set; }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        public virtual bool Spawn { get; set; }

        /// <summary>
        /// Gets or sets the amount of <see cref="BaseCustomItem"/>s to spawn
        /// </summary>
        public virtual int AmountToSpawn { get; set; }

        /// <summary>
        /// Gets or sets the chance for the <see cref="BaseCustomItem"/> to spawn.
        /// </summary>
        public virtual float ChanceToSpawn { get; set; }

        /// <summary>
        /// Gets or sets the rotation of the <see cref="BaseCustomItem"/> when first spawned as a <see cref="Pickup"/>
        /// </summary>
        public virtual Vector4 Rotation { get; set; }

        /// <summary>
        /// Gets or sets the locations that the <see cref="BaseCustomItem"/> can spawn.
        /// The dictionary maps a <see cref="RoomName"/> or GameObject name (key) to a local <see cref="Vector3"/> position
        /// </summary>
        /// <remarks>
        /// Has the highest priority
        /// </remarks>
        public virtual Dictionary<string, Vector3> SpawnLocations { get; set; } = [];

        /// <summary>
        /// Gets or sets the zones the <see cref="BaseCustomItem"/> can spawn.
        /// Selects a random room in the <see cref="FacilityZone"/> to spawn the <see cref="BaseCustomItem"/> in.
        /// </summary>
        /// <remarks>
        /// Has the second highest priority
        /// </remarks>
        public virtual List<FacilityZone> Zones { get; set; } = [];

        /// <summary>
        /// Gets or sets the coordinates that the <see cref="BaseCustomItem"/> can spawn.
        /// </summary>
        /// <remarks>
        /// The only static coordinates are in the suface zone
        /// Has the least priority
        /// </remarks>
        public virtual List<Vector3> Coordinates { get; set; } = [];

        /// <summary>
        /// Gets or sets wether the <see cref="BaseCustomItem"/> will replace a <see cref="Pickup"/> when it spawns.
        /// </summary>
        public virtual bool ReplaceExistingPickup { get; set; }

        /// <summary>
        /// Gets or sets wether the <see cref="BaseCustomItem"/> can only replace a <see cref="Pickup"/> of the same <see cref="ItemType"/>
        /// </summary>
        public virtual bool ForceSameItemType { get; set; }

        /// <summary>
        /// Whether if the item won't be removed from the player's inventory
        /// </summary>
        public virtual bool Reusable { get; set; } = false;

        /// <summary>
        /// The <see cref="ItemType"/> (Base) of the Custom Item
        /// </summary>
        public abstract ItemType Item { get; set; }
    }
}