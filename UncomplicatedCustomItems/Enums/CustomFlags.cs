using System;

namespace UncomplicatedCustomItems.Enums
{
    /// <summary>
    /// Contains all of the available <see cref="CustomFlags"/>
    /// </summary>
    [Flags]
    public enum CustomFlags : long
    {
        // CustomFlags added via CustomFlagsExtensions should use 49 to 62
        None = 0,
        DoNotTriggerTeslaGates = 1L << 1,
        LifeSteal = 1L << 2,
        InfiniteAmmo = 1L << 3,
        DieOnUse = 1L << 4,
        WorkstationBan = 1L << 5,
        ItemGlow = 1L << 6,
        EffectWhenUsed = 1L << 7,
        EffectShot = 1L << 8,
        EffectWhenEquiped = 1L << 9,
        NoCharge = 1L << 10,
        CustomSound = 1L << 11,
        ExplosiveBullets = 1L << 12,
        ToolGun = 1L << 13,
        SpawnItemWhenDetonated = 1L << 14,
        Cluster = 1L << 15,
        SwitchRoleOnUse = 1L << 16,
        DieOnDrop = 1L << 17,
        VaporizeKills = 1L << 18,
        CantDrop = 1L << 19,
        Custom = 1L << 48, // This should only be used if your coding your own CustomFlag
    }
}