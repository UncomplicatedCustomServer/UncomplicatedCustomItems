using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;
using MEC;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomItems.Interfaces;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Features
{
    public class SummonedCustomItem
    {
        /// <summary>
        /// Gets the list of every active SummonedCustomItem
        /// </summary>
        public static List<SummonedCustomItem> List { get; } = [];

        /// <summary>
        /// Gets the list of items that can be managed by the function <see cref="HandleCustomAction"/>
        /// </summary>
        private static readonly List<CustomItemType> _managedItems = [CustomItemType.Painkillers, CustomItemType.Medikit, CustomItemType.Adrenaline];

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
        public bool IsPickup => Pickup is not null;

        /// <summary>
        /// Create a new instance of <see cref="SummonedCustomItem"/>
        /// </summary>
        /// <param name="customItem"></param>
        /// <param name="owner"></param>
        public SummonedCustomItem(ICustomItem customItem, Player owner, Item item, Pickup pickup)
        {
            CustomItem = customItem;
            Owner = owner;
            Item = item;
            Serial = item is not null ? item.Serial : pickup.Serial;
            Pickup = pickup;
            SetProperties();
            List.Add(this);
        }

        /// <summary>
        /// Create an instance of <see cref="SummonedCustomItem"/> by choosing an existing pickup.<br></br>
        /// From now on it will be considered a <see cref="ICustomItem"/>
        /// </summary>
        /// <param name="customItem"></param>
        /// <param name="pickup"></param>
        public SummonedCustomItem(ICustomItem customItem, Pickup pickup) : this(customItem, null, null, pickup) { }

        /// <summary>
        /// Create an instance of <see cref="SummonedCustomItem"/> by spawning a new pickup.<br></br>
        /// From now on it will be considered a <see cref="ICustomItem"/>
        /// </summary>
        /// <param name="customItem"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public SummonedCustomItem(ICustomItem customItem, Vector3 position, Quaternion rotation = new()) : this(customItem, Pickup.CreateAndSpawn(customItem.Item, position, rotation)) { }

        /// <summary>
        /// Create an instance of <see cref="SummonedCustomItem"/> by spawning the item inside the player's inventory<br></br>
        /// From now on it will be considered a <see cref="ICustomItem"/>
        /// </summary>
        /// <param name="customItem"></param>
        /// <param name="owner"></param>
        public SummonedCustomItem(ICustomItem customItem, Player player) : this(customItem, player, player.AddItem(customItem.Item), null) { }

        /// <summary>
        /// Create an instance of <see cref="SummonedCustomItem"/> by choosing an item inside a player's inventory<br></br>
        /// From now on it will be considered a <see cref="ICustomItem"/>
        /// </summary>
        /// <param name="customItem"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public SummonedCustomItem(ICustomItem customItem, Player player, Item item) : this(customItem, player, item, null) { }

        /// <summary>
        /// Apply the custom properties of the current <see cref="ICustomItem"/>
        /// </summary>
        private void SetProperties()
        {
            if (Item is not null)
                switch (CustomItem.CustomItemType)
                {
                    case CustomItemType.Keycard:
                        Keycard Keycard = Item as Keycard;
                        IKeycardData KeycardData = CustomItem.CustomData as IKeycardData;

                        Keycard.Permissions = KeycardData.Permissions;
                        break;

                    case CustomItemType.Armor:
                        Armor Armor = Item as Armor;
                        IArmorData ArmorData = CustomItem.CustomData as IArmorData;

                        Armor.HelmetEfficacy = ArmorData.HeadProtection;
                        Armor.RemoveExcessOnDrop = ArmorData.RemoveExcessOnDrop;
                        Armor.StaminaUseMultiplier = ArmorData.StaminaUseMultiplier;
                        break;

                    case CustomItemType.Weapon:
                        Firearm Firearm = Item as Firearm;
                        IWeaponData WeaponData = CustomItem.CustomData as IWeaponData;

                        Firearm.Ammo = WeaponData.MaxAmmo;
                        // Firearm.MaxAmmo = WeaponData.MaxAmmo;
                        Firearm.FireRate = WeaponData.FireRate;
                        break;

                    case CustomItemType.Jailbird:
                        Jailbird Jailbird = Item as Jailbird;
                        IJailbirdData JailbirdData = CustomItem.CustomData as IJailbirdData;

                        Jailbird.TotalDamageDealt = JailbirdData.TotalDamageDealt;
                        Jailbird.TotalCharges = JailbirdData.TotalCharges;
                        Jailbird.Radius = JailbirdData.Radius;
                        Jailbird.ChargeDamage = JailbirdData.ChargeDamage;
                        Jailbird.MeleeDamage = JailbirdData.MeleeDamage;
                        Jailbird.FlashDuration = JailbirdData.FlashDuration;
                        break;

                    case CustomItemType.ExplosiveGrenade:
                        ExplosiveGrenade ExplosiveGrenade = Item as ExplosiveGrenade;
                        IExplosiveGrenadeData ExplosiveGrenadeData = CustomItem.CustomData as IExplosiveGrenadeData;

                        ExplosiveGrenade.MaxRadius = ExplosiveGrenadeData.MaxRadius;
                        ExplosiveGrenade.PinPullTime = ExplosiveGrenadeData.PinPullTime;
                        ExplosiveGrenade.ScpDamageMultiplier = ExplosiveGrenadeData.ScpDamageMultiplier;
                        ExplosiveGrenade.ConcussDuration = ExplosiveGrenadeData.ConcussDuration;
                        ExplosiveGrenade.BurnDuration = ExplosiveGrenadeData.BurnDuration;
                        ExplosiveGrenade.DeafenDuration = ExplosiveGrenadeData.DeafenDuration;
                        ExplosiveGrenade.FuseTime = ExplosiveGrenadeData.FuseTime;
                        ExplosiveGrenade.Repickable = ExplosiveGrenadeData.Repickable;
                        break;

                    case CustomItemType.FlashGrenade:
                        FlashGrenade FlashGrenade = Item as FlashGrenade;
                        IFlashGrenadeData FlashGrenadeData = CustomItem.CustomData as IFlashGrenadeData;

                        FlashGrenade.PinPullTime = FlashGrenadeData.PinPullTime;
                        FlashGrenade.Repickable = FlashGrenadeData.Repickable;
                        FlashGrenade.MinimalDurationEffect = FlashGrenadeData.MinimalDurationEffect;
                        FlashGrenade.AdditionalBlindedEffect = FlashGrenadeData.AdditionalBlindedEffect;
                        FlashGrenade.SurfaceDistanceIntensifier = FlashGrenadeData.SurfaceDistanceIntensifier;
                        FlashGrenade.FuseTime = FlashGrenadeData.FuseTime;
                        break;

                    default:
                        break;
                }
            else if (Pickup is not null)
            {
                Pickup.Scale = CustomItem.Scale;
                Pickup.Weight = CustomItem.Weight;
            }
        }

        internal void OnPickup(ItemAddedEventArgs pickedUp)
        {
            Pickup = null;
            Item = pickedUp.Item;
            Owner = pickedUp.Player;
            SetProperties();
            Serial = Item.Serial;
            HandleEvent(pickedUp.Player, ItemEvents.Pickup);
        }

        internal void OnDrop(DroppedItemEventArgs dropped)
        {
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
                    if (!Data.Command.Contains("P:"))
                        Server.ExecuteCommand(Data.Command.Replace("%id%", player.Id.ToString())); 
                    else
                        Server.ExecuteCommand(Data.Command.Replace("%id%", player.Id.ToString()).Replace("P:", ""), player.Sender);

                Utilities.ParseResponse(player, Data);

                // Now we can destry the item if we have been told to do it
                if (Data.DestroyAfterUse)
                    Destroy();
            }
        }

        internal void HandleSelectedDisplayHint()
        {
            if (Plugin.Instance.Config.SelectedMessage.Length > 1)
                Owner.ShowHint(Plugin.Instance.Config.SelectedMessage.Replace("%name%", CustomItem.Name).Replace("%desc%", CustomItem.Description).Replace("%description%", CustomItem.Description), Plugin.Instance.Config.SelectedMessageDuration);
        }

        internal void HandlePickedUpDisplayHint()
        {
            if (Plugin.Instance.Config.PickedUpMessage.Length > 1)
                Owner.ShowHint(Plugin.Instance.Config.PickedUpMessage.Replace("%name%", CustomItem.Name).Replace("%desc%", CustomItem.Description).Replace("%description%", CustomItem.Description), Plugin.Instance.Config.PickedUpMessageDuration);
        }

        internal bool HandleCustomAction()
        {
            if (Owner is null) 
                return false;

            if (_managedItems.Contains(CustomItem.CustomItemType))
            {
                switch (CustomItem.CustomItemType)
                {
                    case CustomItemType.Medikit:
                        IMedikitData MedikitData = CustomItem.CustomData as IMedikitData;
                        Owner.Heal(MedikitData.Health, MedikitData.MoreThanMax);
                        break;
                    case CustomItemType.Painkillers:
                        Timing.RunCoroutine(Utilities.PainkillersCoroutine(Owner, CustomItem.CustomData as IPainkillersData));
                        break;
                    case CustomItemType.Adrenaline:
                        IAdrenalineData AdrenalineData = CustomItem.CustomData as IAdrenalineData;
                        Owner.AddAhp(AdrenalineData.Amount, decay: AdrenalineData.Decay, efficacy: AdrenalineData.Efficacy, sustain: AdrenalineData.Sustain, persistant: AdrenalineData.Persistant);
                        break;
                    default:
                        return false;
                }

                return true;
            }

            return false;
        }

        public void Destroy()
        {
            List.Remove(this);
            if (IsPickup)
                Pickup.Destroy();
            else
                Item.Destroy();

            Pickup = null;
            Item = null;
            Serial = 0;
            Owner = null;
            CustomItem = null;
        }

        /// <summary>
        /// Gets a <see cref="List{SummonedCustomItem}"/> of every <see cref="SummonedCustomItem"/> with the given <see cref="ItemType"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static List<SummonedCustomItem> Get(ItemType item) => List.Where(sci => sci.CustomItem.Item == item).ToList();

        /// <summary>
        /// Gets a <see cref="List{SummonedCustomItem}"/> of every <see cref="SummonedCustomItem"/> with the given owner
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static List<SummonedCustomItem> Get(Player owner) => List.Where(sci => sci.Owner?.Id == owner.Id).ToList();

        /// <summary>
        /// Gets a <see cref="SummonedCustomItem"/> by it's owner and it's serial.<br></br>
        /// It can't be a pickup!
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="serial"></param>
        /// <returns></returns>
        public static SummonedCustomItem Get(Player owner, ushort serial) => List.Where(sci => sci.Owner is not null && sci.Owner.Id ==  owner.Id && sci.Serial == serial).FirstOrDefault();

        /// <summary>
        /// Gets a <see cref="SummonedCustomItem"/> by it's serial.
        /// </summary>
        /// <param name="serial"></param>
        /// <returns></returns>
        public static SummonedCustomItem Get(ushort serial) => List.Where(sci => sci.Serial == serial).FirstOrDefault();

        /// <summary>
        /// Try gets a <see cref="SummonedCustomItem"/> by it's owner and it's serial.<br></br>
        /// It can't be a pickup!
        /// </summary>
        /// <param name="serial"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool TryGet(ushort serial, out SummonedCustomItem item)
        {
            item = Get(serial);
            return item != null;
        }

        /// <summary>
        /// Try gets a <see cref="SummonedCustomItem"/> by it's serial.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="serial"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool TryGet(Player player, ushort serial, out SummonedCustomItem item)
        {
            item = Get(player, serial);
            return item != null;
        }
    }
}
