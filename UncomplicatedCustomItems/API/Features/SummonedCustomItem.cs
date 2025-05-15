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
using Interactables.Interobjects.DoorUtils;
using UncomplicatedCustomItems.HarmonyElements.Utilities;
using UncomplicatedCustomItems.API.Wrappers;
using UncomplicatedCustomItems.Extensions;
using System.Reflection;
using LabApi.Features.Wrappers;
using InventorySystem.Items.Armor;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using KeycardItem = LabApi.Features.Wrappers.KeycardItem;
using Armor = LabApi.Features.Wrappers.BodyArmorItem;
using Firearm = LabApi.Features.Wrappers.FirearmItem;
using Jailbird = LabApi.Features.Wrappers.JailbirdItem;
using ExplosiveGrenade = LabApi.Features.Wrappers.ExplosiveGrenadeProjectile;
using FlashGrenade = LabApi.Features.Wrappers.FlashbangProjectile;
using Scp018 = LabApi.Features.Wrappers.Scp018Projectile;
using Scp2176 = LabApi.Features.Wrappers.Scp2176Projectile;
using Scp244 = LabApi.Features.Wrappers.Scp244Item;
using InventorySystem.Items.ThrowableProjectiles;
using LabApi.Events.Arguments.PlayerEvents;

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
        /// Stores the original badge status for players who have had a custom item badge applied. Key is Player ID, Value is true if the badge was hidden.
        /// </summary>
        public static Dictionary<int, bool> PlayerBadges = [];

        /// <summary>
        /// The <see cref="ICustomItem"/> reference of the item
        /// </summary>
        public ICustomItem CustomItem { get; internal set; }

        /// <summary>
        /// The <see cref="Player">Owner</see> of the item
        /// </summary>
        public Player Owner { get; internal set; }

        /// <summary>
        /// The <see cref="SummonedCustomItem"/> as an <see cref="LabApi.Features.Wrappers.Items.Firearm"/>
        /// </summary>
        public Item Item { get; internal set; }

        internal bool NameApplied { get; set; } = false;

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

        internal bool PropertiesSet { get; set; }

        internal MagazineModule MagazineModule { get; set; }
        internal HitscanHitregModuleBase HitscanHitregModule { get; set; }
        internal IAmmoContainerModule BarrelModule { get; set; }

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
        public SummonedCustomItem(ICustomItem customItem, Vector3 position, Quaternion rotation = new()) : this(customItem, Pickup.Create(customItem.Item, position, rotation)) { }
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
        public void SetProperties()
        {
            if (Item is not null)
            {
                switch (CustomItem.CustomItemType)
                {
                    case CustomItemType.Keycard:
                        KeycardItem keycard = Item as KeycardItem;
                        PropertyInfo openDoorsProperty = keycard.Base.GetType().GetProperty("OpenDoorsOnThrow", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        IKeycardData KeycardData = CustomItem.CustomData as IKeycardData;
                        ColorUtility.TryParseHtmlString(KeycardData.PermissionsColor, out Color PermissionsColor);
                        ColorUtility.TryParseHtmlString(KeycardData.TintColor, out Color TintColor);
                        ColorUtility.TryParseHtmlString(KeycardData.LabelColor, out Color LabelColor);
                        Color32 PermissionsColor32 = PermissionsColor;
                        Color32 TintColor32 = TintColor;
                        Color32 LabelColor32 = LabelColor;
                        KeycardLevels permissions = new(KeycardData.Containment, KeycardData.Armory, KeycardData.Admin);
                        if (!keycard.Base.Customizable)
                        {
                            LogManager.Warn($"{CustomItem.Name} is not customizable!\nThe item field must be 'KeycardCustomMetalCase', 'KeycardCustomManagement', 'KeycardCustomSite02', or 'KeycardCustomTaskForce'!");
                            return;
                        }

                        CustomKeycard customKeycard = new CustomKeycard(keycard);
                        if (!NameApplied)
                        {
                            customKeycard.NameTag = KeycardData.Name;
                        }
                        openDoorsProperty.SetValue(keycard.Base, true);
                        customKeycard.SerialNumber = KeycardData.SerialNumber;
                        customKeycard.WearIndex = KeycardData.WearDetail;
                        customKeycard.RankIndex = KeycardData.Rank;
                        customKeycard.LabelColor = LabelColor32;
                        customKeycard.LabelText = KeycardData.Label;
                        customKeycard.ItemName = CustomItem.Name;
                        customKeycard.CardColor = TintColor32;
                        customKeycard.PermissionsColor = PermissionsColor32;
                        customKeycard.Permissions = permissions;
                        LogManager.Debug($"{LabelColor32} {LabelColor} {KeycardData.LabelColor}");
                        KeycardUtils.RemoveKeycardDetail(keycard.Serial);
                        KeycardDetailSynchronizer.ServerProcessItem(keycard.Base);
                        NameApplied = true;
                        break;

                    case CustomItemType.Armor:
                        Armor Armor = Item as Armor;
                        IArmorData ArmorData = CustomItem.CustomData as IArmorData;

                        Armor.Base.HelmetEfficacy = ArmorData.HeadProtection;
                        Armor.Base.VestEfficacy = ArmorData.BodyProtection;
                        // Removed because EXILED deprecated Armor.RemoveExcessOnDrop
                        // Armor.RemoveExcessOnDrop = ArmorData.RemoveExcessOnDrop;
                        //Armor.Base.StaminaUsageMultiplier = ArmorData.StaminaUseMultiplier;
                        //Armor.Base.StaminaRegenMultiplier = ArmorData.StaminaRegenMultiplier;
                        if (ArmorData.RemoveExcessOnDrop)
                            LogManager.Warn($"Name: {CustomItem.Name} - ID: {CustomItem.Id}\n'RemoveExcessOnDrop' in ArmorData is deprecated and has no effect.");
                        break;

                    case CustomItemType.Weapon:
                        /*
                        I'll figure our firearms later.
                        List<string> attachmentList = GetAttachmentsList();
                        Firearm Firearm = Item as Firearm;
                        IWeaponData WeaponData = CustomItem.CustomData as IWeaponData;
                        if (!PropertiesSet)
                            MagCheck(Firearm, WeaponData);
                        Firearm.MagazineAmmo = WeaponData.MaxAmmo;
                        Firearm.MaxMagazineAmmo = WeaponData.MaxMagazineAmmo;
                        Firearm.MaxBarrelAmmo = WeaponData.MaxBarrelAmmo;
                        Firearm.AmmoDrain = WeaponData.AmmoDrain;
                        Firearm.Penetration = WeaponData.Penetration;
                        Firearm.Inaccuracy = WeaponData.Inaccuracy;
                        Firearm.DamageFalloffDistance = WeaponData.DamageFalloffDistance;
                        foreach (string attachmentstring in attachmentList)
                        {
                            Enum.TryParse(attachmentstring, out AttachmentName attachment);
                            LogManager.Debug($"Added {attachment} to {CustomItem.Name}");
                            
                        }
                        PropertiesSet = true;
                        */
                        break;

                    case CustomItemType.Jailbird:
                        Jailbird jailbird = Item as Jailbird;
                        IJailbirdData jailbirdData = CustomItem.CustomData as IJailbirdData;

                        jailbird.Base._hitreg._flashedDuration = jailbirdData.FlashDuration;
                        jailbird.Base._hitreg._hitregRadius = jailbirdData.Radius;
                        jailbird.Base._hitreg._damageCharge = jailbirdData.ChargeDamage;
                        jailbird.Base._hitreg._damageMelee = jailbirdData.MeleeDamage;
                        PropertiesSet = true;
                        break;

                    case CustomItemType.ExplosiveGrenade:
                        IExplosiveGrenadeData ExplosiveGrenadeData = CustomItem.CustomData as IExplosiveGrenadeData;
                        LabApi.Features.Wrappers.ThrowableItem ThrowableItem = Item as LabApi.Features.Wrappers.ThrowableItem;
                        ExplosionGrenade baseGrenade = ThrowableItem.Base.Projectile as ExplosionGrenade;
                        ExplosiveGrenade ExplosiveGrenade = ExplosiveGrenadeProjectile.Get(baseGrenade);

                        ExplosiveGrenade.MaxRadius = ExplosiveGrenadeData.MaxRadius;
                        ThrowableItem.Base._pinPullTime = ExplosiveGrenadeData.PinPullTime;
                        ExplosiveGrenade.ScpDamageMultiplier = ExplosiveGrenadeData.ScpDamageMultiplier;
                        ExplosiveGrenade.Base._concussedDuration = ExplosiveGrenadeData.ConcussDuration;
                        ExplosiveGrenade.Base._burnedDuration = ExplosiveGrenadeData.BurnDuration;
                        ExplosiveGrenade.Base._deafenedDuration = ExplosiveGrenadeData.DeafenDuration;
                        ThrowableItem.Base._repickupable = ExplosiveGrenadeData.Repickable;
                        ExplosiveGrenade.RemainingTime = ExplosiveGrenadeData.FuseTime;
                        break;

                    case CustomItemType.FlashGrenade:
                        LabApi.Features.Wrappers.ThrowableItem throwableItem = Item as LabApi.Features.Wrappers.ThrowableItem;
                        ExplosionGrenade baseFlashGrenade = throwableItem.Base.Projectile as ExplosionGrenade;
                        FlashGrenade FlashGrenade = (FlashGrenade)TimedGrenadeProjectile.Get(baseFlashGrenade);
                        IFlashGrenadeData FlashGrenadeData = CustomItem.CustomData as IFlashGrenadeData;

                        throwableItem.Base._pinPullTime = FlashGrenadeData.PinPullTime;
                        throwableItem.Base._repickupable = FlashGrenadeData.Repickable;
                        FlashGrenade.BaseBlindTime = FlashGrenadeData.MinimalDurationEffect;
                        FlashGrenade.Base._additionalBlurDuration = FlashGrenadeData.AdditionalBlindedEffect;
                        FlashGrenade.Base._surfaceZoneDistanceIntensifier = FlashGrenadeData.SurfaceDistanceIntensifier;
                        FlashGrenade.RemainingTime = FlashGrenadeData.FuseTime;
                        break;

                    case CustomItemType.SCPItem:
                        {
                            if (Item.Type == ItemType.SCP244a)
                            {
                                LogManager.Debug($"SCPItem is SCP-244");
                                Scp244 Scp244 = Item as Scp244;
                                ISCP244Data SCP244Data = CustomItem.CustomData as ISCP244Data;
                                Scp244.Base._primed = SCP244Data.Primed;
                            }
                            else if (Item.Type == ItemType.SCP244b)
                            {
                                LogManager.Debug($"SCPItem is SCP-244");
                                Scp244 Scp244 = Item as Scp244;
                                ISCP244Data SCP244Data = CustomItem.CustomData as ISCP244Data;
                                Scp244.Base._primed = SCP244Data.Primed;
                            }
                            else if (Item.Type == ItemType.GunSCP127)
                            {
                                LogManager.Debug($"SCPItem is SCP-127");
                                Firearm ScpFirearm = Item as Firearm;
                                if (ScpFirearm == null)
                                    LogManager.Error($"ScpFirearm is Null!");
                                ISCP127Data Scp127Data = CustomItem.CustomData as ISCP127Data;
                                if (Scp127Data == null)
                                    LogManager.Error($"Scp127Data is null!");
                                else
                                {
                                    try
                                    {
                                        /*
                                        8ScpFirearm.MagazineAmmo = Scp127Data.MaxAmmo;
                                        ScpFirearm.Damage = Scp127Data.Damage;
                                        ScpFirearm.MaxMagazineAmmo = Scp127Data.MaxMagazineAmmo;
                                        ScpFirearm.MaxBarrelAmmo = Scp127Data.MaxBarrelAmmo;
                                        ScpFirearm.AmmoDrain = Scp127Data.AmmoDrain;
                                        ScpFirearm.Penetration = Scp127Data.Penetration;
                                        ScpFirearm.Inaccuracy = Scp127Data.Inaccuracy;
                                        ScpFirearm.DamageFalloffDistance = Scp127Data.DamageFalloffDistance;
                                        */
                                    }
                                    catch (Exception ex)
                                    {
                                        LogManager.Error($"Error when trying to parse Custom127 {ex.Message}\n{ex.StackTrace}");
                                    }
                                }

                            }
                            break;
                        }
                    default:
                        break;
                }
            }
            else if (IsPickup)
            {
                switch (CustomItem.CustomItemType)
                {
                    case CustomItemType.Keycard:
                        LabApi.Features.Wrappers.KeycardItem keycard = (LabApi.Features.Wrappers.KeycardItem)LabApi.Features.Wrappers.KeycardItem.Get((InventorySystem.Items.Keycards.KeycardItem)Item.Base);
                        PropertyInfo openDoorsProperty = keycard.Base.GetType().GetProperty("OpenDoorsOnThrow", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        IKeycardData KeycardData = CustomItem.CustomData as IKeycardData;
                        ColorUtility.TryParseHtmlString(KeycardData.PermissionsColor, out Color PermissionsColor);
                        ColorUtility.TryParseHtmlString(KeycardData.TintColor, out Color TintColor);
                        ColorUtility.TryParseHtmlString(KeycardData.LabelColor, out Color LabelColor);
                        Color32 PermissionsColor32 = PermissionsColor;
                        Color32 TintColor32 = TintColor;
                        Color32 LabelColor32 = LabelColor;
                        KeycardLevels permissions = new(KeycardData.Containment, KeycardData.Armory, KeycardData.Admin);
                        if (!keycard.Base.Customizable)
                        {
                            LogManager.Warn($"{CustomItem.Name} is not customizable!\nThe item field must be 'KeycardCustomMetalCase', 'KeycardCustomManagement', 'KeycardCustomSite02', or 'KeycardCustomTaskForce'!");
                            return;
                        }

                        openDoorsProperty.SetValue(keycard.Base, true);
                        CustomKeycard customKeycard = new CustomKeycard(keycard);
                        customKeycard.SerialNumber = KeycardData.SerialNumber;
                        customKeycard.WearIndex = KeycardData.WearDetail;
                        customKeycard.RankIndex = KeycardData.Rank;
                        customKeycard.LabelColor = LabelColor32;
                        customKeycard.LabelText = KeycardData.Label;
                        customKeycard.ItemName = CustomItem.Name;
                        customKeycard.CardColor = TintColor32;
                        customKeycard.PermissionsColor = PermissionsColor32;
                        customKeycard.Permissions = permissions;
                        LogManager.Debug($"{LabelColor32} {LabelColor} {KeycardData.LabelColor}");
                        KeycardUtils.RemoveKeycardDetail(keycard.Serial);
                        KeycardDetailSynchronizer.ServerProcessItem(keycard.Base);
                        LabApi.Features.Wrappers.KeycardPickup keycardpickup = (LabApi.Features.Wrappers.KeycardPickup)LabApi.Features.Wrappers.KeycardPickup.Create(keycard.Type, Pickup.Position);
                        Pickup.Destroy();
                        keycardpickup.Spawn();
                        Pickup = keycardpickup;
                        Serial = Pickup.Serial;
                        break;

                    case CustomItemType.Armor:
                        BodyArmorItem Armor = (BodyArmorItem)BodyArmorItem.Get((BodyArmor)Item.Base);
                        IArmorData ArmorData = CustomItem.CustomData as IArmorData;

                        Armor.Base.HelmetEfficacy = ArmorData.HeadProtection;
                        Armor.Base.VestEfficacy = ArmorData.BodyProtection;
                        // Removed because EXILED deprecated Armor.RemoveExcessOnDrop
                        // Armor.RemoveExcessOnDrop = ArmorData.RemoveExcessOnDrop;
                        Armor.Base._staminaUseMultiplier = ArmorData.StaminaUseMultiplier;
                        // Armor.Base.StaminaRegenMultiplier = ArmorData.StaminaRegenMultiplier;
                        if (ArmorData.RemoveExcessOnDrop)
                            LogManager.Warn($"Name: {CustomItem.Name} - ID: {CustomItem.Id}\n'RemoveExcessOnDrop' in ArmorData is deprecated and has no effect.");
                        LabApi.Features.Wrappers.BodyArmorPickup armorPickup = (LabApi.Features.Wrappers.BodyArmorPickup)LabApi.Features.Wrappers.BodyArmorPickup.Create(Armor.Type, Pickup.Position);
                        Pickup.Destroy();
                        Pickup = armorPickup;
                        armorPickup.Spawn();
                        Serial = Pickup.Serial;
                        break;

                    case CustomItemType.Weapon:
                        LogManager.Warn($"Customizing Weapon Data currently is not available.");
                    /*
                        I'll figure our firearms later.
                        List<string> attachmentList = GetAttachmentsList();
                        FirearmItem firearm = (FirearmItem)FirearmItem.Get((Firearm)Item.Base);
                        IWeaponData WeaponData = CustomItem.CustomData as IWeaponData;
                        foreach (ModuleBase module in firearm.Base.Modules)
                        {
                            switch (module)
                            {
                                case MagazineModule magazine when MagazineModule == null:
                                    MagazineModule = magazine;
                                    break;

                                case HitscanHitregModuleBase hitscan when HitscanHitregModule == null:
                                    HitscanHitregModule = hitscan;
                                    break;

                                case IAmmoContainerModule barrelammo when BarrelModule == null:
                                    BarrelModule = barrelammo;
                                    break;
                            }
                        }

                        MagazineModule.AmmoMax = WeaponData.MaxAmmo;
                        HitscanHitregModule.BaseDamage = WeaponData.Damage;
                        MagazineModule.AmmoMax = WeaponData.MaxMagazineAmmo;
                        BarrelModule.AmmoMax = WeaponData.MaxBarrelAmmo;
                        firearm.Base.AmmoDrain = WeaponData.AmmoDrain;
                        firearm.Base.Penetration = WeaponData.Penetration;
                        firearm.Base.Inaccuracy = WeaponData.Inaccuracy;
                        firearm.Base.DamageFalloffDistance = WeaponData.DamageFalloffDistance;
                        foreach (string attachmentstring in attachmentList)
                        {
                            Enum.TryParse(attachmentstring, out AttachmentName attachment);
                            LogManager.Debug($"Added {attachment} to {CustomItem.Name}");
                            
                        }
                        LabApi.Features.Wrappers.FirearmPickup firearmpickup = (LabApi.Features.Wrappers.FirearmPickup)LabApi.Features.Wrappers.FirearmPickup.Create(firearm.Type, Pickup.Position);
                        Pickup.Destroy();
                        firearmpickup.Spawn();
                        Pickup = firearmpickup;
                        Serial = Pickup.Serial;*/
                        break;

                    case CustomItemType.ExplosiveGrenade:
                        ExplosiveGrenadeProjectile ExplosiveGrenade = (ExplosiveGrenadeProjectile)ExplosiveGrenadeProjectile.Create(Item.Type, Pickup.Position);
                        IExplosiveGrenadeData ExplosiveGrenadeData = CustomItem.CustomData as IExplosiveGrenadeData;
                        LabApi.Features.Wrappers.ThrowableItem ThrowableItem = Item as LabApi.Features.Wrappers.ThrowableItem;
                        ExplosionGrenade baseGrenade = ThrowableItem.Base.Projectile as ExplosionGrenade;

                        ExplosiveGrenade.MaxRadius = ExplosiveGrenadeData.MaxRadius;
                        ThrowableItem.Base._pinPullTime = ExplosiveGrenadeData.PinPullTime;
                        ExplosiveGrenade.ScpDamageMultiplier = ExplosiveGrenadeData.ScpDamageMultiplier;
                        ExplosiveGrenade.Base._concussedDuration = ExplosiveGrenadeData.ConcussDuration;
                        ExplosiveGrenade.Base._burnedDuration = ExplosiveGrenadeData.BurnDuration;
                        ExplosiveGrenade.Base._deafenedDuration = ExplosiveGrenadeData.DeafenDuration;
                        ThrowableItem.Base._repickupable = ExplosiveGrenadeData.Repickable;
                        ExplosiveGrenade.RemainingTime = ExplosiveGrenadeData.FuseTime;
                        Pickup.Destroy();
                        ExplosiveGrenade.Spawn();
                        Pickup = ExplosiveGrenade;
                        Serial = Pickup.Serial;
                        break;

                    case CustomItemType.FlashGrenade:
                        LabApi.Features.Wrappers.ThrowableItem throwableItem = Item as LabApi.Features.Wrappers.ThrowableItem;
                        ExplosionGrenade baseFlashGrenade = throwableItem.Base.Projectile as ExplosionGrenade;
                        FlashbangProjectile flashbangProjectile = (FlashbangProjectile)FlashbangProjectile.Create(Item.Type, Pickup.Position);
                        IFlashGrenadeData FlashGrenadeData = CustomItem.CustomData as IFlashGrenadeData;

                        throwableItem.Base._pinPullTime = FlashGrenadeData.PinPullTime;
                        throwableItem.Base._repickupable = FlashGrenadeData.Repickable;
                        flashbangProjectile.BaseBlindTime = FlashGrenadeData.MinimalDurationEffect;
                        flashbangProjectile.Base._additionalBlurDuration = FlashGrenadeData.AdditionalBlindedEffect;
                        flashbangProjectile.Base._surfaceZoneDistanceIntensifier = FlashGrenadeData.SurfaceDistanceIntensifier;
                        flashbangProjectile.RemainingTime = FlashGrenadeData.FuseTime;
                        Pickup.Destroy();
                        flashbangProjectile.Spawn();
                        Pickup = flashbangProjectile;
                        Serial = Pickup.Serial;
                        break;

                    case CustomItemType.SCPItem:
                        {
                            if (Item.Type == ItemType.SCP018)
                            {
                                LogManager.Debug($"SCPItem is SCP-018");
                                Scp018 Scp018 = (Scp018)Scp018.Create(CustomItem.Item, Pickup.Position);
                                ISCP018Data SCP018Data = CustomItem.CustomData as ISCP018Data;
                                Scp018.Base._friendlyFireTime = SCP018Data.FriendlyFireTime;
                                Scp018.RemainingTime = SCP018Data.FuseTime;
                                Pickup.Destroy();
                                Pickup = Scp018;
                                Serial = Pickup.Serial;
                            }
                            else if (Item.Type == ItemType.SCP244a)
                            {
                                LogManager.Debug($"SCPItem is SCP-244");
                                Scp244Pickup Scp244 = Pickup as Scp244Pickup;
                                ISCP244Data SCP244Data = CustomItem.CustomData as ISCP244Data;
                                Scp244.Base._activationDot = SCP244Data.ActivationDot;
                                Scp244.Base._health = SCP244Data.Health;
                                Scp244.Base.MaxDiameter = SCP244Data.MaxDiameter;
                                if (SCP244Data.Primed)
                                    Scp244.State = InventorySystem.Items.Usables.Scp244.Scp244State.Active;
                                else
                                    Scp244.State = InventorySystem.Items.Usables.Scp244.Scp244State.Idle;
                                Scp244Pickup scp244a = (Scp244Pickup)Scp244Pickup.Create(Pickup.Type, Pickup.Position);
                                Pickup.Destroy();
                                scp244a.Spawn();
                                Pickup = scp244a;
                                Serial = Pickup.Serial;
                            }
                            else if (Item.Type == ItemType.SCP244b)
                            {
                                LogManager.Debug($"SCPItem is SCP-244");
                                Scp244Pickup Scp244 = Pickup as Scp244Pickup;
                                ISCP244Data SCP244Data = CustomItem.CustomData as ISCP244Data;
                                Scp244.Base._activationDot = SCP244Data.ActivationDot;
                                Scp244.Base._health = SCP244Data.Health;
                                Scp244.Base.MaxDiameter = SCP244Data.MaxDiameter;
                                if (SCP244Data.Primed)
                                    Scp244.State = InventorySystem.Items.Usables.Scp244.Scp244State.Active;
                                else
                                    Scp244.State = InventorySystem.Items.Usables.Scp244.Scp244State.Idle;
                                Scp244Pickup scp244b = (Scp244Pickup)Scp244Pickup.Create(Pickup.Type, Pickup.Position);
                                Pickup.Destroy();
                                scp244b.Spawn();
                                Pickup = scp244b;
                                Serial = Pickup.Serial;
                            }
                            else if (Item.Type == ItemType.GunSCP127)
                            {
                                /*
                                Firearm ScpFirearm = (Firearm)Firearm.Create(CustomItem.Item);
                                ISCP127Data Scp127Data = CustomItem.CustomData as ISCP127Data;
                                ScpFirearm.MagazineAmmo = Scp127Data.MaxAmmo;
                                ScpFirearm.Damage = Scp127Data.Damage;
                                ScpFirearm.MaxMagazineAmmo = Scp127Data.MaxMagazineAmmo;
                                ScpFirearm.MaxBarrelAmmo = Scp127Data.MaxBarrelAmmo;
                                ScpFirearm.AmmoDrain = Scp127Data.AmmoDrain;
                                ScpFirearm.Penetration = Scp127Data.Penetration;
                                ScpFirearm.Inaccuracy = Scp127Data.Inaccuracy;
                                ScpFirearm.DamageFalloffDistance = Scp127Data.DamageFalloffDistance;
                                FirearmPickup SCP127Pickup = (FirearmPickup)ScpFirearm.CreatePickup(Pickup.Position);
                                Pickup.Destroy();
                                Pickup = SCP127Pickup;
                                Serial = Pickup.Serial;
                                */
                            }
                            break;
                        }
                    default:
                        break;
                }
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
                    case CustomItemType.Armor:
                        {
                            Armor Armor = Item as Armor;
                            IArmorData ArmorData = CustomItem.CustomData as IArmorData;
                            if (Armor != null && ArmorData != null)
                            {
                                ArmorData.HeadProtection = Armor.Base.HelmetEfficacy;
                                ArmorData.BodyProtection = Armor.Base.VestEfficacy;
                                // Removed because EXILED deprecated Armor.RemoveExcessOnDrop
                                // ArmorData.RemoveExcessOnDrop = Armor.RemoveExcessOnDrop;
                                ArmorData.StaminaUseMultiplier = Armor.Base._staminaUseMultiplier;
                                ArmorData.StaminaRegenMultiplier = Armor.Base.StaminaRegenMultiplier;
                            }
                            break;
                        }
                    case CustomItemType.Weapon:
                        {
                            Firearm Firearm = Item as Firearm;
                            IWeaponData WeaponData = CustomItem.CustomData as IWeaponData;
                            if (Firearm != null && WeaponData != null)
                            {
                                /*
                                WeaponData.MaxMagazineAmmo = Firearm.MaxMagazineAmmo;
                                WeaponData.MaxBarrelAmmo = Firearm.MaxBarrelAmmo;
                                WeaponData.Damage = Firearm.Damage;
                                WeaponData.AmmoDrain = Firearm.AmmoDrain;
                                WeaponData.Penetration = Firearm.Penetration;
                                WeaponData.Inaccuracy = Firearm.Inaccuracy;
                                WeaponData.DamageFalloffDistance = Firearm.DamageFalloffDistance;
                                */
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
            else if (Pickup is not null)
            {
                CustomItem.Scale = Pickup.GameObject.transform.localScale;
                CustomItem.Weight = Pickup.Weight;
            }
        }

        private List<string> GetAttachmentsList()
        {
            if (CustomItem.CustomData is IWeaponData weaponData)
            {
                string attachmentsString = weaponData.Attachments;

                if (string.IsNullOrWhiteSpace(attachmentsString))
                {
                    return new List<string>();
                }

                List<string> attachmentsList = attachmentsString
                    .Split(',')
                    .Select(att => att.Trim())
                    .Where(att => !string.IsNullOrEmpty(att))
                    .ToList();

                return attachmentsList;
            }
            else
            {
                LogManager.Warn("CustomData is not in the expected IWeaponData format or is null.");
                return new List<string>();
            }
        }

        /// <summary>
        /// Sets the badge of the <see cref="Player"/> according to the <see cref="ICustomItem"/> BadgeName field.
        /// <param name="Player"></param>
        /// </summary>
        public string LoadBadge(Player Player)
        {
            PlayerBadges.TryAdd(Player.PlayerId, Player.UserGroup.HiddenByDefault);
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
                Badge = new(Player.GroupName ?? "", Player.GroupColor ?? "", Player.ReferenceHub.serverRoles.HasBadgeHidden);
                LogManager.Debug($"Badge detected, putting {Item.BadgeName}@{Item.BadgeColor} to player {Player.PlayerId}");

                Player.GroupName = Item.BadgeName.Replace("@hidden", "");
                Player.GroupColor = Item.BadgeColor;

                if (Item.BadgeName.Contains("@hidden"))
                    if (Player.ReferenceHub.serverRoles.TryHideTag())
                        LogManager.Debug("Tag successfully hidden!");
            }
        }

        /// <summary>
        /// Resets the badge of the <see cref="Player"/>.
        /// <param name="player"></param>
        /// </summary>
        public void ResetBadge(Player player)
        {
            if (CustomItem.BadgeName.Length == 0)
                return;
            
            player.ReferenceHub.serverRoles.RefreshLocalTag();
            if (PlayerBadges.TryGetValue(player.PlayerId, out bool Hidden))
            {
                if (Hidden)
                {
                    LogManager.Debug($"Hid {player.Nickname} badge.");
                    player.ReferenceHub.serverRoles.TryHideTag();
                }
            }
            LogManager.Debug($"{player.Nickname} Badge successfully reset");
        }

        internal void OnPickup(PlayerPickedUpItemEventArgs pickedUp)
        {
            Pickup = null;
            Item = pickedUp.Item;
            Owner = pickedUp.Player;
            SetProperties();
            Serial = Item.Serial;
            HandleEvent(pickedUp.Player, ItemEvents.Pickup, pickedUp.Item.Serial);
        }

        /// <summary>
        /// Unloads all <see cref="ICustomItem"/> information for the <see cref="Player"/> who dropped the custom item.
        /// </summary>
        /// <param name="dropped"></param>
        public void OnDrop(PlayerDroppedItemEventArgs dropped)
        {
            Pickup = dropped.Pickup;
            Item = null;
            Owner = null;
            SaveProperties();
            Serial = Pickup.Serial;
            HandleEvent(dropped.Player, ItemEvents.Drop, dropped.Pickup.Serial);
        }

        /// <summary>
        /// Unloads all <see cref="ICustomItem"/> information for the <see cref="Player"/> who died with the custom item.
        /// </summary>
        /// <param name="ev"></param><param name="customItem"></param>
        public void OnDied(PlayerDyingEventArgs ev, SummonedCustomItem customItem)
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
                        }
                    }
                }
            });
        }
        /*
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
        */

        /// <summary>
        /// Gets if the current <see cref="SummonedCustomItem"/> implements the given <see cref="CustomFlags"/>
        /// </summary>
        /// <returns><see langword="true"/> if the custom flag represented by the Enum <see cref="CustomFlags"/>; otherwise, <see langword="false"/>.</returns>
        public bool HasModule(CustomFlags Flag)
        {
            if (CustomItem.CustomFlags.HasValue && CustomItem.CustomFlags.Value.HasFlag(Flag))
            {
                LogManager.Silent($"{CustomItem.Name} has {Flag}");
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
        /// <param name="playerItemSerial"></param>
        public void HandleEvent(Player player, ItemEvents itemEvent, ushort playerItemSerial)
        {
            IItemData ItemData = CustomItem.CustomData as IItemData;
            if (CustomItem.CustomItemType == CustomItemType.Item && ItemData.Event == itemEvent)
            {
                if (IsOnCooldown(player, playerItemSerial))
                {
                    LogManager.Debug($"{CustomItem.Name} is still on cooldown.");
                    return;
                }

                LogManager.Debug($"Firing events for item {CustomItem.Name}");
                System.Random rand = new();
                Player randomPlayer = Player.List.OrderBy(p => rand.Next()).FirstOrDefault();
                string randomPlayerId = randomPlayer?.PlayerId.ToString();

                if (ItemData.Command is not null && ItemData.Command.Length > 2)
                {
                    List<string?> commandsList = CommandsList(new List<IItemData> { ItemData });
                    foreach (string? cmd in commandsList)
                    {
                        if (string.IsNullOrWhiteSpace(cmd))
                            continue;

                        string processedCommand = cmd
                            .Replace("{p_id}", player.PlayerId.ToString())
                            .Replace("{rp_id}", randomPlayerId)
                            .Replace("{p_pos}", player.Position.ToString())
                            .Replace("{p_role}", player.Role.ToString())
                            .Replace("{p_health}", player.Health.ToString())
                            .Replace("{p_zone}", player.Zone.ToString())
                            .Replace("{p_room}", player.Room.ToString())
                            .Replace("{p_rotation}", player.Rotation.ToString())
                            .Replace("{pj_pos}", Plugin.Instance.Handler.DetonationPosition.ToString());

                        if (cmd.Contains("{p_id}") || cmd.Contains("{rp_id}") ||
                            cmd.Contains("{p_pos}") || cmd.Contains("{p_role}") ||
                            cmd.Contains("{p_health}") || cmd.Contains("{p_zone}") ||
                            cmd.Contains("{p_room}") || cmd.Contains("{p_rotation}") ||
                            cmd.Contains("{pj_pos}"))
                        {
                            Server.RunCommand(processedCommand, player.ReferenceHub.queryProcessor._sender);
                        }
                        else
                        {
                            Server.RunCommand(processedCommand);
                        }
                    }
                }
                StartCooldown(player, playerItemSerial, ItemData.CoolDown);

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
                Owner.SendHint(Plugin.Instance.Config.SelectedMessage.Replace("%name%", CustomItem.Name).Replace("%desc%", CustomItem.Description).Replace("%description%", CustomItem.Description), Plugin.Instance.Config.SelectedMessageDuration);

        }

        /// <summary>
        /// Displays the hint from the <see cref="Config.PickedUpMessage"/> field in the plugin <see cref="Config"/>.
        /// </summary>
        public void HandlePickedUpDisplayHint()
        {
            if (Plugin.Instance.Config.PickedUpMessage.Length > 1)
                Owner.SendHint(Plugin.Instance.Config.PickedUpMessage.Replace("%name%", CustomItem.Name).Replace("%desc%", CustomItem.Description).Replace("%description%", CustomItem.Description), Plugin.Instance.Config.PickedUpMessageDuration);
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
                        Owner.Heal(MedikitData.Health);
                        break;
                    case CustomItemType.Painkillers:
                        Timing.RunCoroutine(Utilities.PainkillersCoroutine(Owner, CustomItem.CustomData as IPainkillersData));
                        break;
                    case CustomItemType.Adrenaline:
                        IAdrenalineData AdrenalineData = CustomItem.CustomData as IAdrenalineData;
                        Owner.CreateAhpProcess(AdrenalineData.Amount, limit: 1000f, decay: AdrenalineData.Decay, efficacy: AdrenalineData.Efficacy, sustain: AdrenalineData.Sustain, persistant: AdrenalineData.Persistant);
                        break;
                    default:
                        return false;
                }

                // Runs also the event as it gets suppressed
                HandleEvent(Owner, ItemEvents.Use, Serial);
                if (!CustomItem.Reusable)
                    Owner.RemoveItem(item.Base);

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
                Owner.RemoveItem(Item.Base);

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
        public static List<SummonedCustomItem> Get(Player owner) => List.Where(sci => sci.Owner?.PlayerId == owner.PlayerId).ToList();

        /// <summary>
        /// Gets a <see cref="SummonedCustomItem"/> by it's owner and it's serial.<br></br>
        /// It can't be a pickup!
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="serial"></param>
        /// <returns></returns>
        public static SummonedCustomItem Get(Player owner, ushort serial) => List.Where(sci => sci.Owner is not null && sci.Owner.PlayerId ==  owner.PlayerId && sci.Serial == serial).FirstOrDefault();

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
