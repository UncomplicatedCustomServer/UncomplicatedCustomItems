using System.ComponentModel;
using UncomplicatedCustomItems.Interfaces;
using UnityEngine;

#nullable enable
namespace UncomplicatedCustomItems.Elements
{
    public class CustomItem : ICustomItem
    {
        [Description("The Id of the custom item")]
        public uint Id { get; set; } = 1;

        [Description("The name of the custom item")]
        public string Name { get; set; } = "Detonator";

        [Description("The description of the custom item")]
        public string Description { get; set; } = "11/09/2001 uwu uwu uwu";

        [Description("The Item base for the custom item")]
        public ItemType Item { get; set; } = ItemType.Coin;

        [Description("The scale of the custom item, 0 0 0 means disabled")]
        public Vector3 Scale { get; set; } = new();

        [Description("The event when the item will execute the command and the response")]
        public ItemEvents Event { get; set; } = ItemEvents.Drop;

        [Description("The command that will be executed when the event is triggered")]
        public string? Command { get; set; } = "/SERVER_EVENT DETONATION_INSTANT";

        [Description("The response. null to disable")]
        public Response? Response { get; set; } = new();

        [Description("Can this item's events be casted?")]
        public bool EventsEnabled { get; set; } = true;
    }
}
