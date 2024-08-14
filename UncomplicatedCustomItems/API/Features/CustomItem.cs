using Exiled.API.Features;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UncomplicatedCustomItems.Interfaces;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using UnityEngine;

#nullable enable
namespace UncomplicatedCustomItems.API.Features
{
    public class CustomItem : ICustomItem
    {
        /// <summary>
        /// Gets a list of every registered <see cref="ICustomItem"/>
        /// </summary>
        public static List<ICustomItem> List => CustomItems.Values.ToList();

        internal static Dictionary<uint, ICustomItem> CustomItems { get; } = new();

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
            CustomItems.Add(item.Id, item);
            Log.Info($"Successfully registered ICustomItem '{item.Name}' (Id: {item.Id}) into the plugin!");
        }

        /// <summary>
        /// Unregister a <see cref="ICustomItem"/> from the plugin by it's class
        /// </summary>
        /// <param name="item"></param>
        public static void Unregister(ICustomItem item) => Unregister(item.Id);

        /// <summary>
        /// Unregister a <see cref="ICustomItem"/> from the plugin by it's Id
        /// </summary>
        /// <param name="item"></param>
        public static void Unregister(uint item)
        {
            if (CustomItems.ContainsKey(item))
                CustomItems.Remove(item);
        }

        /// <summary>
        /// Gets the first free Id for a custom item
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public static uint GetFirstFreeId(uint from = 0)
        {
            for (uint i = from; i < uint.MaxValue; i++)
                if (!CustomItems.ContainsKey(i))
                    return i;

            return 0;
        }

        /// <summary>
        /// The unique Id of the Custom Item. Can't be <= 0
        /// </summary>
        public uint Id { get; set; } = 1;

        /// <summary>
        /// The Name of the object. Can appears when for example you pick it up
        /// </summary>
        [Description("The name of the custom item")]
        public string Name { get; set; } = "Detonator";

        /// <summary>
        /// The description. Useful at the moment ig
        /// </summary>
        [Description("The description of the custom item")]
        public string Description { get; set; } = "11/09/20 COFF COFF uwu uwu uwu";

        /// <summary>
        /// The weight of the item
        /// </summary>
        [Description("The weight of the custom item")]
        public float Weight { get; set; } = 2f;

        /// <summary>
        /// The <see cref="ItemType"/> (Base) of the Custom Item
        /// </summary>
        [Description("The Item base for the custom item")]
        public ItemType Item { get; set; } = ItemType.Coin;

        /// <summary>
        /// The Scale of the Custom Item. If 0, 0, 0 then it's disabled
        /// </summary>
        [Description("The scale of the custom item, 0 0 0 means disabled")]
        public Vector3 Scale { get; set; } = Vector3.one;

        /// <summary>
        /// The <see cref="Elements.Spawn"/> settings for the item
        /// </summary>
        [Description("The spawn settings for the item")]
        public ISpawn Spawn { get; set; } = new Spawn();

        /// <summary>
        /// The <see cref="CustomItemType"/> of the Custom Item
        /// </summary>
        public CustomItemType CustomItemType { get; set; } = CustomItemType.Item;

        /// <summary>
        /// The <see cref="IData">Custom Data</see>, based on the CustomItemType
        /// </summary>
        public IData CustomData { get; set; } = new ItemData();
    }
}