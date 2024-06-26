﻿using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;
using PluginAPI.Commands;
using System.Runtime.Remoting.Messaging;
using UncomplicatedCustomItems.Interfaces;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Features
{
    public class SummonedCustomItem
    {
        /// <summary>
        /// The <see cref="ICustomItem"/> reference of the item
        /// </summary>
        public ICustomItem CustomItem { get; internal set; }

        /// <summary>
        /// The <see cref="Player">Owner</see> of the item
        /// </summary>
        public Player Owner { get; internal set; }

        /// <summary>
        /// The <see cref="SummonedCustomItem"/> as an <see cref="Exiled.API.Features.Items.Item"/>
        /// </summary>
        public Item Item { get; internal set; }

        /// <summary>
        /// The <see cref="SummonedCustomItem"/> as a <see cref="Exiled.API.Features.Pickups.Pickup"/>.
        /// If this is not <see cref="null"/> then <see cref="Owner"/> and <see cref="Item"/> will be <see cref="null"/>
        /// </summary>
        public Pickup Pickup { get; internal set; }

        /// <summary>
        /// The serial of the item or pickup, used for identification
        /// </summary>
        public ushort Serial { get; internal set; }

        /// <summary>
        /// Check if this item is a pickup
        /// </summary>
        public bool IsPickup { get; internal set; }

        /// <summary>
        /// Summon the <see cref="ICustomItem"/> inside the inventory of a player
        /// </summary>
        /// <param name="customItem"></param>
        /// <param name="owner"></param>
        public SummonedCustomItem(ICustomItem customItem, Player owner)
        {
            CustomItem = customItem;
            Owner = owner;
            Item = Item.Create(customItem.Item);
            owner.AddItem(Item);
            Serial = Item.Serial;
            IsPickup = false;
            Pickup = null;
            SetProperties();
        }

        /// <summary>
        /// Summon the <see cref="ICustomItem"/> as an existing pickup
        /// </summary>
        /// <param name="customItem"></param>
        /// <param name="pickup"></param>
        public SummonedCustomItem(ICustomItem customItem, Pickup pickup)
        {
            CustomItem = customItem;
            Owner = null;
            Pickup = pickup;
            Serial = Pickup.Serial;
            IsPickup = true;
            Item = null;
            SetProperties();
        }

        /// <summary>
        /// Summon the <see cref="ICustomItem"/> as a new pickup
        /// </summary>
        /// <param name="customItem"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public SummonedCustomItem(ICustomItem customItem, Vector3 position, Quaternion rotation = new())
        {
            CustomItem = customItem;
            Owner = null;
            Item = null;
            Pickup = Pickup.CreateAndSpawn(customItem.Item, position, rotation);
            IsPickup = true;
            Serial = Pickup.Serial;
            SetProperties();
        }

        /// <summary>
        /// Summon the <see cref="ICustomItem"/> inside the inventory of a player
        /// </summary>
        /// <param name="customItem"></param>
        /// <param name="owner"></param>
        /// <returns>The <see cref="SummonedCustomItem"/> class of the summoned item</returns>
        public static SummonedCustomItem Summon(ICustomItem customItem, Player owner)
        {
            SummonedCustomItem Item = new(customItem, owner);
            Manager.SummonedItems.Add(Item);
            return Item;
        }

        public static SummonedCustomItem Summon(ICustomItem customItem, Vector3 position, Quaternion rotation = new())
        {
            SummonedCustomItem Item = new(customItem, position, rotation);
            Manager.SummonedItems.Add(Item);
            return Item;
        }

        internal void SetProperties()
        {
            if (Item is not null)
            {
                switch (CustomItem.CustomItemType)
                {
                    case CustomItemType.Item:
                        break;

                    case CustomItemType.Keycard:
                        ((Keycard)Item).Permissions = ((IKeycardData)CustomItem.CustomData).Permissions;
                        break;

                    case CustomItemType.Armor:
                        ((Armor)Item).HelmetEfficacy = ((IArmorData)CustomItem.CustomData).HeadProtection;
                        ((Armor)Item).RemoveExcessOnDrop = ((IArmorData)CustomItem.CustomData).RemoveExcessOnDrop;
                        ((Armor)Item).StaminaUseMultiplier = ((IArmorData)CustomItem.CustomData).StaminaUseMultiplier;
                        break;

                    case CustomItemType.Weapon:
                        Log.Debug("Updating weapon data");
                        ((Firearm)Item).Ammo = ((IWeaponData)CustomItem.CustomData).MaxAmmo;
                        ((Firearm)Item).MaxAmmo = ((IWeaponData)CustomItem.CustomData).MaxAmmo;
                        ((Firearm)Item).FireRate = ((IWeaponData)CustomItem.CustomData).FireRate;
                        break;

                    default:
                        break;
                }
            }
            else if (Pickup is not null)
            {
                Pickup.Scale = CustomItem.Scale;
                Pickup.Weight = CustomItem.Weight;
            }
        }

        internal void OnPickup(ItemAddedEventArgs pickedUp)
        {
            IsPickup = false;
            Pickup = null;
            Item = pickedUp.Item;
            Owner = pickedUp.Player;
            SetProperties();
            Serial = Item.Serial;
            HandleEvent(pickedUp.Player, ItemEvents.Pickup);
        }

        internal void OnDrop(DroppedItemEventArgs dropped)
        {
            IsPickup = true;
            Pickup = dropped.Pickup;
            Item = null;
            Owner = null;
            SetProperties();
            Serial = Pickup.Serial;
            HandleEvent(dropped.Player, ItemEvents.Drop);
        }

        internal void HandleEvent(Player player, ItemEvents itemEvent) 
        {
            if (CustomItem.CustomItemType == CustomItemType.Item && CustomItem.CustomData is ICustomItem && ((IItemData)CustomItem.CustomData).Event == itemEvent)
            {
                IItemData Data = CustomItem.CustomData as IItemData;
                Log.Debug($"Firing events for item {CustomItem.Name}");
                if (Data.Command is not null && Data.Command.Length > 2)
                {
                    if (!Data.Command.Contains("P:"))
                    {
                        Server.ExecuteCommand(Data.Command.Replace("%id%", player.Id.ToString()));
                    } 
                    else
                    {
                        Server.ExecuteCommand(Data.Command.Replace("%id%", player.Id.ToString()).Replace("P:", ""), player.Sender);
                    }
                }

                Utilities.ParseResponse(player, Data);
            }
        }

        internal void HandleSelectedDisplayHint()
        {
            if (Plugin.Instance.Config.SelectedMessage.Length > 1)
            {
                Owner.ShowHint(Plugin.Instance.Config.SelectedMessage.Replace("%name%", CustomItem.Name).Replace("%desc%", CustomItem.Description).Replace("%description%", CustomItem.Description), Plugin.Instance.Config.SelectedMessageDuration);
            }
        }

        internal void HandlePickedUpDisplayHint()
        {
            if (Plugin.Instance.Config.PickedUpMessage.Length > 1)
            {
                Owner.ShowHint(Plugin.Instance.Config.PickedUpMessage.Replace("%name%", CustomItem.Name).Replace("%desc%", CustomItem.Description).Replace("%description%", CustomItem.Description), Plugin.Instance.Config.PickedUpMessageDuration);
            }
        }

        public void Destroy()
        {
            Manager.SummonedItems.Remove(this);
            if (IsPickup)
            {
                Pickup.Destroy();
            } 
            else
            {
                Item.Destroy();
            }
            Pickup = null;
            Item = null;
            Serial = 0;
            Owner = null;
            CustomItem = null;
        }
    }
}
