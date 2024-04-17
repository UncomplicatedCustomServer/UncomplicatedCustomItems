using Exiled.API.Features;
using Exiled.API.Features.Items;
using UncomplicatedCustomItems.API.Serializable;

namespace UncomplicatedCustomItems.API.Features
{
    public class CustomItem : CustomThing
    {
        public CustomItem(Player player, SerializableCustomItem serializableCustomItem)
        {
            Name = serializableCustomItem.Name;
            Description = serializableCustomItem.Description;
            Command = serializableCustomItem.Command;
            Response = serializableCustomItem.Response;
            Player = player;

            _serializable = serializableCustomItem;
        }

        public override string Name { get; }

        public override string Description { get; }

        public override Item Item { get; protected set; }

        public Player Player { get; }

        public string Response { get; }

        public string Command { get; }

        private readonly SerializableCustomItem _serializable;

        /// <summary>
        /// Execute command if it is not null, after it message response
        /// </summary>
        public void Execute()
        {
            if (Command is not null && Command != string.Empty)
            {
                Server.ExecuteCommand(Command.Replace("%id%", Player.Id.ToString()), Player.Sender);
            }

            Player.SendConsoleMessage(Response, string.Empty);
        }

        /// <summary>
        /// Spawn custom item in hand
        /// </summary>
        public override void Spawn()
        {
            Item = Item.Create(_serializable.Model, Player);
            Item.Scale = _serializable.Scale;

            Player.CurrentItem = Item;

            Plugin.API.Add(this);
        }
    }
}
