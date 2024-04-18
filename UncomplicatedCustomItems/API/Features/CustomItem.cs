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

        private string Сommand => _itemInfo.Command;

        private readonly ItemInfo _itemInfo;

        /// <summary>
        /// Execute command if it is not null, after it send message with response
        /// </summary>
        public void Execute()
        {
            if (Сommand is not null && Сommand != string.Empty)
            {
                Server.ExecuteCommand(Сommand.Replace("%id%", Player.Id.ToString()), Player.Sender);
            }

            Player.SendConsoleMessage(_itemInfo.Response, string.Empty);
        }
    }
}
