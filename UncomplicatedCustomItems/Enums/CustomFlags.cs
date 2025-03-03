using System;

namespace UncomplicatedCustomItems.Enums
{
    [Flags]
    public enum CustomFlags
    {
        NotExecutable = -1,
        None = 1,
        DoNotTriggerTeslaGates = 1 << 1,
        LifeSteal = 1 << 2,
        HalfLifeSteal = 1 << 3,
        InfiniteAmmo = 1 << 4,
        DieOnUse = 1 << 5,
        WorkstationBan = 1 << 6,
        ItemGlow = 1 << 7,
        EffectWhenUsed = 1 << 8,
        EffectShot = 1 << 9,
        EffectWhenEquiped = 1 << 10,
        NoCharge = 1 << 11,
    }
}