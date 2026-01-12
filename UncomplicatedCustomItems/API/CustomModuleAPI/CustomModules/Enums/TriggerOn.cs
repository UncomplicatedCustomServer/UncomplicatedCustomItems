using System;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules.Enums
{
    [Flags]
    public enum TriggerOn
    {
        OnShot = 1,
        OnUse = 2,
        OnReload = 3,
        OnChangedItem = 4,
        OnAdded = 5,
        OnDropped = 6,
        OnDeath = 7,
        OnHurt = 8,
        OnDoorInteracted = 9,
    }
}