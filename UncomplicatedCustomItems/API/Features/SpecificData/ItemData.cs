using UncomplicatedCustomItems.Interfaces.SpecificData;
using System;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
#nullable enable

    /// <summary>
    /// The data associated with <see cref="CustomItemType.Item"/> <see cref="CustomItem"/>s
    /// </summary>
    public class ItemData : Data, IItemData
    {
        /// <summary>
        /// Gets or sets the <see cref="ItemEvents"/> of the object
        /// </summary>
        public virtual ItemEvents Event { get; set; } = ItemEvents.Pickup;

        /// <summary>
        /// Gets or sets the command (<see cref="string"/>) that will be executed when the <see cref="ItemEvents"/> will be fired
        /// </summary>
        public virtual string? Command { get; set; } = null;

        /// <summary>
        /// Gets or sets when the delay after <see cref="ItemEvents"/> is fired. 
        /// </summary>
        public virtual float CoolDown { get; set; } = 1f;
        /// <summary>
        /// Gets or sets the message that will be sent inside the console when the <see cref="ItemEvents"/> will be fired
        /// </summary>
        public virtual string ConsoleMessage { get; set; } = "A funny message for the console";

        /// <summary>
        /// Gets or sets the message that will be sent as broadcast when the <see cref="ItemEvents"/> will be fired
        /// </summary>
        public virtual string BroadcastMessage { get; set; } = "The broadcast uuhh!!!";

        /// <summary>
        /// Gets or sets the broadcast duration
        /// </summary>
        public virtual ushort BroadcastDuration { get; set; } = 3;

        /// <summary>
        /// Gets or sets the message that will be sent as hint when the <see cref="ItemEvents"/> will be fired
        /// </summary>
        public virtual string HintMessage { get; set; } = "Yamato is a femboy";

        /// <summary>
        /// Gets or sets the hint duration
        /// </summary>
        public virtual float HintDuration { get; set; } = 2.3f;

        /// <summary>
        /// Gets or sets whether should the item be destroyed after it's use
        /// </summary>
        public virtual bool DestroyAfterUse { get; set; } = false;
    }
}
