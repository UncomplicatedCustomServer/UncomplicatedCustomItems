using Exiled.API.Features;
using UncomplicatedCustomItems.API.Features.Data;
using UncomplicatedCustomItems.API.Serializable;

namespace UncomplicatedCustomItems.API.Features
{
    public class CustomItem : CustomThing
    {
        public CustomItem(Player player, ItemInfo itemInfo, SerializableCustomItem serializableCustomItem) : base(player, itemInfo, serializableCustomItem)
        {
            _itemInfo = itemInfo;
        }

        private readonly ItemInfo _itemInfo;

        /// <summary>
        /// Execute command if it is not null, after it send message with response
        /// </summary>
        public void Execute()
        {
            Player.SendConsoleMessage(_itemInfo.Response, string.Empty);

            if (_itemInfo.Commands is null)
            {
                return;
            }

            foreach (var command in _itemInfo.Commands)
            {
                if (command is not null && command != string.Empty)
                {
                    Server.ExecuteCommand(command.Replace("%id%", Player.Id.ToString()), Player.Sender);
                }
            }
        }
    }
}
