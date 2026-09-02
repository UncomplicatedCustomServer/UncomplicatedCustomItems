using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features.Manager;

namespace UncomplicatedCustomItems.API.Features
{
    public class CustomAction
    {
        /// <summary>
        /// Gets a list of every registered <see cref="CustomAction"/>
        /// </summary>
        public static List<CustomAction> List => CustomActions.Values.ToList();

        /// <summary>
        /// Gets a list of every unregistered <see cref="CustomAction"/>
        /// </summary>
        public static List<CustomAction> UnregisteredList => UnregisteredCustomActions.Values.ToList();

        internal static Dictionary<uint, CustomAction> CustomActions { get; set; } = [];
        internal static Dictionary<uint, CustomAction> UnregisteredCustomActions { get; set; } = [];

        public static void Register(CustomAction action)
        {
            if (!Utilities.CustomActionValidator(action, out string error))
            {
                LogManager.Warn($"Unable to register the CustomAction with the Id {action.Id} and name '{action.Name}':\n{error}\nError code: 0x029");
                UnregisteredCustomActions.TryAdd(action.Id, action);
                return;
            }
            
            CustomActions.TryAdd(action.Id, action);
            LogManager.Info($"Successfully registered CustomAction '{action.Name}' (Id: {action.Id}) into the plugin!");
        }

        /// <summary>
        /// Unregister a <see cref="CustomAction"/> from the plugin by its class
        /// </summary>
        /// <param name="action"></param>
        public static void Unregister(CustomAction action) => Unregister(action.Id);

        /// <summary>
        /// Unregister a <see cref="CustomAction"/> from the plugin by its Id
        /// </summary>
        /// <param name="action"></param>
        public static void Unregister(uint action)
        {
            if (CustomActions.ContainsKey(action) || UnregisteredCustomActions.ContainsKey(action))
            {
                CustomActions.Remove(action);
                UnregisteredCustomActions.Remove(action);
            }
        }

        public static uint GetFirstFreeId(uint from = 0)
        {
            for (uint i = from; i < uint.MaxValue; i++)
                if (!CustomActions.ContainsKey(i))
                    return i;

            return 0;
        }

        public uint Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string[] Actions { get; set; } = [];
    }
}