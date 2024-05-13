using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.Elements.SpecificData
{
#nullable enable

    public class ItemData : Data, IItemData
    {
        /// <summary>
        /// The <see cref="ItemEvents"/> of the object
        /// </summary>
        public ItemEvents Event { get; set; } = ItemEvents.Pickup;

        /// <summary>
        /// The command (<see cref="string>"/>) that will be executed when the <see cref="ItemEvents"/> will be fired
        /// </summary>
        public string? Command { get; set; } = null;

        /// <summary>
        /// The message that will be sent inside the console when the <see cref="ItemEvents"/> will be fired
        /// </summary>
        public string ConsoleMessage { get; set; } = "A funny message for the console";

        /// <summary>
        /// The message that will be sent as broadcast when the <see cref="ItemEvents"/> will be fired
        /// </summary>
        public string BroadcastMessage { get; set; } = "The broadcast uuhh!!!";

        /// <summary>
        /// The broadcast duration
        /// </summary>
        public ushort BroadcastDuration { get; set; } = 3;

        /// <summary>
        /// The message that will be sent as hint when the <see cref="ItemEvents"/> will be fired
        /// </summary>
        public string HintMessage { get; set; } = "Yamato is a femboy";

        /// <summary>
        /// The hint duration
        /// </summary>
        public float HintDuration { get; set; } = 2.3f;

        /// <summary>
        /// Do destry the item after the use?
        /// </summary>
        public bool DestroyAfterUse { get; set; } = false;
    }
}
