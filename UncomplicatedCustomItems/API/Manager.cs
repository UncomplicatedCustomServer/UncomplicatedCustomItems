using Exiled.API.Features;
using Exiled.API.Features.Items;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.API
{
    public class Manager
    {
        internal static Dictionary<uint, ICustomItem> Items = new();
        internal static List<SummonedCustomItem> SummonedItems = new();

        /// <summary>
        /// Register a new <see cref="ICustomItem"/> inside the plugin
        /// </summary>
        /// <param name="Item"></param>
        public static void Register(ICustomItem Item)
        {
            if (!Utilities.CustomItemValidator(Item, out string Error))
            {
                Log.Warn($"Unable to register the ICustomItem with the Id {Item.Id} and name {Item.Name}:\n{Error}");
                return;
            }
            Items.Add(Item.Id, Item);
        }

        /// <summary>
        /// Unregister a <see cref="ICustomItem"/> from the plugin by it's class
        /// </summary>
        /// <param name="Item"></param>
        public static void Unregister(ICustomItem Item)
        {
            if (Items.ContainsKey(Item.Id))
            {
                Items.Remove(Item.Id);
            }
        }

        /// <summary>
        /// Unregister a <see cref="ICustomItem"/> from the plugin by it's Id
        /// </summary>
        /// <param name="Item"></param>
        public static void Unregister(uint Item)
        {
            if (Items.ContainsKey(Item))
            {
                Items.Remove(Item);
            }
        }
    }
}
