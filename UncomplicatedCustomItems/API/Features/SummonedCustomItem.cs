using Interactables.Interobjects.DoorUtils;
using InventorySystem;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Scp127;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.ThrowableProjectiles;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Features.Wrappers;
using MEC;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Interfaces;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using UncomplicatedCustomItems.API.Struct;
using UncomplicatedCustomItems.API.Wrappers;
using UncomplicatedCustomItems.Commands;
using UncomplicatedCustomItems.Events;
using UncomplicatedCustomItems.Events.Arguments.CustomItemEvents;
using UnityEngine;
using Armor = LabApi.Features.Wrappers.BodyArmorItem;
using Jailbird = LabApi.Features.Wrappers.JailbirdItem;
using KeycardItem = LabApi.Features.Wrappers.KeycardItem;
using Light = LabApi.Features.Wrappers.LightSourceToy;
using Scp018 = LabApi.Features.Wrappers.Scp018Projectile;
using Scp244 = LabApi.Features.Wrappers.Scp244Item;

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
        /// Cache of all <see cref="SummonedCustomItem"/> instances mapped by their <see cref="Serial"/>.
        /// </summary>
        public static readonly ConcurrentDictionary<ushort, SummonedCustomItem> bySerial = new();

        /// <summary>
        /// Cache of all <see cref="SummonedCustomItem"/> instances grouped by their owner's <see cref="Player.PlayerId"/>.
        /// </summary>
        public static readonly ConcurrentDictionary<int, ConcurrentBag<SummonedCustomItem>> byPlayerId = new();

        /// <summary>
        /// HashSet for faster existence checks and removal operations
        /// </summary>
        private static readonly ConcurrentDictionary<ushort, byte> _activeSerials = new();

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
        /// The <see cref="SummonedCustomItem"/> as an <see cref="LabApi.Features.Wrappers.Item"/>
        /// </summary>
        public Item Item { get; internal set; }

        internal bool NameApplied { get; set; } = false;

        /// <summary>
        /// Converts the Command custom data from items into a list to allow multiple commands.
        /// </summary>
        public static List<string?> CommandsList(List<ItemDataList> commands)
        {
            List<string?> result = [];
            
                foreach (ItemDataList data in commands)
                    result.Add(data.Command);

            return result;
        }

        /// <summary>
        /// The <see cref="SummonedCustomItem"/> as a <see cref="LabApi.Features.Wrappers.Pickup"/>.
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

        internal bool FlashLightToggle { get; set; }

        internal bool PropertiesSet { get; set; }

        /// <summary>
        /// Gets or sets the light on a <see cref="Features.CustomItem"/> if its type is <see cref="CustomItemType.Light"/>.
        /// </summary>
        public Light Light { get; set; }
        internal bool Toggled { get; set; } = false;
        internal MagazineModule MagazineModule { get; set; }
        internal HitscanHitregModuleBase HitscanHitregModule { get; set; }
        internal IAmmoContainerModule BarrelModule { get; set; }
        internal Scp127MagazineModule Scp127MagazineModule { get; set; }
        internal Scp127Hitscan Scp127Hitscan { get; set; }

        public SummonedCustomItem(ICustomItem customItem, Player owner, Item item, Pickup pickup, Quaternion rotation = new())
        {
            CustomItem = customItem;
            Owner = owner;
            Item = item;
            Serial = item is not null ? item.Serial : pickup.Serial;
            Pickup = pickup;
            
            if (IsPickup)
                Pickup.Rotation = rotation;

            SetProperties();
            AddToCollections(this);
        }

        public SummonedCustomItem(ICustomItem customItem, Pickup pickup) : this(customItem, null, null, pickup) { }

        public SummonedCustomItem(ICustomItem customItem, Vector3 position, Quaternion rotation = new()) : this(customItem, null, null, customItem.Item.CreateAndSpawn(position), rotation) { }

        public SummonedCustomItem(ICustomItem customItem, Player player) : this(customItem, player, player.AddItem(customItem.Item), null) { }

        public SummonedCustomItem(ICustomItem customItem, Player player, Item item) : this(customItem, player, item, null) { }

#if EXILED
        public SummonedCustomItem(ICustomItem customItem, Exiled.API.Features.Player player) : this(customItem, Player.Get(player.Id), Player.Get(player.Id).AddItem(customItem.Item), null) { }

        public SummonedCustomItem(ICustomItem customItem, Exiled.API.Features.Player player, Item item) : this(customItem, Player.Get(player.Id), item, null) { }
#endif

        private static void AddToCollections(SummonedCustomItem sci)
        {
            List.Add(sci);
            bySerial[sci.Serial] = sci;
            _activeSerials[sci.Serial] = 0;

            if (sci.Owner != null)
            {
                byPlayerId.AddOrUpdate(
                    sci.Owner.PlayerId,
                    [sci],
                    (key, existingBag) => { existingBag.Add(sci); return existingBag; }
                );
            }
        }
        
        public void SetProperties()
        {
            if (Item is not null)
            {
                switch (CustomItem.CustomItemType)
                {
                    case CustomItemType.Keycard:
                        KeycardItem keycard = Item as KeycardItem;
                        IKeycardData keycardData = CustomItem.CustomData as IKeycardData;
                        
                        ColorUtility.TryParseHtmlString(keycardData.PermissionsColor, out Color permissionsColor);
                        ColorUtility.TryParseHtmlString(keycardData.TintColor, out Color tintColor);
                        ColorUtility.TryParseHtmlString(keycardData.LabelColor, out Color labelColor);
                        Color32 permissionsColor32 = permissionsColor;
                        Color32 tintColor32 = tintColor;
                        Color32 labelColor32 = labelColor;
                        KeycardLevels permissions = new(keycardData.Containment, keycardData.Armory, keycardData.Admin);
                        if (!keycard.Base.Customizable)
                        {
                            LogManager.Warn($"{CustomItem.Name} is not customizable!\nThe item field must be 'KeycardCustomMetalCase', 'KeycardCustomManagement', 'KeycardCustomSite02', or 'KeycardCustomTaskForce'!");
                            return;
                        }

                        CustomKeycard customKeycard = new(keycard.Base);
                        if (!NameApplied)
                            customKeycard.NameTag = keycardData.Name;

                        customKeycard.SerialNumber = keycardData.SerialNumber;
                        customKeycard.WearIndex = keycardData.WearDetail;
                        customKeycard.RankIndex = keycardData.Rank;
                        customKeycard.LabelColor = labelColor32;
                        customKeycard.LabelText = keycardData.Label;
                        customKeycard.ItemName = CustomItem.Name;
                        customKeycard.CardColor = tintColor32;
                        customKeycard.PermissionsColor = permissionsColor32;
                        customKeycard.Permissions = permissions;
                        LogManager.Debug($"{labelColor32} {labelColor} {keycardData.LabelColor}");
                        KeycardDetailSynchronizer.Database.Remove(keycard.Serial);
                        KeycardDetailSynchronizer.ServerProcessItem(keycard.Base);
                        NameApplied = true;
                        break;

                    case CustomItemType.Armor:
                        Armor armor = Item as Armor;
                        IArmorData armorData = CustomItem.CustomData as IArmorData;

                        armor.Base.HelmetEfficacy = armorData.HeadProtection;
                        armor.Base.VestEfficacy = armorData.BodyProtection;
                        armor.Base._staminaUseMultiplier = armorData.StaminaUseMultiplier;
                        if (armorData.RemoveExcessOnDrop)
                            LogManager.Warn($"Name: {CustomItem.Name} - ID: {CustomItem.Id}\n'RemoveExcessOnDrop' in ArmorData is deprecated and has no effect.");
                        break;

                    case CustomItemType.Weapon:
                        List<string> attachmentList = GetAttachmentsList();
                        FirearmItem firearm = Item as FirearmItem;
                        IWeaponData weaponData = CustomItem.CustomData as IWeaponData;
                        firearm.Base.TryGetModule<MagazineModule>(out var magazine);
                        MagazineModule = magazine;
                        firearm.Base.TryGetModule<HitscanHitregModuleBase>(out var hitscan);
                        HitscanHitregModule = hitscan;

                        foreach (string attachmentString in attachmentList)
                        {
                            if (Enum.TryParse(attachmentString, out AttachmentName attachment))
                            {
                                if (firearm.Base.TryApplyAttachment(attachment))
                                    LogManager.Debug($"Added {attachment} to {CustomItem.Name}");
                                else
                                    LogManager.Error($"Failed to add {attachment} to {CustomItem.Name}");
                            }
                            else
                                LogManager.Warn($"{nameof(SetProperties)}: [{attachment}] is not a valid attachment for {CustomItem.Name} - {CustomItem.Id} - {Item.Type}");
                        }

                        if (!PropertiesSet)
                            MagazineModule.AmmoStored = weaponData.MaxAmmo;

                        HitscanHitregModule.BaseDamage = weaponData.Damage;
                        HitscanHitregModule.BasePenetration = weaponData.Penetration;
                        HitscanHitregModule.BaseBulletInaccuracy = weaponData.Inaccuracy;
                        HitscanHitregModule.DamageFalloffDistance = weaponData.DamageFalloffDistance;
                        MagazineModule.ServerResyncData();
                        PropertiesSet = true;
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
                        IExplosiveGrenadeData explosiveGrenadeData = CustomItem.CustomData as IExplosiveGrenadeData;
                        Item.Base.ItemId.TryGetTemplate<InventorySystem.Items.ThrowableProjectiles.ThrowableItem>(out var grenadeThrowable);
                        ExplosionGrenade grenade = grenadeThrowable.Projectile as ExplosionGrenade;
                        grenadeThrowable.ItemSerial = Item.Serial;
                        grenade.Info.Serial = Item.Serial;

                        grenade.MaxRadius = explosiveGrenadeData.MaxRadius;
                        grenadeThrowable._pinPullTime = explosiveGrenadeData.PinPullTime;
                        grenade.ScpDamageMultiplier = explosiveGrenadeData.ScpDamageMultiplier;
                        grenade._concussedDuration = explosiveGrenadeData.ConcussDuration;
                        grenade._burnedDuration = explosiveGrenadeData.BurnDuration;
                        grenade._deafenedDuration = explosiveGrenadeData.DeafenDuration;
                        grenadeThrowable._repickupable = explosiveGrenadeData.Repickable;
                        grenade._fuseTime = explosiveGrenadeData.FuseTime;
                        break;

                    case CustomItemType.FlashGrenade:
                        Item.Base.ItemId.TryGetTemplate<InventorySystem.Items.ThrowableProjectiles.ThrowableItem>(out var flashGrenadeThrowable);
                        FlashbangGrenade flashGrenade = flashGrenadeThrowable.Projectile as FlashbangGrenade;
                        flashGrenadeThrowable.ItemSerial = Item.Serial;
                        flashGrenade.Info.Serial = Item.Serial;
                        IFlashGrenadeData flashGrenadeData = CustomItem.CustomData as IFlashGrenadeData;

                        flashGrenadeThrowable._pinPullTime = flashGrenadeData.PinPullTime;
                        flashGrenadeThrowable._repickupable = flashGrenadeData.Repickable;
                        flashGrenade.BlindTime = flashGrenadeData.MinimalDurationEffect;
                        flashGrenade._additionalBlurDuration = flashGrenadeData.AdditionalBlindedEffect;
                        flashGrenade._surfaceZoneDistanceIntensifier = flashGrenadeData.SurfaceDistanceIntensifier;
                        flashGrenade._fuseTime = flashGrenadeData.FuseTime;
                        break;

                    case CustomItemType.MicroHID:
                        MicroHIDItem microHID = Item as MicroHIDItem;
                        IMicroHIDData microData = CustomItem.CustomData as IMicroHIDData;
                        microHID.Energy = microData.Energy;
                        if (microData.Broken)
                            microHID.Base.BrokenSync.ServerSetBroken();
                        break;

                    case CustomItemType.ParticleDisruptor:
                        ParticleDisruptorItem particleDisruptor = Item as ParticleDisruptorItem;
                        IParticleDisruptorData disruptorData = CustomItem.CustomData as IParticleDisruptorData;
                        particleDisruptor.Base.TryGetModule<DisruptorHitregModule>(out var hitregModule);

                        hitregModule.BasePenetration = disruptorData.Penetration;
                        break;

                    case CustomItemType.Light:
                        if (Item.Type is ItemType.Flashlight && !PropertiesSet)
                        {
                            FlashlightItem flashLight = Item as FlashlightItem;
                            IFlashlightData data = CustomItem.CustomData as IFlashlightData;
                            Light newLight = Light.Create(flashLight.CurrentOwner.GameObject.transform);
                            ColorUtility.TryParseHtmlString(data.HexColor, out Color color);

                            newLight.Color = color;
                            newLight.Type = data.LightType;
                            newLight.Range = data.Range;
                            newLight.ShadowType = data.ShadowType;
                            newLight.ShadowStrength = data.ShadowStrength;
                            newLight.Intensity = data.Intensity;
                            newLight.Shape = data.Shape;
                            newLight.SpotAngle = data.SpotLightAngle;
                            newLight.Transform.localPosition = new Vector3(0.05f, 0.35f, 0.5f);
                            newLight.SyncInterval = 0;
                            Light = newLight;
                            Timing.RunCoroutine(LightFacingForward());
                            LogManager.Info($"{newLight.Position}, {newLight.Parent}, {flashLight.IsEmitting}, {newLight.Base.enabled}");
                        }
                        if (Item.Type is ItemType.Lantern && !PropertiesSet)
                        {
                            LanternItem lantern = Item as LanternItem;
                            IFlashlightData data = CustomItem.CustomData as IFlashlightData;
                            Light newLight = Light.Create(lantern.CurrentOwner.GameObject.transform);
                            ColorUtility.TryParseHtmlString(data.HexColor, out Color color);

                            newLight.Color = color;
                            newLight.Type = data.LightType;
                            newLight.Range = data.Range;
                            newLight.ShadowType = data.ShadowType;
                            newLight.ShadowStrength = data.ShadowStrength;
                            newLight.Intensity = data.Intensity;
                            newLight.Shape = data.Shape;
                            newLight.SpotAngle = data.SpotLightAngle;
                            newLight.Transform.localPosition = new Vector3(0.05f, 0f, 0.5f);
                            newLight.Base.enabled = true;
                            newLight.SyncInterval = 0;
                            Light = newLight;
                            Timing.RunCoroutine(LightFacingForward());
                        }
                        break;

                    case CustomItemType.SCPItem:
                        {
                            if (Item.Type == ItemType.SCP018)
                            {
                                LabApi.Features.Wrappers.ThrowableItem scp018throwableItem = Item as LabApi.Features.Wrappers.ThrowableItem;
                                InventorySystem.Items.ThrowableProjectiles.Scp018Projectile scp018 = scp018throwableItem.Base.Projectile as InventorySystem.Items.ThrowableProjectiles.Scp018Projectile;
                                ISCP018Data scp018Data = CustomItem.CustomData as ISCP018Data;
                                scp018._fuseTime = scp018Data.FuseTime;
                                scp018._friendlyFireTime = scp018Data.FriendlyFireTime;
                            }
                            else if (Item.Type == ItemType.SCP244a)
                            {
                                LogManager.Debug($"SCPItem is SCP-244");
                                Scp244 scp244 = Item as Scp244;
                                ISCP244Data scp244Data = CustomItem.CustomData as ISCP244Data;
                                scp244.Base._primed = scp244Data.Primed;
                            }
                            else if (Item.Type == ItemType.SCP244b)
                            {
                                LogManager.Debug($"SCPItem is SCP-244");
                                Scp244 scp244 = Item as Scp244;
                                ISCP244Data scp244Data = CustomItem.CustomData as ISCP244Data;
                                scp244.Base._primed = scp244Data.Primed;
                            }
                            else if (Item.Type == ItemType.GunSCP127)
                            {
                                LogManager.Debug($"SCPItem is SCP-127");
                                FirearmItem scpFirearm = Item as FirearmItem;
                                ISCP127Data scp127Data = CustomItem.CustomData as ISCP127Data;
                                scpFirearm.Base.TryGetModule<Scp127MagazineModule>(out var scp127magazine);
                                Scp127MagazineModule = scp127magazine;
                                scpFirearm.Base.TryGetModule<Scp127Hitscan>(out var scp127hitscan);
                                Scp127Hitscan = scp127hitscan;

                                if (!PropertiesSet)
                                    Scp127MagazineModule.AmmoStored = scp127Data.MaxAmmo;
                                Scp127Hitscan.BaseDamage = scp127Data.Damage;
                                Scp127Hitscan.BasePenetration = scp127Data.Penetration;
                                Scp127Hitscan.BaseBulletInaccuracy = scp127Data.Inaccuracy;
                                Scp127Hitscan.DamageFalloffDistance = scp127Data.DamageFalloffDistance;
                                Scp127MagazineModule.ServerResyncData();
                                PropertiesSet = true;
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
                        LabApi.Features.Wrappers.KeycardPickup keycard = (LabApi.Features.Wrappers.KeycardPickup)LabApi.Features.Wrappers.KeycardPickup.Create(CustomItem.Item, Pickup.Position);
                        IKeycardData keycardData = CustomItem.CustomData as IKeycardData;
                        ColorUtility.TryParseHtmlString(keycardData.PermissionsColor, out Color permissionsColor);
                        ColorUtility.TryParseHtmlString(keycardData.TintColor, out Color tintColor);
                        ColorUtility.TryParseHtmlString(keycardData.LabelColor, out Color labelColor);
                        Color32 permissionsColor32 = permissionsColor;
                        Color32 tintColor32 = tintColor;
                        Color32 labelColor32 = labelColor;
                        KeycardLevels permissions = new(keycardData.Containment, keycardData.Armory, keycardData.Admin);
                        keycard.Base.Info.ItemId.TryGetTemplate<InventorySystem.Items.Keycards.KeycardItem>(out var item);
                        item.ItemSerial = keycard.Serial;
                        CustomKeycard customKeycard = new CustomKeycard(item);
                        customKeycard.SerialNumber = keycardData.SerialNumber;
                        customKeycard.WearIndex = keycardData.WearDetail;
                        customKeycard.RankIndex = keycardData.Rank;
                        customKeycard.LabelColor = labelColor32;
                        customKeycard.LabelText = keycardData.Label;
                        customKeycard.ItemName = CustomItem.Name;
                        customKeycard.CardColor = tintColor32;
                        customKeycard.PermissionsColor = permissionsColor32;
                        customKeycard.Permissions = permissions;
                        LogManager.Debug($"{labelColor32} {labelColor} {keycardData.LabelColor}");
                        KeycardDetailSynchronizer.Database.Remove(keycard.Serial);
                        KeycardDetailSynchronizer.ServerProcessPickup(keycard.Base);
                        Pickup.Destroy();
                        keycard.Spawn();
                        Pickup = keycard;
                        Serial = Pickup.Serial;
                        break;

                    case CustomItemType.Weapon:
                        List<string> attachmentList = GetAttachmentsList();
                        LabApi.Features.Wrappers.FirearmPickup firearmPickup = (LabApi.Features.Wrappers.FirearmPickup)LabApi.Features.Wrappers.FirearmPickup.Create(CustomItem.Item, Pickup.Position);
                        firearmPickup.Base.Info.ItemId.TryGetTemplate<Firearm>(out Firearm firearm);
                        IWeaponData weaponData = CustomItem.CustomData as IWeaponData;
                        firearm.ItemSerial = firearmPickup.Serial;
                        firearm.TryGetModule<MagazineModule>(out var magazine);
                        MagazineModule = magazine;
                        firearm.TryGetModule<HitscanHitregModuleBase>(out var hitscan);
                        HitscanHitregModule = hitscan;

                        if (weaponData.Attachments.Count() > 1)
                        {
                            foreach (string attachmentString in attachmentList)
                            {
                                if (Enum.TryParse(attachmentString, out AttachmentName attachment))
                                {
                                    if (firearmPickup.Base.TryApplyAttachment(attachment))
                                        LogManager.Debug($"Added {attachment} to {CustomItem.Name}");
                                    else
                                        LogManager.Error($"Failed to add {attachment} to {CustomItem.Name}");
                                }
                                else
                                    LogManager.Warn($"{nameof(SetProperties)}: [{attachment}] is not a valid attachment for {CustomItem.Name} - {CustomItem.Id} - {Pickup.Type}");
                            }
                        }
                        else
                        {
                            LogManager.Debug($"No attachments found for {CustomItem.Name} - {CustomItem.Id} applying random attachments...");
                            AttachmentCodeSync.ServerSetCode(firearmPickup.Base.Info.Serial, AttachmentsUtils.GetRandomAttachmentsCode(firearmPickup.Base.Info.ItemId));
                        }
                        MagazineModule.MagazineInserted = true;
                        HitscanHitregModule.BaseDamage = weaponData.Damage;
                        if (!PropertiesSet)
                            MagazineModule.AmmoStored = weaponData.MaxAmmo;
                        HitscanHitregModule.BasePenetration = weaponData.Penetration;
                        HitscanHitregModule.BaseBulletInaccuracy = weaponData.Inaccuracy;
                        HitscanHitregModule.DamageFalloffDistance = weaponData.DamageFalloffDistance;
                        MagazineModule.ServerResyncData();
                        Pickup.Destroy();
                        firearmPickup.Spawn();
                        Pickup = firearmPickup;
                        Serial = Pickup.Serial;
                        PropertiesSet = true;
                        break;

                    case CustomItemType.ExplosiveGrenade:
                        Pickup.Base.Info.ItemId.TryGetTemplate<InventorySystem.Items.ThrowableProjectiles.ThrowableItem>(out var explosiveGrenadeThrowable);
                        IExplosiveGrenadeData explosiveGrenadeData = CustomItem.CustomData as IExplosiveGrenadeData;
                        explosiveGrenadeThrowable.ItemSerial = Pickup.Serial;
                        ExplosionGrenade explosiveGrenade = explosiveGrenadeThrowable.Projectile as ExplosionGrenade;
                        explosiveGrenade.Info.Serial = Pickup.Serial;

                        explosiveGrenadeThrowable._pinPullTime = explosiveGrenadeData.PinPullTime;
                        explosiveGrenadeThrowable._repickupable = explosiveGrenadeData.Repickable;
                        explosiveGrenade.MaxRadius = explosiveGrenadeData.MaxRadius;
                        explosiveGrenade.ScpDamageMultiplier = explosiveGrenadeData.ScpDamageMultiplier;
                        explosiveGrenade._concussedDuration = explosiveGrenadeData.ConcussDuration;
                        explosiveGrenade._burnedDuration = explosiveGrenadeData.BurnDuration;
                        explosiveGrenade._deafenedDuration = explosiveGrenadeData.DeafenDuration;
                        explosiveGrenade._fuseTime = explosiveGrenadeData.FuseTime;
                        break;

                    case CustomItemType.FlashGrenade:
                        Pickup.Base.Info.ItemId.TryGetTemplate<InventorySystem.Items.ThrowableProjectiles.ThrowableItem>(out var flashGrenadeThrowable);
                        IFlashGrenadeData flashGrenadeData = CustomItem.CustomData as IFlashGrenadeData;
                        flashGrenadeThrowable.ItemSerial = Pickup.Serial;
                        FlashbangGrenade flashGrenade = flashGrenadeThrowable.Projectile as FlashbangGrenade;
                        flashGrenade.Info.Serial = Pickup.Serial;

                        flashGrenadeThrowable._repickupable = flashGrenadeData.Repickable;
                        flashGrenadeThrowable._pinPullTime = flashGrenadeData.PinPullTime;
                        flashGrenade._additionalBlurDuration = flashGrenadeData.AdditionalBlindedEffect;
                        flashGrenade._surfaceZoneDistanceIntensifier = flashGrenadeData.SurfaceDistanceIntensifier;
                        flashGrenade._fuseTime = flashGrenadeData.FuseTime;
                        break;

                    case CustomItemType.MicroHID:
                        MicroHIDPickup microHID = Pickup as MicroHIDPickup;
                        IMicroHIDData microData = CustomItem.CustomData as IMicroHIDData;
                        microHID.Base.Info.ItemId.TryGetTemplate<InventorySystem.Items.MicroHID.MicroHIDItem>(out var microHIDItem);
                        microHIDItem.ItemSerial = microHID.Serial;

                        microHIDItem.EnergyManager.ServerSetEnergy(microHIDItem.ItemSerial, microData.Energy);
                        break;

                    case CustomItemType.ParticleDisruptor:
                        Pickup.Base.Info.ItemId.TryGetTemplate<ParticleDisruptor>(out var particleDisruptor);
                        IParticleDisruptorData disruptorData = CustomItem.CustomData as IParticleDisruptorData;
                        particleDisruptor.TryGetModule<DisruptorHitregModule>(out var hitregModule);

                        hitregModule.BasePenetration = disruptorData.Penetration;
                        break;

                    case CustomItemType.Light:
                        if (Pickup.Type is ItemType.Flashlight && !PropertiesSet)
                        {
                            Pickup.Base.Info.ItemId.TryGetTemplate<InventorySystem.Items.ToggleableLights.Flashlight.FlashlightItem>(out var flashLight);
                            IFlashlightData data = CustomItem.CustomData as IFlashlightData;
                            Light newLight = Light.Create(flashLight.gameObject.transform.position);
                            ColorUtility.TryParseHtmlString(data.HexColor, out Color color);
                            newLight.Color = color;
                            newLight.Type = data.LightType;
                            newLight.Range = data.Range;
                            newLight.ShadowType = data.ShadowType;
                            newLight.ShadowStrength = data.ShadowStrength;
                            newLight.Intensity = data.Intensity;
                            newLight.Shape = data.Shape;
                            newLight.SpotAngle = data.SpotLightAngle;
                            newLight.Parent = Pickup.Base.transform;
                            newLight.Position = Pickup.Position;
                            newLight.SyncInterval = 0;
                            Light = newLight;
                            newLight.Base.enabled = true;
                        }
                        if (Pickup.Type is ItemType.Lantern && !PropertiesSet)
                        {
                            Pickup.Base.Info.ItemId.TryGetTemplate<InventorySystem.Items.ToggleableLights.Lantern.LanternItem>(out var lantern);
                            IFlashlightData data = CustomItem.CustomData as IFlashlightData;
                            Light newLight = Light.Create(lantern.gameObject.transform.position);
                            ColorUtility.TryParseHtmlString(data.HexColor, out Color color);
                            newLight.Color = color;
                            newLight.Type = data.LightType;
                            newLight.Range = data.Range;
                            newLight.ShadowType = data.ShadowType;
                            newLight.ShadowStrength = data.ShadowStrength;
                            newLight.Intensity = data.Intensity;
                            newLight.Shape = data.Shape;
                            newLight.SpotAngle = data.SpotLightAngle;
                            newLight.Parent = Pickup.Base.transform;
                            newLight.Position = Pickup.Position;
                            newLight.SyncInterval = 0;
                            Light = newLight;
                            newLight.Base.enabled = true;
                        }
                        break;

                    case CustomItemType.SCPItem:
                        {
                            if (Pickup.Type == ItemType.SCP244a)
                            {
                                LogManager.Debug($"SCPItem is SCP-244");
                                Scp244Pickup scp244Pickup = (Scp244Pickup)Scp244Pickup.Create(CustomItem.Item, Pickup.Position);
                                ISCP244Data scp244Data = CustomItem.CustomData as ISCP244Data;
                                scp244Pickup.Base.MaxDiameter = scp244Data.MaxDiameter;
                                scp244Pickup.Base._activationDot = scp244Data.ActivationDot;
                                scp244Pickup.Base._health = scp244Data.Health;
                                scp244Pickup.Base.enabled = scp244Data.Primed;
                                Pickup.Destroy();
                                scp244Pickup.Spawn();
                                Pickup = scp244Pickup;
                                Serial = Pickup.Serial;
                            }
                            else if (Pickup.Type == ItemType.SCP244b)
                            {
                                LogManager.Debug($"SCPItem is SCP-244");
                                Scp244Pickup scp244Pickup = (Scp244Pickup)Scp244Pickup.Create(CustomItem.Item, Pickup.Position);
                                ISCP244Data scp244Data = CustomItem.CustomData as ISCP244Data;
                                scp244Pickup.Base.MaxDiameter = scp244Data.MaxDiameter;
                                scp244Pickup.Base._activationDot = scp244Data.ActivationDot;
                                scp244Pickup.Base._health = scp244Data.Health;
                                scp244Pickup.Base.enabled = scp244Data.Primed;
                                Pickup.Destroy();
                                scp244Pickup.Spawn();
                                Pickup = scp244Pickup;
                                Serial = Pickup.Serial;
                            }
                            else if (Pickup.Type == ItemType.SCP018)
                            {
                                Scp018 scp018 = Pickup as Scp018;
                                ISCP018Data scp018Data = CustomItem.CustomData as ISCP018Data;
                                scp018.Base._fuseTime = scp018Data.FuseTime;
                                scp018.Base._friendlyFireTime = scp018Data.FriendlyFireTime;
                            }
                            else if (Pickup.Type == ItemType.GunSCP127)
                            {
                                LogManager.Debug($"SCPItem is SCP-127");
                                LabApi.Features.Wrappers.FirearmPickup scpFirearmPickup = (LabApi.Features.Wrappers.FirearmPickup)LabApi.Features.Wrappers.FirearmPickup.Create(CustomItem.Item, Pickup.Position);
                                scpFirearmPickup.Base.Info.ItemId.TryGetTemplate<Firearm>(out Firearm scpFirearm);
                                ISCP127Data scp127Data = CustomItem.CustomData as ISCP127Data;
                                scpFirearm.ItemSerial = scpFirearmPickup.Serial;
                                scpFirearm.TryGetModule<Scp127MagazineModule>(out var scp127magazine);
                                Scp127MagazineModule = scp127magazine;
                                scpFirearm.TryGetModule<Scp127Hitscan>(out var scp127hitscan);
                                Scp127Hitscan = scp127hitscan;

                                Scp127MagazineModule.MagazineInserted = true;
                                if (!PropertiesSet)
                                    Scp127MagazineModule.AmmoStored = scp127Data.MaxAmmo;
                                Scp127Hitscan.BaseDamage = scp127Data.Damage;
                                Scp127Hitscan.BasePenetration = scp127Data.Penetration;
                                Scp127Hitscan.BaseBulletInaccuracy = scp127Data.Inaccuracy;
                                Scp127Hitscan.DamageFalloffDistance = scp127Data.DamageFalloffDistance;
                                Scp127MagazineModule.ServerResyncData();
                                Pickup.Destroy();
                                scpFirearmPickup.Spawn();
                                Pickup = scpFirearmPickup;
                                Serial = Pickup.Serial;
                                PropertiesSet = true;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public void SaveProperties()
        {
            if (Item is not null)
            {
                switch (CustomItem.CustomItemType)
                {
                    case CustomItemType.Armor:
                        {
                            Armor armor = Item as Armor;
                            if (armor != null && CustomItem.CustomData is IArmorData armorData)
                            {
                                armorData.HeadProtection = armor.Base.HelmetEfficacy;
                                armorData.BodyProtection = armor.Base.VestEfficacy;
                                armorData.StaminaUseMultiplier = armor.Base._staminaUseMultiplier;
                                armorData.StaminaRegenMultiplier = armor.Base.StaminaRegenMultiplier;
                            }
                            break;
                        }
                    case CustomItemType.Weapon:
                        {
                            FirearmItem firearm = Item as FirearmItem;
                            IWeaponData weaponData = CustomItem.CustomData as IWeaponData;
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
                                }
                            }

                            weaponData.MaxAmmo = MagazineModule.AmmoStored;
                            weaponData.Damage = HitscanHitregModule.BaseDamage;
                            weaponData.Penetration = HitscanHitregModule.BasePenetration;
                            weaponData.Inaccuracy = HitscanHitregModule.BaseBulletInaccuracy;
                            weaponData.DamageFalloffDistance = HitscanHitregModule.DamageFalloffDistance;
                            MagazineModule.ServerResyncData();
                            break;
                        }

                    case CustomItemType.MicroHID:
                        MicroHIDItem microHID = Item as MicroHIDItem;
                        IMicroHIDData microHIDData = CustomItem.CustomData as IMicroHIDData;
                        microHIDData.Energy = microHID.Energy;
                        break;

                    case CustomItemType.Light:
                        if (Item.Type is ItemType.Flashlight)
                        {
                            FlashlightItem flashLight = Item as FlashlightItem;
                            IFlashlightData data = CustomItem.CustomData as IFlashlightData;
                            Light.Intensity = 0;
                        }
                        if (Item.Type is ItemType.Lantern)
                        {
                            LanternItem lantern = Item as LanternItem;
                            IFlashlightData data = CustomItem.CustomData as IFlashlightData;
                            Light.Intensity = 0;
                        }
                        break;

                    case CustomItemType.SCPItem:
                        if (Item.Type == ItemType.GunSCP127)
                        {
                            FirearmItem scpFirearm = Item as FirearmItem;
                            ISCP127Data scp127Data = CustomItem.CustomData as ISCP127Data;

                            foreach (ModuleBase module in scpFirearm.Base.Modules)
                            {
                                switch (module)
                                {
                                    case Scp127MagazineModule mag when Scp127MagazineModule == null:
                                        Scp127MagazineModule = mag;
                                        break;

                                    case Scp127Hitscan hitscan when Scp127Hitscan == null:
                                        Scp127Hitscan = hitscan;
                                        break;
                                }
                            }

                            scp127Data.MaxAmmo = Scp127MagazineModule.AmmoStored;
                            scp127Data.Damage = Scp127Hitscan.BaseDamage;
                            scp127Data.Penetration = Scp127Hitscan.BasePenetration;
                            scp127Data.Inaccuracy = Scp127Hitscan.BaseBulletInaccuracy;
                            scp127Data.DamageFalloffDistance = Scp127Hitscan.DamageFalloffDistance;
                            Scp127MagazineModule.ServerResyncData();
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private List<string> GetAttachmentsList()
        {
            if (CustomItem.CustomData is IWeaponData weaponData)
            {
                string attachmentsString = weaponData.Attachments;

                if (string.IsNullOrWhiteSpace(attachmentsString))
                    return [];

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
                return [];
            }
        }

        internal IEnumerator<float> LightFacingForward()
        {
            for (; ; )
            {
                if (Owner == null)
                    Light.Intensity = 0;
                if (Owner.CurrentItem == null)
                    Light.Intensity = 0;
                if (Serial != Owner.CurrentItem.Serial)
                    Light.Intensity = 0;

                if (Toggled)
                {
                    IFlashlightData data = CustomItem.CustomData as IFlashlightData;
                    Light.Intensity = data.Intensity;
                    Light.Base.transform.forward = Owner.Camera.forward;
                }

                yield return Timing.WaitForOneFrame;
            }
        }
        
        public void LoadBadge(Player player)
        {
            if (string.IsNullOrWhiteSpace(CustomItem.BadgeColor) || string.IsNullOrWhiteSpace(CustomItem.BadgeName))
                return;

            Triplet<string, string, bool>? badge = null;
            if (CustomItem.BadgeName is not null && CustomItem.BadgeName.Length > 1 && CustomItem.BadgeColor is not null && CustomItem.BadgeColor.Length > 2)
            {
                badge = new(player.GroupName ?? "", player.GroupColor ?? "", player.ReferenceHub.serverRoles.HasBadgeHidden);
                LogManager.Debug($"Badge detected, putting {CustomItem.BadgeName}@{CustomItem.BadgeColor} to player {player.PlayerId}");

                player.GroupName = CustomItem.BadgeName.Replace("@hidden", "");
                player.GroupColor = CustomItem.BadgeColor;

                if (CustomItem.BadgeName.Contains("@hidden"))
                    if (player.ReferenceHub.serverRoles.TryHideTag())
                        LogManager.Debug("Tag successfully hidden!");
            }
        }

        public void ResetBadge(Player player)
        {
            if (string.IsNullOrWhiteSpace(CustomItem.BadgeColor) || string.IsNullOrWhiteSpace(CustomItem.BadgeName))
                return;

            if (player.ReferenceHub.serverRoles.HasBadgeHidden)
                player.ReferenceHub.serverRoles.RefreshHiddenTag();
            else
                player.ReferenceHub.serverRoles.RefreshLocalTag();

            if (Plugin.Instance.Config.EnableCreditTags && player.UserId == "76561199150506472@steam")
            {
                player.GroupName = "UCI Lead Developer";
                player.GroupColor = "emerald";
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

        public void OnDrop(PlayerDroppedItemEventArgs dropped)
        {
            Pickup = dropped.Pickup;
            Item = null;
            Owner = null;
            SaveProperties();
            Serial = Pickup.Serial;
            HandleEvent(dropped.Player, ItemEvents.Drop, dropped.Pickup.Serial);
        }

        public void OnDrop(PickupCreatedEventArgs created)
        {
            Pickup = created.Pickup;
            Item = null;
            Owner = null;
            SaveProperties();
            Serial = Pickup.Serial;
        }

        public bool HasModule(CustomFlags flag)
        {
            if (CustomItem.CustomFlags.HasValue && CustomItem.CustomFlags.Value.HasFlag(flag))
            {
                CheckingCustomFlagEventArgs args = new(CustomItem, flag);
                Events.Handlers.CustomItemEvents.OnCheckingCustomFlag(args);
                if (!args.IsAllowed)
                    return false;

                LogManager.Silent($"{CustomItem.Name} has {flag}");

                Events.Handlers.CustomItemEvents.OnCheckedCustomFlag(new(CustomItem, flag));
                return true;
            }
            else
                return false;
        }

        private static readonly Dictionary<Player, Dictionary<ushort, bool>> _cooldownStates = [];

        public void HandleEvent(Player player, ItemEvents itemEvent, ushort playerItemSerial)
        {
            if (CustomItem.CustomItemType == CustomItemType.Item)
            {
                IItemData itemData = CustomItem.CustomData as IItemData;
                foreach (ItemDataList data in itemData.Data)
                {
                    if (data.Event == itemEvent)
                    {
                        if (IsOnCooldown(player, playerItemSerial))
                        {
                            LogManager.Debug($"{CustomItem.Name} is still on cooldown.");
                            continue;
                        }

                        LogManager.Debug($"Firing events for item {CustomItem.Name}");
                        Player randomPlayer = Player.ReadyList.ToList().RandomItem();
                        if (data.Command is not null && data.Command.Length > 2)
                        {
                            string processedCommand = data.Command
                                .Replace("{p_id}", player.PlayerId.ToString())
                                .Replace("{rp_id}", randomPlayer.PlayerId.ToString())
                                .Replace("{p_pos}", player.Position.ToString())
                                .Replace("{p_role}", player.Role.ToString())
                                .Replace("{p_health}", player.Health.ToString())
                                .Replace("{p_zone}", player.Zone.ToString())
                                .Replace("{p_room}", player.Room.ToString())
                                .Replace("{p_rotation}", player.Rotation.ToString())
                                .Replace("{pj_pos}", PlayerHandler.DetonationPosition.ToString());

                            if (data.Command.Contains("{p_id}") || data.Command.Contains("{rp_id}") ||
                                data.Command.Contains("{p_pos}") || data.Command.Contains("{p_role}") ||
                                data.Command.Contains("{p_health}") || data.Command.Contains("{p_zone}") ||
                                data.Command.Contains("{p_room}") || data.Command.Contains("{p_rotation}") ||
                                data.Command.Contains("{pj_pos}"))
                            {
                                RunningCustomItemCommandEventArgs args = new(processedCommand, data.Command, this);
                                Events.Handlers.CustomItemEvents.OnRunningCustomItemCommand(args);

                                if (args.IsAllowed)
                                    Server.RunCommand(processedCommand, player.GetSender());

                                Events.Handlers.CustomItemEvents.OnRanCustomItemCommand(new(processedCommand, data.Command, this));
                            }
                            else
                            {
                                RunningCustomItemCommandEventArgs args = new(processedCommand, data.Command, this);
                                Events.Handlers.CustomItemEvents.OnRunningCustomItemCommand(args);

                                if (args.IsAllowed)
                                    Server.RunCommand(processedCommand, new SilentCommandSender());

                                Events.Handlers.CustomItemEvents.OnRanCustomItemCommand(new(processedCommand, data.Command, this));
                            }
                        }
                        StartCooldown(player, playerItemSerial, data.CoolDown);

                        Utilities.ParseResponse(player, itemData);

                        if (data.DestroyAfterUse)
                            Destroy();
                    }
                }
            }
        }

        public bool IsOnCooldown(Player player, ushort serial)
        {
            if (_cooldownStates.TryGetValue(player, out Dictionary<ushort, bool> itemStates))
            {
                if (itemStates.TryGetValue(serial, out bool isOnCooldown))
                    return isOnCooldown;
            }
            return false;
        }

        public void StartCooldown(Player player, ushort serial, float cooldown)
        {
            if (!_cooldownStates.ContainsKey(player))
                _cooldownStates[player] = new Dictionary<ushort, bool>();

            _cooldownStates[player][serial] = true;
            Timing.RunCoroutine(CooldownCoroutine(player, serial, cooldown));
        }

        public IEnumerator<float> CooldownCoroutine(Player player, ushort serial, float cooldown)
        {
            yield return Timing.WaitForSeconds(cooldown);

            if (_cooldownStates.TryGetValue(player, out Dictionary<ushort, bool> itemStates))
            {
                itemStates[serial] = false;
            }
            LogManager.Debug($"Cooldown complete for item {CustomItem.Name}");
        }

        public void HandleSelectedDisplayHint()
        {
            if (Plugin.Instance.Config.SelectedMessage.Length > 1)
                Owner.SendHint(Plugin.Instance.Config.SelectedMessage.Replace("%name%", CustomItem.Name).Replace("%desc%", CustomItem.Description).Replace("%description%", CustomItem.Description), Plugin.Instance.Config.SelectedMessageDuration);

        }

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
                        IMedikitData medikitData = CustomItem.CustomData as IMedikitData;
                        Owner.Heal(medikitData.Health);
                        break;
                    case CustomItemType.Painkillers:
                        Timing.RunCoroutine(Utilities.PainkillersCoroutine(Owner, CustomItem.CustomData as IPainkillersData));
                        break;
                    case CustomItemType.Adrenaline:
                        IAdrenalineData adrenalineData = CustomItem.CustomData as IAdrenalineData;
                        Owner.CreateAhpProcess(adrenalineData.Amount, limit: 1000f, decay: adrenalineData.Decay, efficacy: adrenalineData.Efficacy, sustain: adrenalineData.Sustain, adrenalineData.Persistant);
                        break;
                    default:
                        return false;
                }

                HandleEvent(Owner, ItemEvents.Use, Serial);
                if (!CustomItem.Reusable)
                    Owner.RemoveItem(item.Base);

                return true;
            }

            return false;
        }

        public void Destroy()
        {
            List.Remove(this);
            bySerial.TryRemove(Serial, out _);
            _activeSerials.TryRemove(Serial, out _);

            if (Owner?.PlayerId != null && byPlayerId.TryGetValue(Owner.PlayerId, out var bag))
            {
                ConcurrentBag<SummonedCustomItem> newBag = new(bag.Where(sci => sci.Serial != Serial));
                if (newBag.IsEmpty)
                    byPlayerId.TryRemove(Owner.PlayerId, out _);
                else
                    byPlayerId[Owner.PlayerId] = newBag;
            }

            if (IsPickup)
                Pickup?.Destroy();
            else if (Item != null)
                Owner?.RemoveItem(Item.Base);

            Pickup = null;
            Item = null;
            Serial = 0;
            Owner = null;
            CustomItem = null;
        }

        public static bool TryGet(ushort serial, out SummonedCustomItem item)
        {
            if (!_activeSerials.ContainsKey(serial))
            {
                item = null;
                return false;
            }

            return bySerial.TryGetValue(serial, out item);
        }
        
        public static SummonedCustomItem Get(ushort serial) => _activeSerials.ContainsKey(serial) && bySerial.TryGetValue(serial, out var item) ? item : null;

        public static SummonedCustomItem Get(Player owner, ushort serial)
        {
            if (owner?.PlayerId == null || !_activeSerials.ContainsKey(serial))
                return null;

            if (!byPlayerId.TryGetValue(owner.PlayerId, out var bag))
                return null;

            return bag.FirstOrDefault(sci => sci.Serial == serial);
        }
        
        public static List<SummonedCustomItem> Get(ItemType item)
        {
            List<SummonedCustomItem> result = [];
            List<SummonedCustomItem> items = List.Count > 100 ? List.AsParallel().Where(sci => sci.CustomItem.Item == item).ToList() : List.Where(sci => sci.CustomItem.Item == item).ToList();

            return items;
        }

        public static List<SummonedCustomItem> Get(Player owner)
        {
            if (owner?.PlayerId == null)
                return [];

            if (byPlayerId.TryGetValue(owner.PlayerId, out var bag))
                return bag.ToList();

            return [];
        }
    }
}