using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.API.Enums
{
    /// <summary>
    /// Contains all the <see cref="ItemEvents"/> that can be used in a <see cref="CustomItemType.Item"/> <see cref="CustomItem"/>
    /// </summary>
    public enum ItemEvents
    {
        Command,
        Pickup,
        Drop,
        Use,
        Noclip,
        Detonation,
        SSSS
    }
}