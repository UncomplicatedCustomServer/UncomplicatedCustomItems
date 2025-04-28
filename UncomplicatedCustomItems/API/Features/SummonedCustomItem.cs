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
using UncomplicatedCustomItems.API.Struct;
using UncomplicatedCustomItems.API.Features.Helper;
using System;
using UncomplicatedCustomItems.Enums;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Keycards;
using Exiled.API.Enums;
using Interactables.Interobjects.DoorUtils;

namespace UncomplicatedCustomItems.API.Features
{
    /// <summary>
    /// Handles the information and methods for every summoned <see cref="ICustomItem"/>
    /// </summary>
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
        /// Converts the attachments custom weapon data to a list so it applies all attachments instead of one
        /// </summary>
        public static List<string> GetAttachmentsList(List<IWeaponData> items)
        {
            return items
                .Where(item => !string.IsNullOrWhiteSpace(item.Attachments))
                .SelectMany(item => item.Attachments.Split(',')
                    .Select(attachment => new { AttachmentName = attachment.Trim() }))
                .Select(x => x.AttachmentName)
                .ToList();
        }
        /// <summary>
        /// Converts the Command custom data from items into a list to allow multiple commands.
        /// </summary>
        public static List<string?> CommandsList(List<IItemData> Commands)
        {
            return Commands
                .Where(item => !string.IsNullOrWhiteSpace(item.Command))
                .SelectMany(item => item.Command.Split(',')
                    .Select(Commands => new { Commands = Commands.Trim() }))
                .Select(x => x.Commands)
                .ToList();
        }

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
        /// <param name="item"></param>
        /// <param name="pickup"></param>
        public SummonedCustomItem(ICustomItem customItem, Player owner, Item item, Pickup pickup)
        {
            CustomItem = customItem;
            Owner = owner;
            Item = item;
            Serial = item is not null ? item.Serial : pickup.Serial;
            Pickup = pickup;
            SetProperties(owner);
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
        /// <param name="player"></param>
        public SummonedCustomItem(ICustomItem customItem, Player player) : this(customItem, player, player.AddItem(customItem.Item), null) { }

        /// <summary>
        /// Create an instance of <see cref="SummonedCustomItem"/> by choosing an item inside a player's inventory<br></br>
        /// From now on it will be considered a <see cref="ICustomItem"/>
        /// </summary>
        /// <param name="customItem"></param>
        /// <param name="player"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public SummonedCustomItem(ICustomItem customItem, Player player, Item item) : this(customItem, player, item, null) { }

        private int Charges { get; set; }

        /// <summary>
        /// Applies the custom properties of the current <see cref="ICustomItem"/>
        /// </summary>
        public void SetProperties(Player player)
        {
            if (Item is not null)
                switch (CustomItem.CustomItemType)
                {
                    case CustomItemType.Keycard:
                        Keycard keycard = Item as Keycard;
                        IKeycardData KeycardData = CustomItem.CustomData as IKeycardData;
                        Color PermissionsColor = GetColor(KeycardData.PermissionsColor);
                        Color TintColor = GetColor(KeycardData.TintColor);
                        KeycardGfx keycardGfx = keycard.Base.KeycardGfx;
                        keycardGfx.SetTint(TintColor);
                        KeycardLevels permissions = new(KeycardData.Containment, KeycardData.Armory, KeycardData.Admin);
                        keycardGfx.SetPermissions(permissions, PermissionsColor);
                        keycardGfx.ExtraWeight = CustomItem.Weight;
                        keycardGfx.KeycardLabels[0].text = $"{KeycardData.Label}";
                        keycardGfx.NameFields[0].text = $"{KeycardData.Name.Replace("%name%", player.DisplayNickname)}";
                        DetailBase detailBase = keycard.Base.Details[0];
                        detailBase.ApplyDetail(keycardGfx, keycard.Base);
                        break;

                    case CustomItemType.Armor:
                        Armor Armor = Item as Armor;
                        IArmorData ArmorData = CustomItem.CustomData as IArmorData;

                        Armor.HelmetEfficacy = ArmorData.HeadProtection;
                        Armor.VestEfficacy = ArmorData.BodyProtection;
                        Armor.StaminaUseMultiplier = ArmorData.StaminaUseMultiplier;
                        Armor.StaminaRegenMultiplier = ArmorData.StaminaRegenMultiplier;
                        break;

                    case CustomItemType.Weapon:
                        Firearm Firearm = Item as Firearm;
                        IWeaponData WeaponData = CustomItem.CustomData as IWeaponData;

                        Firearm.MagazineAmmo = WeaponData.MaxAmmo;
                        Firearm.Damage = WeaponData.Damage;
                        Firearm.MaxMagazineAmmo = WeaponData.MaxMagazineAmmo;
                        Firearm.MaxBarrelAmmo = WeaponData.MaxBarrelAmmo;
                        Firearm.AmmoDrain = WeaponData.AmmoDrain;
                        Firearm.Penetration = WeaponData.Penetration;
                        Firearm.Inaccuracy = WeaponData.Inaccuracy;
                        Firearm.DamageFalloffDistance = WeaponData.DamageFalloffDistance;
                        List<string> attachmentNames = GetAttachmentsList([WeaponData]);
                        List<AttachmentName> attachmentsList = attachmentNames
                            .Select(name => Enum.TryParse(name, true, out AttachmentName attachment) ? attachment : (AttachmentName?)null)
                            .Where(attachment => attachment.HasValue)
                            .Select(attachment => attachment.Value)
                            .ToList();
                        foreach (AttachmentName attachment in attachmentsList)
                        {
                            Firearm.AddAttachment(attachment);
                        }
                        MagCheck(Firearm, WeaponData);
                        break;

                    case CustomItemType.Jailbird:
                        Jailbird Jailbird = Item as Jailbird;
                        IJailbirdData JailbirdData = CustomItem.CustomData as IJailbirdData;

                        Jailbird.Radius = JailbirdData.Radius;
                        Jailbird.ChargeDamage = JailbirdData.ChargeDamage;
                        Jailbird.MeleeDamage = JailbirdData.MeleeDamage;
                        Jailbird.FlashDuration = JailbirdData.FlashDuration;
                        if (JailbirdData.TotalCharges > 3)
                        {
                            JailbirdData.TotalCharges = -(JailbirdData.TotalCharges + 3);
                            Charges = JailbirdData.TotalCharges;
                        }
                        else
                        {
                            Charges = JailbirdData.TotalCharges;
                        }
                        Jailbird.TotalCharges = Charges;
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

                    case CustomItemType.SCPItem:
                        {
                            if (Item.Type == ItemType.SCP018)
                            {
                                LogManager.Debug($"SCPItem is SCP-018");
                                Scp018 Scp018 = Item as Scp018;
                                ISCP018Data SCP018Data = CustomItem.CustomData as ISCP018Data;
                                Scp018.FriendlyFireTime = SCP018Data.FriendlyFireTime;
                                Scp018.FuseTime = SCP018Data.FuseTime;
                            }
                            else if (Item.Type == ItemType.SCP2176)
                            {
                                LogManager.Debug($"SCPItem is SCP-2176");
                                Scp2176 Scp2176 = Item as Scp2176;
                                ISCP2176Data SCP2176Data = CustomItem.CustomData as ISCP2176Data;
                                Scp2176.FuseTime = SCP2176Data.FuseTime;
                            }
                            else if (Item.Type == ItemType.SCP244a)
                            {
                                LogManager.Debug($"SCPItem is SCP-244");
                                Scp244 Scp244 = Item as Scp244;
                                ISCP244Data SCP244Data = CustomItem.CustomData as ISCP244Data;
                                Scp244.ActivationDot = SCP244Data.ActivationDot;
                                Scp244.Health = SCP244Data.Health;
                                Scp244.MaxDiameter = SCP244Data.MaxDiameter;
                                Scp244.Primed = SCP244Data.Primed;
                            }
                            else if (Item.Type == ItemType.SCP244b)
                            {
                                LogManager.Debug($"SCPItem is SCP-244");
                                Scp244 Scp244 = Item as Scp244;
                                ISCP244Data SCP244Data = CustomItem.CustomData as ISCP244Data;
                                Scp244.ActivationDot = SCP244Data.ActivationDot;
                                Scp244.Health = SCP244Data.Health;
                                Scp244.MaxDiameter = SCP244Data.MaxDiameter;
                                Scp244.Primed = SCP244Data.Primed;
                            }
                            break;
                        }
                    default:
                        break;
                }
        }
        /// <summary>
        /// Saves the custom properties of the <see cref="ICustomItem"/> that triggered it
        /// </summary>
        public void SaveProperties()
        {
            if (Item is not null)
            {
                switch (CustomItem.CustomItemType)
                {
                    case CustomItemType.Keycard:
                        {
                            Keycard keycard = Item as Keycard;
                            IKeycardData KeycardData = CustomItem.CustomData as IKeycardData;
                            KeycardGfx keycardGfx = keycard.Base.KeycardGfx;
                            if (keycard != null && KeycardData != null)
                            {
                                CustomItem.Weight = keycardGfx.ExtraWeight;
                                KeycardData.Label = keycardGfx.KeycardLabels[0].text;
                                KeycardData.Name = keycardGfx.NameFields[0].text;
                            }
                            break;
                        }
                    case CustomItemType.Armor:
                        {
                            Armor Armor = Item as Armor;
                            IArmorData ArmorData = CustomItem.CustomData as IArmorData;
                            if (Armor != null && ArmorData != null)
                            {
                                ArmorData.HeadProtection = Armor.HelmetEfficacy;
                                ArmorData.BodyProtection = Armor.VestEfficacy;
                                ArmorData.StaminaUseMultiplier = Armor.StaminaUseMultiplier;
                                ArmorData.StaminaRegenMultiplier = Armor.StaminaRegenMultiplier;
                            }
                            break;
                        }
                    case CustomItemType.Weapon:
                        {
                            Firearm Firearm = Item as Firearm;
                            IWeaponData WeaponData = CustomItem.CustomData as IWeaponData;
                            if (Firearm != null && WeaponData != null)
                            {
                                WeaponData.MaxMagazineAmmo = Firearm.MaxMagazineAmmo;
                                WeaponData.MaxBarrelAmmo = Firearm.MaxBarrelAmmo;
                                WeaponData.Damage = Firearm.Damage;
                                WeaponData.AmmoDrain = Firearm.AmmoDrain;
                                WeaponData.Penetration = Firearm.Penetration;
                                WeaponData.Inaccuracy = Firearm.Inaccuracy;
                                WeaponData.DamageFalloffDistance = Firearm.DamageFalloffDistance;
                            }
                            break;
                        }
                    case CustomItemType.Jailbird:
                        {
                            Jailbird Jailbird = Item as Jailbird;
                            IJailbirdData JailbirdData = CustomItem.CustomData as IJailbirdData;
                            if (Jailbird != null && JailbirdData != null)
                            {
                                JailbirdData.Radius = Jailbird.Radius;
                                JailbirdData.ChargeDamage = Jailbird.ChargeDamage;
                                JailbirdData.MeleeDamage = Jailbird.MeleeDamage;
                                JailbirdData.FlashDuration = Jailbird.FlashDuration;
                                if (JailbirdData.TotalCharges > 3)
                                {
                                    JailbirdData.TotalCharges = -(JailbirdData.TotalCharges + 3);
                                    Charges = JailbirdData.TotalCharges;
                                }
                                else
                                {
                                    Charges = JailbirdData.TotalCharges;
                                }
                                Jailbird.TotalCharges = Charges;
                            }
                            break;
                        }
                    case CustomItemType.ExplosiveGrenade:
                        {
                            ExplosiveGrenade ExplosiveGrenade = Item as ExplosiveGrenade;
                            IExplosiveGrenadeData ExplosiveGrenadeData = CustomItem.CustomData as IExplosiveGrenadeData;
                            if (ExplosiveGrenade != null && ExplosiveGrenadeData != null)
                            {
                                ExplosiveGrenadeData.MaxRadius = ExplosiveGrenade.MaxRadius;
                                ExplosiveGrenadeData.PinPullTime = ExplosiveGrenade.PinPullTime;
                                ExplosiveGrenadeData.ScpDamageMultiplier = ExplosiveGrenade.ScpDamageMultiplier;
                                ExplosiveGrenadeData.ConcussDuration = ExplosiveGrenade.ConcussDuration;
                                ExplosiveGrenadeData.BurnDuration = ExplosiveGrenade.BurnDuration;
                                ExplosiveGrenadeData.DeafenDuration = ExplosiveGrenade.DeafenDuration;
                                ExplosiveGrenadeData.FuseTime = ExplosiveGrenade.FuseTime;
                                ExplosiveGrenadeData.Repickable = ExplosiveGrenade.Repickable;
                            }
                            break;
                        }
                    case CustomItemType.FlashGrenade:
                        {
                            FlashGrenade FlashGrenade = Item as FlashGrenade;
                            IFlashGrenadeData FlashGrenadeData = CustomItem.CustomData as IFlashGrenadeData;
                            if (FlashGrenade != null && FlashGrenadeData != null)
                            {
                                FlashGrenadeData.PinPullTime = FlashGrenade.PinPullTime;
                                FlashGrenadeData.Repickable = FlashGrenade.Repickable;
                                FlashGrenadeData.MinimalDurationEffect = FlashGrenade.MinimalDurationEffect;
                                FlashGrenadeData.AdditionalBlindedEffect = FlashGrenade.AdditionalBlindedEffect;
                                FlashGrenadeData.SurfaceDistanceIntensifier = FlashGrenade.SurfaceDistanceIntensifier;
                                FlashGrenadeData.FuseTime = FlashGrenade.FuseTime;
                            }
                            break;
                        }
                    case CustomItemType.SCPItem:
                        {
                            if (Item.Type == ItemType.SCP018)
                            {
                                Scp018 Scp018 = Item as Scp018;
                                ISCP018Data SCP018Data = CustomItem.CustomData as ISCP018Data;
                                SCP018Data.FriendlyFireTime = Scp018.FriendlyFireTime;
                                SCP018Data.FuseTime = Scp018.FuseTime;
                            }
                            else if (Item.Type == ItemType.SCP2176)
                            {
                                Scp2176 Scp2176 = Item as Scp2176;
                                ISCP2176Data SCP2176Data = CustomItem.CustomData as ISCP2176Data;
                                SCP2176Data.FuseTime = Scp2176.FuseTime;
                            }
                            else if(Item.Type == ItemType.SCP244a)
                            {
                                Scp244 Scp244 = Item as Scp244;
                                ISCP244Data SCP244Data = CustomItem.CustomData as ISCP244Data;
                                SCP244Data.ActivationDot = Scp244.ActivationDot;
                                SCP244Data.Health = Scp244.Health;
                                SCP244Data.MaxDiameter = Scp244.MaxDiameter;
                                SCP244Data.Primed = Scp244.Primed;
                            }
                            else if (Item.Type == ItemType.SCP244b)
                            {
                                Scp244 Scp244 = Item as Scp244;
                                ISCP244Data SCP244Data = CustomItem.CustomData as ISCP244Data;
                                SCP244Data.ActivationDot = Scp244.ActivationDot;
                                SCP244Data.Health = Scp244.Health;
                                SCP244Data.MaxDiameter = Scp244.MaxDiameter;
                                SCP244Data.Primed = Scp244.Primed;
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
            else if (Pickup is not null)
            {
                CustomItem.Scale = Pickup.Scale;
                CustomItem.Weight = Pickup.Weight;
            }
        }

        public static Color GetColor(string colorName)
        {
            switch (colorName.ToLower())
            {
                case "red": return Color.red;
                case "green": return Color.green;
                case "blue": return Color.blue;
                case "yellow": return Color.yellow;
                case "cyan": return Color.cyan;
                case "magenta": return Color.magenta;
                case "white": return Color.white;
                case "black": return Color.black;
                case "gray": return Color.gray;
                case "grey": return Color.gray;
                default:
                    LogManager.Warn($"Invalid color '{colorName}' in config. Using default blue.");
                    return Color.blue;
            }
        }

        /// <summary>
        /// Sets the badge of the <see cref="Player"/> according to the <see cref="ICustomItem"/> BadgeName field.
        /// <param name="Player"></param>
        /// </summary>
        public string LoadBadge(Player Player)
        {
            LogManager.Debug("LoadBadge Triggered");
            string output = "Badge: ";

            if (CustomItem.BadgeColor != string.Empty && CustomItem.BadgeName != string.Empty)
            {
                if (BadgeManager.colorMap.ContainsKey(CustomItem.BadgeColor))
                    output += $"<color={BadgeManager.colorMap[CustomItem.BadgeColor]}>{CustomItem.BadgeName}</color>";
                else
                    output += $"{CustomItem.BadgeName.Replace("@hidden", "")}";
            }
            else
            {
                output += "None";
            }

            LogManager.Debug($"Badge loaded: {output}");

            CustomItemBadgeApplier(Player, CustomItem);

            return output;
        }

        /// <summary>
        /// Applies a custom badge to the specified <see cref="Player"/> if the item has a valid badge name and color.
        /// </summary>
        /// <param name="Player">The player receiving the badge.</param>
        /// <param name="Item">The custom item containing badge details.</param>
        private void CustomItemBadgeApplier(Player Player, ICustomItem Item)
        {
            Triplet<string, string, bool>? Badge = null;
            if (Item.BadgeName is not null && Item.BadgeName.Length > 1 && Item.BadgeColor is not null && Item.BadgeColor.Length > 2)
            {
                Badge = new(Player.RankName ?? "", Player.RankColor ?? "", Player.ReferenceHub.serverRoles.HasBadgeHidden);
                LogManager.Debug($"Badge detected, putting {Item.BadgeName}@{Item.BadgeColor} to player {Player.Id}");

                Player.RankName = Item.BadgeName.Replace("@hidden", "");
                Player.RankColor = Item.BadgeColor;

                if (Item.BadgeName.Contains("@hidden"))
                    if (Player.ReferenceHub.serverRoles.TryHideTag())
                        LogManager.Debug("Tag successfully hidden!");
            }
        }

        /// <summary>
        /// Resets the badge of the <see cref="Player"/>.
        /// <param name="Player"></param>
        /// </summary>
        public void ResetBadge(Player Player)
        {
            if (CustomItem.BadgeName.Length == 0)
                return;
            
            Player.ReferenceHub.serverRoles.RefreshLocalTag();
            Player.ReferenceHub.serverRoles.TryHideTag();
            LogManager.Debug($"{Player.Nickname} Badge successfully reset");
        }

        internal void OnPickup(ItemAddedEventArgs pickedUp)
        {
            Pickup = null;
            Item = pickedUp.Item;
            Owner = pickedUp.Player;
            SetProperties(pickedUp.Player);
            Serial = Item.Serial;
            HandleEvent(pickedUp.Player, ItemEvents.Pickup);
        }

        /// <summary>
        /// Unloads all <see cref="ICustomItem"/> information for the <see cref="Player"/> who dropped the custom item.
        /// </summary>
        /// <param name="dropped"></param>
        public void OnDrop(DroppedItemEventArgs dropped)
        {
            Pickup = dropped.Pickup;
            Item = null;
            Owner = null;
            SaveProperties();
            Serial = Pickup.Serial;
            HandleEvent(dropped.Player, ItemEvents.Drop);
        }

        /// <summary>
        /// Unloads all <see cref="ICustomItem"/> information for the <see cref="Player"/> who died with the custom item.
        /// </summary>
        /// <param name="ev"></param><param name="customItem"></param>
        public void OnDied(DyingEventArgs ev, SummonedCustomItem customItem)
        {
            Timing.CallDelayed(0.1f, () =>
            {
                foreach (Pickup pickup in Pickup.List)
                {
                    if (pickup.Type == customItem.Item.Type)
                    {
                        if (pickup.Serial == customItem.Serial)
                        {
                            Pickup = pickup;
                            Item = null;
                            Owner = null;
                            SaveProperties();
                            Serial = pickup.Serial;
                            HandleEvent(ev.Player, ItemEvents.Drop);
                            Plugin.Instance.Handler.OnDeath(customItem);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Checks the magazine of the held <see cref="Firearm"/> to remove the capacity modifier from a modification.
        /// <param name="Firearm"></param>
        /// <param name="WeaponData"></param>
        /// </summary>
        public void MagCheck(Firearm Firearm, IWeaponData WeaponData)
        { 
            LogManager.Debug($"Performing MagCheck for {CustomItem.Name}");
            if (Firearm.MaxMagazineAmmo != WeaponData.MaxMagazineAmmo)
            {
                int Ammo = WeaponData.MaxMagazineAmmo - Firearm.MaxMagazineAmmo;
                WeaponData.MaxMagazineAmmo += Ammo;
                LogManager.Debug($"Added/Subtracted {Ammo} to/from MaxMagazineAmmo for {CustomItem.Name}");
                LogManager.Debug($"MaxMagazineAmmo for {CustomItem.Name} is now {WeaponData.MaxMagazineAmmo}");
            }
        }
        /// <summary>
        /// Displays the debug ui for <see cref="Firearm"/> information to the selected <see cref="Player"/>.
        /// <param name="Player"></param>
        /// </summary>
        public void ShowDebugUi(Player Player)
        {
            Firearm Firearm = Item as Firearm;
            Player.ShowHint($"{CustomItem.Name} Effective Damage: {Firearm.EffectiveDamage} \n {CustomItem.Name} Effective Inaccuracy: {Firearm.EffectiveInaccuracy} \n {CustomItem.Name} Effective Penetration: {Firearm.EffectivePenetration} \n {CustomItem.Name} Can See Through Dark: {Firearm.CanSeeThroughDark} \n {CustomItem.Name} Aiming: {Firearm.Aiming}");
        }

        /// <summary>
        /// Gets if the current <see cref="SummonedCustomItem"/> implements the given <see cref="CustomFlags"/>
        /// </summary>
        /// <returns><see langword="true"/> if the custom flag represented by the Enum <see cref="CustomFlags"/>; otherwise, <see langword="false"/>.</returns>
        public bool HasModule(CustomFlags Flag)
        {
            if (CustomItem.CustomFlags.HasValue && CustomItem.CustomFlags.Value.HasFlag(Flag))
            {
                LogManager.Debug($"{CustomItem.Name} has {Flag}");
                return true;
            }
            else
                return false;
        }

        private static readonly Dictionary<Player, Dictionary<ushort, bool>> _cooldownStates = new();
        /// <summary>
        /// Handles the commands for <see cref="IItemData"/> CustomItems
        /// </summary>
        /// <param name="player"></param>
        /// <param name="itemEvent"></param>
        public void HandleEvent(Player player, ItemEvents itemEvent)
        {
            IItemData ItemData = CustomItem.CustomData as IItemData;
            if (CustomItem.CustomItemType == CustomItemType.Item && ItemData.Event == itemEvent)
            {
                if (IsOnCooldown(player, player.CurrentItem.Serial))
                {
                    LogManager.Debug($"{CustomItem.Name} is still on cooldown.");
                    return;
                }

                LogManager.Debug($"Firing events for item {CustomItem.Name}");
                System.Random rand = new();
                Player randomPlayer = Player.List.OrderBy(p => rand.Next()).FirstOrDefault();
                string randomPlayerId = randomPlayer?.Id.ToString();

                if (ItemData.Command is not null && ItemData.Command.Length > 2)
                {
                    List<string?> commandsList = CommandsList(new List<IItemData> { ItemData });
                    foreach (string? cmd in commandsList)
                    {
                        if (string.IsNullOrWhiteSpace(cmd))
                            continue;

                        string processedCommand = cmd
                            .Replace("{p_id}", player.Id.ToString())
                            .Replace("{rp_id}", randomPlayerId)
                            .Replace("{p_pos}", player.Position.ToString())
                            .Replace("{p_role}", player.Role.ToString())
                            .Replace("{p_health}", player.Health.ToString())
                            .Replace("{p_zone}", player.Zone.ToString())
                            .Replace("{p_room}", player.CurrentRoom.ToString())
                            .Replace("{p_rotation}", player.Rotation.ToString())
                            .Replace("{pj_pos}", Plugin.Instance.Handler.DetonationPosition.ToString());

                        if (cmd.Contains("{p_id}") || cmd.Contains("{rp_id}") ||
                            cmd.Contains("{p_pos}") || cmd.Contains("{p_role}") ||
                            cmd.Contains("{p_health}") || cmd.Contains("{p_zone}") ||
                            cmd.Contains("{p_room}") || cmd.Contains("{p_rotation}") ||
                            cmd.Contains("{pj_pos}"))
                        {
                            Server.ExecuteCommand(processedCommand, player.Sender);
                        }
                        else
                        {
                            Server.ExecuteCommand(processedCommand);
                        }
                    }
                }
                StartCooldown(player, player.CurrentItem.Serial, ItemData.CoolDown);

                Utilities.ParseResponse(player, ItemData);

                // Destroy the item if needed.
                if (ItemData.DestroyAfterUse)
                    Destroy();
            }
        }

        /// <summary>
        /// Checks whether the specified <see cref="Item.Serial"/> for the given <see cref="Player"/> is on cooldown.
        /// </summary>
        public bool IsOnCooldown(Player player, ushort Serial)
        {
            if (_cooldownStates.TryGetValue(player, out Dictionary<ushort, bool> itemStates))
            {
                if (itemStates.TryGetValue(Serial, out bool isOnCooldown))
                    return isOnCooldown;
            }
            return false;
        }

        /// <summary>
        /// Starts the cooldown coroutine for the given <see cref="Item.Serial"/> and marks it as on cooldown.
        /// </summary>
        public void StartCooldown(Player player, ushort Serial, float cooldown)
        {
            if (!_cooldownStates.ContainsKey(player))
                _cooldownStates[player] = new Dictionary<ushort, bool>();

            _cooldownStates[player][Serial] = true;
            Timing.RunCoroutine(CooldownCoroutine(player, Serial, cooldown));
        }

        /// <summary>
        /// A coroutine that waits for the cooldown period and then resets the cooldown state.
        /// </summary>
        public IEnumerator<float> CooldownCoroutine(Player player, ushort Serial, float cooldown)
        {
            yield return Timing.WaitForSeconds(cooldown);

            if (_cooldownStates.TryGetValue(player, out Dictionary<ushort, bool> itemStates))
            {
                itemStates[Serial] = false;
            }
            LogManager.Debug($"Cooldown complete for item {CustomItem.Name}");
        }

        /// <summary>
        /// Displays the hint from the <see cref="Config.SelectedMessage"/> field in the plugin <see cref="Config"/>.
        /// </summary>
        public void HandleSelectedDisplayHint()
        {
            if (Plugin.Instance.Config.SelectedMessage.Length > 1)
                Owner.ShowHint(Plugin.Instance.Config.SelectedMessage.Replace("%name%", CustomItem.Name).Replace("%desc%", CustomItem.Description).Replace("%description%", CustomItem.Description), Plugin.Instance.Config.SelectedMessageDuration);

        }

        /// <summary>
        /// Displays the hint from the <see cref="Config.PickedUpMessage"/> field in the plugin <see cref="Config"/>.
        /// </summary>
        public void HandlePickedUpDisplayHint()
        {
            if (Plugin.Instance.Config.PickedUpMessage.Length > 1)
                Owner.ShowHint(Plugin.Instance.Config.PickedUpMessage.Replace("%name%", CustomItem.Name).Replace("%desc%", CustomItem.Description).Replace("%description%", CustomItem.Description), Plugin.Instance.Config.PickedUpMessageDuration);
        }

        internal bool HandleCustomAction(Item item)
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

                // Runs also the event as it gets suppressed
                HandleEvent(Owner, ItemEvents.Use);
                if (!CustomItem.Reusable)
                    Item.Get(item.Base).Destroy();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Destroys the <see cref="ICustomItem"/>.
        /// </summary>
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
        /// Try gets a <see cref="SummonedCustomItem"/> by it's serial.
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
        /// Try gets a <see cref="SummonedCustomItem"/> by it's owner and it's serial.<br></br>
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
