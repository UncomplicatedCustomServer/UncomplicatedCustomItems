using System;

namespace UncomplicatedCustomItems.Enums
{
    /// <summary>
    /// Contains all of the available <see cref="CustomFlags"/>
    /// </summary>
    [Flags]
    public enum CustomFlags
    {
        NotExecutable = -1,
        None = 1,
        DoNotTriggerTeslaGates = 1 << 1,
        LifeSteal = 1 << 2,
        InfiniteAmmo = 1 << 3,
        DieOnUse = 1 << 4,
        WorkstationBan = 1 << 5,
        ItemGlow = 1 << 6,
        EffectWhenUsed = 1 << 7,
        EffectShot = 1 << 8,
        EffectWhenEquiped = 1 << 9,
        NoCharge = 1 << 10,
        CustomSound = 1 << 11,
        ExplosiveBullets = 1 << 12,
    }
}