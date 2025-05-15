using InventorySystem;
using InventorySystem.Items;
using UncomplicatedCustomItems.Enums;

namespace UncomplicatedCustomItems.Extensions
{
    /// <summary>
    /// A set of extensions for <see cref="ItemType"/>.
    /// </summary>
    public static class ItemExtensions
    {
        /// <summary>
        /// Check if an <see cref="ItemType">item</see> is an ammo.
        /// </summary>
        /// <param name="item">The item to be checked.</param>
        /// <returns>Returns whether the <see cref="ItemType"/> is an ammo.</returns>
        public static bool IsAmmo(this ItemType item) => item.GetAmmoType() is not AmmoType.None;

        /// <summary>
        /// Check if an <see cref="ItemType">item</see> is a weapon.
        /// </summary>
        /// <param name="type">The item to be checked.</param>
        /// <param name="checkMicro">Indicates whether the MicroHID item should be taken into account.</param>
        /// <returns>Returns whether the <see cref="ItemType"/> is a weapon.</returns>
        public static bool IsWeapon(this ItemType type, bool checkMicro = true) => type.GetFirearmType() is not FirearmType.None || (checkMicro && type is ItemType.MicroHID);

        /// <summary>
        /// Check if an <see cref="ItemType">item</see> is an SCP.
        /// </summary>
        /// <param name="type">The item to be checked.</param>
        /// <returns>Returns whether the <see cref="ItemType"/> is an SCP.</returns>
        public static bool IsScp(this ItemType type) => GetCategory(type) == ItemCategory.SCPItem;

        /// <summary>
        /// Check if an <see cref="ItemType">item</see> is a throwable item.
        /// </summary>
        /// <param name="type">The item to be checked.</param>
        /// <returns>Returns whether the <see cref="ItemType"/> is a throwable item.</returns>
        public static bool IsThrowable(this ItemType type) => type is ItemType.SCP018 or ItemType.GrenadeHE or ItemType.GrenadeFlash or ItemType.SCP2176 or ItemType.Coal or ItemType.SpecialCoal or ItemType.Snowball;

        /// <summary>
        /// Check if an <see cref="ItemType">item</see> is a medical item.
        /// </summary>
        /// <param name="type">The item to be checked.</param>
        /// <returns>Returns whether the <see cref="ItemType"/> is a medical item.</returns>
        public static bool IsMedical(this ItemType type) => GetCategory(type) == ItemCategory.Medical;

        /// <summary>
        /// Check if an <see cref="ItemType">item</see> is a utility item.
        /// </summary>
        /// <param name="type">The item to be checked.</param>
        /// <returns>Returns whether the <see cref="ItemType"/> is an utilty item.</returns>
        public static bool IsUtility(this ItemType type) => type is ItemType.Flashlight or ItemType.Radio;

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

        public static AmmoType GetAmmoType(this ItemType type) => type switch
        {
            ItemType.Ammo9x19 => AmmoType.Nato9,
            ItemType.Ammo556x45 => AmmoType.Nato556,
            ItemType.Ammo762x39 => AmmoType.Nato762,
            ItemType.Ammo12gauge => AmmoType.Ammo12Gauge,
            ItemType.Ammo44cal => AmmoType.Ammo44Cal,
            _ => AmmoType.None,
        };

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


    }
}
