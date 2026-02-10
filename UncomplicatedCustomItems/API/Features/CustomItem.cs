using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UncomplicatedCustomItems.API.Interfaces;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using UnityEngine;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Extensions;
using System.Text;
using UncomplicatedCustomItems.API.CustomModuleAPI;

namespace UncomplicatedCustomItems.API.Features
{
    public class CustomItem : ICustomItem
    {
        #nullable enable
        /// <summary>
        /// Gets a list of every registered <see cref="ICustomItem"/>
        /// </summary>
        public static List<ICustomItem> List => CustomItems.Values.ToList();

        /// <summary>
        /// Gets a list of every unregistered <see cref="ICustomItem"/>
        /// </summary>
        public static List<ICustomItem> UnregisteredList => UnregisteredCustomItems.Values.ToList();

        internal static List<ErrorCustomItem> ErrorCustomItems { get; set; } = [];

        internal static Dictionary<uint, ICustomItem> CustomItems { get; set; } = new();
        internal static Dictionary<uint, ICustomItem> UnregisteredCustomItems { get; set; } = new();

        /// <summary>
        /// Register a new <see cref="ICustomItem"/> inside the plugin
        /// </summary>
        /// <param name="item"></param>
        public static void Register(ICustomItem item)
        {
            if (!Utilities.CustomItemValidator(item, out string error))
            {
                LogManager.Warn($"Unable to register the ICustomItem with the Id {item.Id} and name '{item.Name}':\n{error}\nError code: 0x029");
                UnregisteredCustomItems.TryAdd(item.Id, item);
                return;
            }
            CustomItems.TryAdd(item.Id, item);
            LogManager.Info($"Successfully registered ICustomItem '{item.Name}' (Id: {item.Id}) into the plugin!");
        }

        public bool HasModule<T>() where T : CustomModuleBase =>
            CustomModules.OfType<T>().FirstOrDefault() != null;

        public bool TryGetModule<T>(out T? module) where T : CustomModuleBase
        {
            module = CustomModules.OfType<T>().FirstOrDefault();
            return module != null;
        }

        /// <summary>
        /// Unregister a <see cref="ICustomItem"/> from the plugin by its class
        /// </summary>
        /// <param name="item"></param>
        public static void Unregister(ICustomItem item) => Unregister(item.Id);

        /// <summary>
        /// Unregister a <see cref="ICustomItem"/> from the plugin by its Id
        /// </summary>
        /// <param name="item"></param>
        public static void Unregister(uint item)
        {
            if (CustomItems.ContainsKey(item) || UnregisteredCustomItems.ContainsKey(item))
            {
                CustomItems.Remove(item);
                UnregisteredCustomItems.Remove(item);
            }
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
        /// The unique Id of the Custom Item. Can't be less than 1
        /// </summary>
        public virtual uint Id { get; set; } = 1;

        /// <summary>
        /// The Name of the object. Can appears when for example you pick it up
        /// </summary>
        [Description("The name of the custom item")]
        public virtual string Name { get; set; } = "Detonator";

        /// <summary>
        /// The description. Useful at the moment ig
        /// </summary>
        [Description("The description of the custom item")]
        public virtual string Description { get; set; } = "25/06/2024";

        [Description("The extended description of the custom item. Used by the `.customiteminfo` command")]
        public virtual string ExtendedDescription { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the badge name
        /// </summary>
        [Description("Sets the badge name")]
        public virtual string BadgeName { get; set; } = "Uncomplicated Custom Items";

        /// <summary>
        /// Gets or sets the badge color
        /// </summary>
        [Description("Sets the badge color. This uses the badge colors available for server")]
        public virtual string BadgeColor { get; set; } = "pumpkin";

        /// <su/mmary>
        /// The weight of the item
        /// </summary>
        [Description("The weight of the custom item")]
        public virtual float Weight { get; set; } = 2f;

        /// <summary>
        /// Whether if the item won't be removed from the player's inventory
        /// </summary>
        [Description("Whether if the item won't be removed from the player's inventory after being used. Available only for Consumable items!")]
        public virtual bool Reusable { get; set; } = false;

        /// <summary>
        /// The <see cref="ItemType"/> (Base) of the Custom Item
        /// </summary>
        [Description("The Item base for the custom item")]
        public virtual ItemType Item { get; set; } = ItemType.Coin;

        /// <summary>
        /// The Scale of the Custom Item. If 0, 0, 0 then it's disabled
        /// </summary>
        [Description("The scale of the custom item, 0 0 0 means disabled")]
        public virtual Vector3 Scale { get; set; } = Vector3.one;

        /// <summary>
        /// The Spawn data for the custom item.
        /// </summary>
        public virtual ISpawn Spawn { get; set; } = new Spawn();

        /// <summary>
        /// Custom flags of the item
        /// </summary>
        [Description("Custom flags for the item")]
        public virtual CustomFlags? CustomFlags { get; set; } = new();

        public virtual Dictionary<CustomModuleBase, List<object>> CustomModules { get; set; } = [];

        public virtual Dictionary<ArgumentType, string> Arguments { get; set; } = [];

        /// <summary>
        /// The <see cref="CustomItemType"/> of the Custom Item
        /// </summary>
        public virtual CustomItemType CustomItemType { get; set; } = CustomItemType.Item;

        /// <summary>
        /// The <see cref="IData">Custom Data</see>, based on the CustomItemType
        /// </summary>
        public virtual IData CustomData { get; set; } = new ItemData();

        public override string ToString()
        {
            StringBuilder sb = new();
            
            sb.AppendLine($"CustomItem [{Name}]");
            sb.AppendLine($"  Id: {Id}");
            sb.AppendLine($"  Description: {Description}");
            
            if (!string.IsNullOrEmpty(ExtendedDescription))
                sb.AppendLine($"  Extended Description: {ExtendedDescription}");
            
            sb.AppendLine($"  Item Type: {Item}");
            sb.AppendLine($"  Custom Item Type: {CustomItemType}");
            sb.AppendLine($"  Weight: {Weight}");
            sb.AppendLine($"  Scale: {Scale}");
            sb.AppendLine($"  Reusable: {Reusable}");
            sb.AppendLine($"  Badge: {BadgeName} ({BadgeColor})");
            
            if (CustomFlags.HasValue)
                sb.AppendLine($"  Custom Flags: {CustomFlags.Value}");
            
            if (Spawn != null)
                sb.AppendLine($"  Spawn: {Spawn}");
            
            if (CustomData != null)
                sb.AppendLine($"  Custom Data: {CustomData.GetType().Name}");
            
            if (Arguments != null && Arguments.Count > 0)
                sb.AppendLine($"  Arguments: {Arguments.Count} defined");
            
            return sb.ToString().TrimEnd();
        }
    }
}