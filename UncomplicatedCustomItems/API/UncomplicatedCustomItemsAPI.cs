using Exiled.API.Features;
using Exiled.API.Features.Items;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.API
{
    public class UncomplicatedCustomItemsAPI
    {
        public UncomplicatedCustomItemsAPI()
        {
            _items = new List<CustomItem>();
        }

        private List<CustomItem> _items;

        /// <summary>
        /// Add custom item to inventory
        /// </summary>
        /// <param name="player"></param>
        /// <param name="customItem"></param>
        public void Add(CustomItem customItem)
        {
            _items.Add(customItem);
        }

        /// <summary>
        /// Remove custom item
        /// </summary>
        /// <param name="player"></param>
        /// <param name="customItem"></param>
        public void Remove(CustomItem customItem)
        {
            _items.Remove(customItem);
        }

        /// <summary>
        /// Try get custom item
        /// </summary>
        /// <param name="player"></param>
        /// <param name="serial"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryGet(ushort serial, out CustomItem result)
        {
            var customItem = Get(serial);

            if (customItem is null)
            {
                result = null;

                return false;
            }

            result = customItem;

            return true;
        }

        /// <summary>
        /// Get custom item
        /// </summary>
        /// <param name="player"></param>
        /// <param name="serial"></param>
        /// <returns></returns>
        public CustomItem Get(ushort serial)
        {
            return _items.FirstOrDefault(customItem => customItem.Item.Serial == serial); 
        }

        /// <summary>
        /// Is custom item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool IsCustomItem(Item item)
        {
            return _items.FirstOrDefault(customItem => customItem.Item.Serial == item.Serial) is not null;
        }
    }
}
