using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using UncomplicatedCustomItems.API.Enums;
using System.Collections.Generic;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
#nullable enable

    /// <summary>
    /// The data associated with <see cref="CustomItemType.Item"/> <see cref="CustomItem"/>s
    /// </summary>
    public class ItemData : Data, IItemData
    {
        public virtual List<ItemDataList> Data { get; set; } = new List<ItemDataList>
        {
            new() 
            {
                Event = ItemEvents.Pickup,
                Command = null,
                CoolDown = 1f,
                ConsoleMessage = "A funny message for the console",
                BroadcastMessage = "The broadcast uuhh!!!",
                BroadcastDuration = 3,
                HintMessage = "Yamato is a femboy",
                HintDuration = 2.3f,
                DestroyAfterUse = false
            }
        };  
    }
}
