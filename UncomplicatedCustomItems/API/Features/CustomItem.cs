using Exiled.API.Features;
using Exiled.API.Features.Items;
using UncomplicatedCustomItems.API.Serializable;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Features
{
    public class CustomItem
    {
        public CustomItem(Player player, int id, SerializableCustomItem serializableCustomItem)
        {
            Name = serializableCustomItem.Name;
            Id = id;
            Description = serializableCustomItem.Description;
            Command = serializableCustomItem.Command;
            Response = serializableCustomItem.Response;
            Player = player;

            _serializable = serializableCustomItem;
        }

        public string Name { get; }

        public string Description { get; }

        public int Id { get; }

        public Player Player { get; }

        public Item Item { get; private set; }

        public string Response { get; }

        public string Command { get; }

        private SerializableCustomItem _serializable;

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
        public void Spawn()
        {
            Item = Item.Create(_serializable.Model, Player);
            Item.Scale = _serializable.Scale;

            Player.CurrentItem = Item;

            Plugin.API.Add(this);
        }
    }
}
