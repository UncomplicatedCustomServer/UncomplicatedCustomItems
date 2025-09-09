using InventorySystem;
using InventorySystem.Items;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Interfaces;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Extensions
{
    /// <summary>
    /// A set of extensions for <see cref="ItemType"/>.
    /// </summary>
    public static class ItemExtensions
    {
        /// <summary>
        /// Check if an <see cref="ItemType">item</see> is a weapon.
        /// </summary>
        /// <param name="type">The item to be checked.</param>
        /// <param name="checkMicro">Indicates whether the MicroHID item should be taken into account.</param>
        /// <returns>Returns whether the <see cref="ItemType"/> is a weapon.</returns>
        public static bool IsWeapon(this ItemType type, bool checkMicro = true) => type.GetFirearmType() is not FirearmType.None || (checkMicro && type is ItemType.MicroHID);

        /// <summary>
        /// Check if an <see cref="ItemType">item</see> is an SCPItem.
        /// </summary>
        /// <param name="type">The item to be checked.</param>
        /// <returns>Returns whether the <see cref="ItemType"/> is an SCPItem.</returns>
        public static bool IsScp(this ItemType type) => GetCategory(type) == ItemCategory.SCPItem || type == ItemType.GunSCP127;

        /// <summary>
        /// Check if a <see cref="ItemType"/> is an armor item.
        /// </summary>
        /// <param name="type">The item to be checked.</param>
        /// <returns>Returns whether the <see cref="ItemType"/> is an armor.</returns>
        public static bool IsArmor(this ItemType type) => GetCategory(type) == ItemCategory.Armor;

        /// <summary>
        /// Check if an <see cref="ItemType">item</see> is a keycard.
        /// </summary>
        /// <param name="type">The item to be checked.</param>
        /// <returns>Returns whether the <see cref="ItemType"/> is a keycard.</returns>
        public static bool IsKeycard(this ItemType type) => GetCategory(type) == ItemCategory.Keycard;

        public static ItemCategory GetCategory(this ItemType type) => GetItemBase(type).Category;

        public static ItemBase GetItemBase(this ItemType type)
        {
            if (!InventoryItemLoader.AvailableItems.TryGetValue(type, out ItemBase itemBase))
                return null;

            return itemBase;
        }

        public static FirearmType GetFirearmType(this ItemType type) => type switch
        {
            ItemType.GunCOM15 => FirearmType.Com15,
            ItemType.GunCOM18 => FirearmType.Com18,
            ItemType.GunE11SR => FirearmType.E11SR,
            ItemType.GunCrossvec => FirearmType.Crossvec,
            ItemType.GunFSP9 => FirearmType.FSP9,
            ItemType.GunLogicer => FirearmType.Logicer,
            ItemType.GunRevolver => FirearmType.Revolver,
            ItemType.GunAK => FirearmType.AK,
            ItemType.GunA7 => FirearmType.A7,
            ItemType.GunShotgun => FirearmType.Shotgun,
            ItemType.GunCom45 => FirearmType.Com45,
            ItemType.GunFRMG0 => FirearmType.FRMG0,
            ItemType.ParticleDisruptor => FirearmType.ParticleDisruptor,
            ItemType.GunSCP127 => FirearmType.GunSCP127,
            _ => FirearmType.None,
        };

        public static bool IsLightItem(this ItemType item)
        {
            if (item is ItemType.Lantern || item is ItemType.Flashlight)
                return true;

            return false;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Item"/> is a custom item.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns><c>true</c> if the item is a custom item; otherwise, <c>false</c>.</returns>
        public static bool IsCustomItem(this Item item) => Utilities.IsCustomItem(item.Serial);

        /// <summary>
        /// Determines whether the specified <see cref="Item"/> is a summoned custom item.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns><c>true</c> if the item is a summoned custom item; otherwise, <c>false</c>.</returns>
        public static bool IsSummonedCustomItem(this Item item) => Utilities.IsSummonedCustomItem(item.Serial);

        /// <summary>
        /// Attempts to retrieve the <see cref="SummonedCustomItem"/> associated with the specified <see cref="Item"/>.
        /// </summary>
        /// <param name="item">The item to query.</param>
        /// <returns>
        /// The corresponding <see cref="SummonedCustomItem"/> if found; otherwise, <c>null</c>.
        /// </returns>
        public static SummonedCustomItem TryGetSummonedCustomItem(this Item item) => Utilities.TryGetSummonedCustomItem(item.Serial, out SummonedCustomItem customItem) ? customItem : null;

        /// <summary>
        /// Attempts to retrieve the <see cref="ICustomItem"/> associated with the specified <see cref="Item"/>.
        /// </summary>
        /// <param name="item">The item to query.</param>
        /// <returns>
        /// The corresponding <see cref="ICustomItem"/> if found; otherwise, <c>null</c>.
        /// </returns>
        public static ICustomItem TryGetCustomItem(this Item item) => Utilities.TryGetCustomItem(item.Serial, out ICustomItem customItem) ? customItem : null;

        /// <summary>
        /// Compares two <see cref="Item"/> instances to determine if they refer to the same custom item definition.
        /// </summary>
        /// <param name="item1">The first item to compare.</param>
        /// <param name="item2">The second item to compare.</param>
        /// <returns>
        /// <c>true</c> if both items refer to the same custom item definition; otherwise, <c>false</c>.
        /// </returns>
        public static bool CompareCustomItems(this Item item1, Item item2) => TryGetCustomItem(item1) == TryGetCustomItem(item2);

        /// <summary>
        /// Compares two <see cref="Item"/> instances to determine if they are the same summoned custom item.
        /// </summary>
        /// <param name="item1">The first item to compare.</param>
        /// <param name="item2">The second item to compare.</param>
        /// <returns>
        /// <c>true</c> if both items refer to the same summoned custom item instance; otherwise, <c>false</c>.
        /// </returns>
        public static bool CompareSummonedCustomItems(this Item item1, Item item2) => TryGetSummonedCustomItem(item1) == TryGetSummonedCustomItem(item2);

        public static Pickup Create(this Item item, Vector3 pos, Quaternion rot = default, Vector3 scale = default) => Pickup.Create(item.Type, pos, rot, scale == default ? Vector3.one : scale);

        public static Pickup CreateAndSpawn(this Item item, Vector3 pos, Quaternion rot = default, Vector3 scale = default)
        {
            Pickup pickup = Pickup.Create(item.Type, pos, rot, scale == default ? Vector3.one : scale);
            pickup.Spawn();
            return pickup;
        }
    }
}
