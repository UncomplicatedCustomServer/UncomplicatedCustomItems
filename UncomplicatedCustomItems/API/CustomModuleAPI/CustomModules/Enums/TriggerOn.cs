using System;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules.Enums
{
    [Flags]
    public enum TriggerOn
    {
        None = 0,
        OnShot = 1 << 0,
        OnUse = 1 << 1,
        OnReload = 1 << 2,
        OnChangedItem = 1 << 3,
        OnAdded = 1 << 4,
        OnDropped = 1 << 5,
        OnDeath = 1 << 6,
        OnHurt = 1 << 7,
        OnDoorInteracted = 1 << 8,
    }
}