using Exiled.API.Features;
using Exiled.API.Features.Items;
using System.Linq;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Interfaces;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using ExiledItem = Exiled.API.Features.Items.Item;

namespace UncomplicatedCustomItems.API
{
    public class Utilities
    {
        /// <summary>
        /// Check if a <see cref="ICustomItem"/> is valid and can be registered
        /// </summary>
        /// <param name="Item"></param>
        /// <param name="Error"></param>
        /// <returns><see cref="false"/> if there's any problem. Every error will be outputted with <paramref name="Error"/></returns>
        public static bool CustomItemValidator(ICustomItem Item, out string Error)
        {
            if (Manager.Items.ContainsKey(Item.Id))
            {
                Error = $"There's already another ICustomItem registered with the same Id ({Item.Id})!";
                return false;
            }

            switch (Item.CustomItemType)
            {
                case CustomItemType.Item:
                    if (Item.CustomData is not IItemData)
                    {
                        Error = "The item has been flagged as 'Item' but the CustomData class is not 'IItemData'";
                        return false;
                    }

                    break;

                case CustomItemType.Weapon:
                    if (Item.CustomData is not IWeaponData)
                    {
                        Error = "The item has been flagged as 'Weapon' but the CustomData class is not 'IWeaponData'";
                        return false;
                    }

                    if (ExiledItem.Create(Item.Item) is not Firearm)
                    {
                        Error = $"The item has been flagged as 'Weapon' but the item {Item.Item} is not a weapon in the game!";
                        return false;
                    }

                    break;

                case CustomItemType.Keycard:
                    if (Item.CustomData is not IKeycardData)
                    {
                        Error = "The item has been flagged as 'Keycard' but the CustomData class is not 'IKeycardData'";
                        return false;
                    }

                    if (ExiledItem.Create(Item.Item) is not Keycard)
                    {
                        Error = $"The item has been flagged as 'Keycard' but the item {Item.Item} is not a keycard in the game!";
                        return false;
                    }

                    break;

                case CustomItemType.Armor:
                    if (Item.CustomData is not IArmorData)
                    {
                        Error = "The item has been flagged as 'Armor' but the CustomData class is not 'IArmorData'";
                        return false;
                    }

                    if (ExiledItem.Create(Item.Item) is not Armor)
                    {
                        Error = $"The item has been flagged as 'Armor' but the item {Item.Item} is not a armor in the game!";
                        return false;
                    }

                    break;

                default:
                    Error = "Unknown error? Uhm please report it on our discord server!";
                    return false;
            }

            Error = "";
            return true;
        }

        /// <summary>
        /// Check if a <see cref="ICustomItem"/> is valid and can be registered.
        /// Does not return the error as text!
        /// </summary>
        /// <param name="Item"></param>
        /// <returns><see cref="false"/> if there's any problem.</returns>
        public bool CustomItemValidator(ICustomItem Item)
        {
            return CustomItemValidator(Item, out _);
        }

        /// <summary>
        /// Parse a <see cref="IResponse"/> object as response to a player
        /// </summary>
        /// <param name="Player"></param>
        /// <param name="Response"></param>
        public static void ParseResponse(Player Player, IResponse Response)
        {
            if (Response.ConsoleMessage is not null && Response.ConsoleMessage.Length > 1) // FUCK 1 char messages!
            {
                Player.SendConsoleMessage(Response.ConsoleMessage, string.Empty);
            }

            if (Response.BroadcastMessage.Length > 1 && Response.BroadcastDuration > 0)
            {
                Player.Broadcast(Response.BroadcastDuration, Response.BroadcastMessage);
            }

            if (Response.HintMessage.Length > 1 && Response.HintDuration > 0)
            {
                Player.ShowHint(Response.HintMessage, Response.HintDuration);
            }
        }

        /// <summary>
        /// Try to get a <see cref="SummonedCustomItem"/> by it's serial
        /// </summary>
        /// <param name="Serial"></param>
        /// <param name="Item"></param>
        /// <returns><see cref="true"/> if succeeded</returns>
        public static bool TryGetSummonedCustomItem(ushort Serial, out SummonedCustomItem Item)
        {
            Item = Manager.SummonedItems.Where(item => item.Serial == Serial).FirstOrDefault();
            if (Item == default)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get a <see cref="SummonedCustomItem"/> by it's serial
        /// </summary>
        /// <param name="Serial"></param>
        /// <returns><see cref="SummonedCustomItem"/> if succeeded, <see cref="default"/> if not</returns>
        public static SummonedCustomItem GetSummonedCustomItem(ushort Serial)
        {
            if (!TryGetSummonedCustomItem(Serial, out SummonedCustomItem Item))
            {
                return default;
            }
            return Item;
        }

        /// <summary>
        /// Check if an item is a <see cref="SummonedCustomItem"/> by it's serial
        /// </summary>
        /// <param name="Serial"></param>
        /// <returns><see cref="true"/> if it is</returns>
        public static bool IsSummonedCustomItem(ushort Serial)
        {
            return Manager.SummonedItems.Where(item => item.Serial == Serial).Count() > 0;
        }

        /// <summary>
        /// Summon a <see cref="ICustomItem"/> as a <see cref="SummonedCustomItem"/>
        /// </summary>
        /// <param name="Item"></param>
        /// <param name="Player"></param>
        /// <returns>The <see cref="SummonedCustomItem"/> class</returns>
        public static SummonedCustomItem Summon(ICustomItem Item, Player Player)
        {
            SummonedCustomItem SummonedCustomItem = SummonedCustomItem.Summon(Item, Player);
            Manager.SummonedItems.Add(SummonedCustomItem);
            return SummonedCustomItem;
        }
    }
}
