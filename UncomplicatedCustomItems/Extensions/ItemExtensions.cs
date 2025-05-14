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
    }
}
