using Exiled.API.Features;
using Exiled.API.Features.Items;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Interfaces;

// Spgerg is a furry ehehe, foxworn too! hehehe

namespace UncomplicatedCustomItems.API
{
    public static class Manager
    {
        internal static Dictionary<uint, ICustomItem> Items = new();
        internal static List<SummonedCustomItem> SummonedItems = new();

        /// <summary>
        /// Register a new <see cref="ICustomItem"/> inside the plugin
        /// </summary>
        /// <param name="item"></param>
        public static void Register(ICustomItem item)
        {
            if (!Utilities.CustomItemValidator(item, out string error))
            {
                Log.Warn($"Unable to register the ICustomItem with the Id {item.Id} and name '{item.Name}':\n{error}\nError code: 0x029");
                return;
            }
            Items.Add(item.Id, item);
            Log.Info($"Successfully registered ICustomItem '{item.Name}' (Id: {item.Id}) into the plugin!");
        }

        /// <summary>
        /// Unregister a <see cref="ICustomItem"/> from the plugin by it's class
        /// </summary>
        /// <param name="item"></param>
        public static void Unregister(ICustomItem item)
        {
            if (Items.ContainsKey(item.Id))
            {
                Items.Remove(item.Id);
            }
        }

        /// <summary>
        /// Unregister a <see cref="ICustomItem"/> from the plugin by it's Id
        /// </summary>
        /// <param name="item"></param>
        public static void Unregister(uint item)
        {
            if (Items.ContainsKey(item))
            {
                Items.Remove(item);
            }
        }
    }
}
