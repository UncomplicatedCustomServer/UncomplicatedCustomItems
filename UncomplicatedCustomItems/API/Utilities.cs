using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using System.Linq;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Elements.SpecificData;
using UncomplicatedCustomItems.Interfaces;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using ExiledItem = Exiled.API.Features.Items.Item;

namespace UncomplicatedCustomItems.API
{
    public static class Utilities
    {
        /// <summary>
        /// Check if a <see cref="ICustomItem"/> is valid and can be registered
        /// </summary>
        /// <param name="item"></param>
        /// <param name="error"></param>
        /// <returns><see cref="false"/> if there's any problem. Every error will be outputted with <paramref name="error"/></returns>
        public static bool CustomItemValidator(ICustomItem item, out string error)
        {
            if (Manager.Items.ContainsKey(item.Id))
            {
                error = $"There's already another ICustomItem registered with the same Id ({item.Id})!";
                return false;
            }

            
            switch (item.CustomItemType)
            {
                case CustomItemType.Item:
                    if (item.CustomData is not IItemData)
                    {
                        error = $"The item has been flagged as 'Item' but the CustomData class is not 'IItemData', found '{item.CustomData.GetType().Name}'";
                        return false;
                    }

                    break;

                case CustomItemType.Weapon:
                    if (item.CustomData is not IWeaponData)
                    {
                        error = $"The item has been flagged as 'Weapon' but the CustomData class is not 'IWeaponData', found '{item.CustomData.GetType().Name}'";
                        return false;
                    }

                    if (!item.Item.IsWeapon())
                    {
                        error = $"The item has been flagged as 'Weapon' but the item {item.Item} is not a weapon in the game!";
                        return false;
                    }

                    break;

                case CustomItemType.Keycard:
                    if (item.CustomData is not IKeycardData)
                    {
                        error = $"The item has been flagged as 'Keycard' but the CustomData class is not 'IKeycardData', found '{item.CustomData.GetType().Name}'";
                        return false;
                    }

                    if (!item.Item.IsKeycard())
                    {
                        error = $"The item has been flagged as 'Keycard' but the item {item.Item} is not a keycard in the game!";
                        return false;
                    }

                    break;

                case CustomItemType.Armor:
                    if (item.CustomData is not IArmorData)
                    {
                        error = $"The item has been flagged as 'Armor' but the CustomData class is not 'IArmorData', found '{item.CustomData.GetType().Name}'";
                        return false;
                    }

                    if (!item.Item.IsArmor())
                    {
                        error = $"The item has been flagged as 'Armor' but the item {item.Item} is not a armor in the game!";
                        return false;
                    }

                    break;

                default:
                    error = "Unknown error? Uhm please report it on our discord server!";
                    return false;
            }

            error = "";
            return true;
        }

        /// <summary>
        /// Check if a <see cref="ICustomItem"/> is valid and can be registered.
        /// Does not return the error as text!
        /// </summary>
        /// <param name="item"></param>
        /// <returns><see cref="false"/> if there's any problem.</returns>
        public static bool CustomItemValidator(ICustomItem item)
        {
            return CustomItemValidator(item, out _);
        }

        /// <summary>
        /// Parse a <see cref="IResponse"/> object as response to a player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="response"></param>
        public static void ParseResponse(Player player, IItemData response)
        {
            if (response.ConsoleMessage is not null && response.ConsoleMessage.Length > 1) // FUCK 1 char messages!
            {
                player.SendConsoleMessage(response.ConsoleMessage, string.Empty);
            }

            if (response.BroadcastMessage.Length > 1 && response.BroadcastDuration > 0)
            {
                player.Broadcast(response.BroadcastDuration, response.BroadcastMessage);
            }

            if (response.HintMessage.Length > 1 && response.HintDuration > 0)
            {
                player.ShowHint(response.HintMessage, response.HintDuration);
            }
        }

        /// <summary>
        /// Try to get a <see cref="SummonedCustomItem"/> by it's serial
        /// </summary>
        /// <param name="serial"></param>
        /// <param name="item"></param>
        /// <returns><see cref="true"/> if succeeded</returns>
        public static bool TryGetSummonedCustomItem(ushort serial, out SummonedCustomItem item)
        {
            item = Manager.SummonedItems.Where(item => item.Serial == serial).FirstOrDefault();
            if (item == default)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get a <see cref="SummonedCustomItem"/> by it's serial
        /// </summary>
        /// <param name="serial"></param>
        /// <returns><see cref="SummonedCustomItem"/> if succeeded, <see cref="default"/> if not</returns>
        public static SummonedCustomItem GetSummonedCustomItem(ushort serial)
        {
            if (!TryGetSummonedCustomItem(serial, out SummonedCustomItem Item))
            {
                return default;
            }
            return Item;
        }

        /// <summary>
        /// Check if an item is a <see cref="SummonedCustomItem"/> by it's serial
        /// </summary>
        /// <param name="serial"></param>
        /// <returns><see cref="true"/> if it is</returns>
        public static bool IsSummonedCustomItem(ushort serial)
        {
            return Manager.SummonedItems.Where(item => item.Serial == serial).Count() > 0;
        }

        /// <summary>
        /// Try to get a <see cref="ICustomItem"/> by it's Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <returns><see cref="true"/> if the item exists and <paramref name="item"/> is not <see cref="null"/> or <see cref="default"/></returns>
        public static bool TryGetCustomItem(uint id, out ICustomItem item)
        {
            item = default;
            if (Manager.Items.ContainsKey(id))
            {
                item = Manager.Items[id];
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get a <see cref="ICustomItem"/> by it's Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns><see cref="ICustomItem"/> if it exists, otherwhise a <see cref="default"/> will be returned</returns>
        public static ICustomItem GetCustomItem(uint id)
        {
            TryGetCustomItem(id, out ICustomItem Item);
            return Item;
        }

        /// <summary>
        /// Check if the given Id is already registered as a <see cref="ICustomItem"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsCustomItem(uint id)
        {
            return Manager.Items.ContainsKey(id);
        }
    }
}
