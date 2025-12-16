using Interactables.Interobjects.DoorUtils;
using InventorySystem;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Scp127;
using InventorySystem.Items.Jailbird;
using InventorySystem.Items.Keycards;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Features.Wrappers;
using MEC;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using InventorySystem.Items.Firearms.Extensions;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UncomplicatedCustomItems.API.Interfaces;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;
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
using ThrowableItem = LabApi.Features.Wrappers.ThrowableItem;

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

        internal static CoroutineHandle RegenHandle;

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
            if (Item is FirearmItem firearm)
                StartAmmoRegen(firearm);
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
                    case CustomItemType.FlashGrenade when Item is ThrowableItem throwable && CustomItem.CustomData is FlashGrenadeData flashData:
                        HandleFlashbangItem(throwable, flashData);
                        break;

                    case CustomItemType.ExplosiveGrenade when Item is ThrowableItem throwable && CustomItem.CustomData is ExplosiveGrenadeData grenadeData:
                        HandleGrenadeItem(throwable, grenadeData);
                        break;

                    case CustomItemType.Keycard when Item is KeycardItem keycard && CustomItem.CustomData is KeycardData keycardData:
                        HandleKeycardItem(keycard, keycardData);
                        break;

                    case CustomItemType.Armor when Item is Armor armor && CustomItem.CustomData is ArmorData armorData:
                        armor.Base.HelmetEfficacy = armorData.HeadProtection;
                        armor.Base.VestEfficacy = armorData.BodyProtection;
                        armor.Base._staminaUseMultiplier = armorData.StaminaUseMultiplier;
                        break;

                    case CustomItemType.Weapon when Item is FirearmItem firearm && CustomItem.CustomData is WeaponData weaponData:
                        HandleWeaponItem(firearm, weaponData);
                        break;

                    case CustomItemType.Jailbird when Item is Jailbird jailbird && CustomItem.CustomData is JailbirdData jbData:
                        jailbird.Base._flashedDuration = jbData.FlashDuration;
                        jailbird.Base._hitregRadius = jbData.Radius;
                        jailbird.Base._chargeDamage = jbData.ChargeDamage;
                        jailbird.Base.MeleeDamage = jbData.MeleeDamage;
                        JailbirdDeteriorationTracker.ReceivedStates[jailbird.Serial] = jbData.WearState;
                        PropertiesSet = true;
                        break;

                    case CustomItemType.MicroHID when Item is MicroHIDItem microHID && CustomItem.CustomData is MicroHIDData microData:
                        microHID.Energy = microData.Energy;
                        if (microData.Broken)
                            microHID.Base.BrokenSync.ServerSetBroken();
                        break;

                    case CustomItemType.ParticleDisruptor when Item is ParticleDisruptorItem pd && CustomItem.CustomData is ParticleDisruptorData pdData:
                        pd.Base.TryGetModule<DisruptorHitregModule>(out var hitregModule);
                        hitregModule.BasePenetration = pdData.Penetration;
                        break;

                    case CustomItemType.Light when Item is FlashlightItem flashLight && !PropertiesSet && CustomItem.CustomData is FlashlightData flData:
                        CreateAndAttachLightToItem(flashLight.CurrentOwner.GameObject.transform, flData, new Vector3(0.05f, 0.35f, 0.5f));
                        LogManager.Info($"{Light.Position}, {Light.Parent}, {flashLight.IsEmitting}, {Light.Base.enabled}");
                        break;

                    case CustomItemType.Light when Item is LanternItem lantern && !PropertiesSet && CustomItem.CustomData is FlashlightData lanData:
                        CreateAndAttachLightToItem(lantern.CurrentOwner.GameObject.transform, lanData, new Vector3(0.05f, 0f, 0.5f));
                        break;

                    case CustomItemType.SCPItem:
                        HandleSCPItemForItem();
                        break;
                }
            }
            else if (IsPickup)
            {
                switch (CustomItem.CustomItemType)
                {
                    case CustomItemType.Keycard when CustomItem.CustomData is KeycardData keycardData:
                        HandleKeycardPickup(keycardData);
                        break;

                    case CustomItemType.Weapon when CustomItem.CustomData is WeaponData weaponData:
                        HandleWeaponPickup(weaponData);
                        break;

                    case CustomItemType.MicroHID when CustomItem.CustomData is MicroHIDData microData:
                        {
                            MicroHIDPickup microHID = (MicroHIDPickup)Pickup;
                            Pickup.Base.Info.ItemId.TryGetTemplate<InventorySystem.Items.MicroHID.MicroHIDItem>(out var microHIDItem);
                            microHIDItem.ItemSerial = microHID.Serial;
                            microHIDItem.EnergyManager.ServerSetEnergy(microHIDItem.ItemSerial, microData.Energy);
                        }
                        break;

                    case CustomItemType.ParticleDisruptor when CustomItem.CustomData is ParticleDisruptorData pdData:
                        Pickup.Base.Info.ItemId.TryGetTemplate<ParticleDisruptor>(out var particleDisruptor);
                        particleDisruptor.TryGetModule<DisruptorHitregModule>(out var hitregModule2);
                        hitregModule2.BasePenetration = pdData.Penetration;
                        break;

                    case CustomItemType.Light:
                        HandlePickupLight();
                        break;

                    case CustomItemType.SCPItem:
                        HandleSCPItemForPickup();
                        break;
                }
            }
        }

        private void HandleFlashbangItem(ThrowableItem throwable, FlashGrenadeData data)
        {
            LogManager.Debug($"Throwable Flashbang");
            throwable.Base._pinPullTime = data.PinPullTime;
            throwable.Base._repickupable = data.Repickable;
        }

        private void HandleGrenadeItem(ThrowableItem throwable, ExplosiveGrenadeData data)
        {
            LogManager.Debug($"Throwable Grenade");
            throwable.Base._pinPullTime = data.PinPullTime;
            throwable.Base._repickupable = data.Repickable;
        }

        public void HandleKeycardItem(KeycardItem keycardItem, IKeycardData kd)
        {
            ColorUtility.TryParseHtmlString(kd.PermissionsColor, out Color permissionsColor);
            ColorUtility.TryParseHtmlString(kd.TintColor, out Color tintColor);
            ColorUtility.TryParseHtmlString(kd.LabelColor, out Color labelColor);

            Color32 permissionsColor32 = permissionsColor;
            Color32 tintColor32 = tintColor;
            Color32 labelColor32 = labelColor;

            KeycardLevels permissions = new(kd.Containment, kd.Armory, kd.Admin);

            if (!keycardItem.Base.Customizable)
            {
                LogManager.Warn($"{CustomItem.Name} is not customizable!\nThe item field must be 'KeycardCustomMetalCase', 'KeycardCustomManagement', 'KeycardCustomSite02', or 'KeycardCustomTaskForce'!");
                return;
            }

            CustomKeycard customKeycard = new(keycardItem.Base);
            if (!NameApplied)
                customKeycard.NameTag = kd.Name;

            customKeycard.SerialNumber = kd.SerialNumber;
            customKeycard.WearIndex = kd.WearDetail;
            customKeycard.RankIndex = kd.Rank;
            customKeycard.LabelColor = labelColor32;
            customKeycard.LabelText = kd.Label;
            customKeycard.ItemName = CustomItem.Name;
            customKeycard.CardColor = tintColor32;
            customKeycard.PermissionsColor = permissionsColor32;
            customKeycard.Permissions = permissions;

            LogManager.Debug($"{labelColor32} {labelColor} {kd.LabelColor}");

            KeycardDetailSynchronizer.Database.Remove(keycardItem.Serial);
            KeycardDetailSynchronizer.ServerProcessItem(keycardItem.Base);
            NameApplied = true;
        }

        public void HandleKeycardPickup(IKeycardData kd)
        {
            LabApi.Features.Wrappers.KeycardPickup keycardPickup = (LabApi.Features.Wrappers.KeycardPickup)LabApi.Features.Wrappers.KeycardPickup.Create(CustomItem.Item, Pickup.Position);

            ColorUtility.TryParseHtmlString(kd.PermissionsColor, out Color permissionsColor);
            ColorUtility.TryParseHtmlString(kd.TintColor, out Color tintColor);
            ColorUtility.TryParseHtmlString(kd.LabelColor, out Color labelColor);

            Color32 permissionsColor32 = permissionsColor;
            Color32 tintColor32 = tintColor;
            Color32 labelColor32 = labelColor;
            KeycardLevels permissions = new(kd.Containment, kd.Armory, kd.Admin);

            keycardPickup.Base.Info.ItemId.TryGetTemplate<InventorySystem.Items.Keycards.KeycardItem>(out var item);
            item.ItemSerial = keycardPickup.Serial;

            CustomKeycard customKeycard = new(item);
            customKeycard.SerialNumber = kd.SerialNumber;
            customKeycard.WearIndex = kd.WearDetail;
            customKeycard.RankIndex = kd.Rank;
            customKeycard.LabelColor = labelColor32;
            customKeycard.LabelText = kd.Label;
            customKeycard.ItemName = CustomItem.Name;
            customKeycard.CardColor = tintColor32;
            customKeycard.PermissionsColor = permissionsColor32;
            customKeycard.Permissions = permissions;

            LogManager.Debug($"{labelColor32} {labelColor} {kd.LabelColor}");
            KeycardDetailSynchronizer.Database.Remove(keycardPickup.Serial);
            KeycardDetailSynchronizer.ServerProcessPickup(keycardPickup.Base);

            Pickup.Destroy();
            keycardPickup.Spawn();
            Pickup = keycardPickup;
            Serial = Pickup.Serial;
        }

        public void HandleWeaponItem(FirearmItem firearmItem, IWeaponData wd)
        {
            firearmItem.Base.TryGetModule<MagazineModule>(out var mag);
            MagazineModule = mag;

            firearmItem.Base.TryGetModule<HitscanHitregModuleBase>(out var hitscan);
            HitscanHitregModule = hitscan;

            firearmItem.Base.ApplyAttachmentsCode(firearmItem.GetCodeFromAttachmentNamesRaw(GetAttachments()), true);
            if (!PropertiesSet && MagazineModule != null)
            {
                if (!MagazineModule.MagazineInserted)
                    MagazineModule.ServerInsertEmptyMagazine();
                MagazineModule.ServerSetInstanceAmmo(Serial, wd.MaxAmmo);

                if (firearmItem.ActionModule is AutomaticActionModule actionModule)
                {
                    actionModule.AmmoStored = wd.MaxBarrelAmmo;
                    actionModule.Cocked = true;
                    actionModule.BoltLocked = false;
                    actionModule.ServerResync();
                }
            }

            if (HitscanHitregModule != null)
            {
                HitscanHitregModule.BaseDamage = wd.Damage;
                HitscanHitregModule.BasePenetration = wd.Penetration;
                HitscanHitregModule.BaseBulletInaccuracy = wd.Inaccuracy;
                HitscanHitregModule.DamageFalloffDistance = wd.DamageFalloffDistance;
            }

            MagazineModule?.ServerResyncData();
            PropertiesSet = true;
        }

        public void HandleWeaponPickup(IWeaponData wd)
        {
            var firearmPickup = (LabApi.Features.Wrappers.FirearmPickup)LabApi.Features.Wrappers.FirearmPickup.Create(CustomItem.Item, Pickup.Position);
            Firearm firearm = AttachmentPreview.Get(firearmPickup.Base.CurId);
            firearm.ItemSerial = firearmPickup.Serial;

            firearm.TryGetModule<MagazineModule>(out var mag);
            MagazineModule = mag;
            firearm.TryGetModule<HitscanHitregModuleBase>(out var hitscan);
            HitscanHitregModule = hitscan;
            firearm.TryGetModule<AutomaticActionModule>(out var actionModule);
            
            if (wd.Attachments.Count() > 1)
            {
                foreach (AttachmentName attachment in GetAttachments())
                {
                    if (firearmPickup.Base.TryApplyAttachment(attachment))
                        LogManager.Debug($"Added {attachment} to {CustomItem.Name}");
                    else
                        LogManager.Error($"Failed to add {attachment} to {CustomItem.Name}");
                }
            }
            else
            {
                LogManager.Debug($"No attachments found for {CustomItem.Name} - {CustomItem.Id} applying random attachments...");
                AttachmentCodeSync.ServerSetCode(firearmPickup.Base.Info.Serial, AttachmentsUtils.GetRandomAttachmentsCode(firearmPickup.Base.Info.ItemId));
            }

            if (!PropertiesSet)
            {
                if (!MagazineModule.MagazineInserted)
                    MagazineModule.ServerInsertEmptyMagazine();

                MagazineModule.ServerSetInstanceAmmo(firearmPickup.Serial, wd.MaxAmmo);
                firearmPickup.Base.Worldmodel.TryGetExtension<WorldmodelMagazineExtension>(out var extension);
                extension.UpdateAllMags();
            }

            if (HitscanHitregModule is not null)
            {
                HitscanHitregModule.BaseDamage = wd.Damage;
                HitscanHitregModule.BasePenetration = wd.Penetration;
                HitscanHitregModule.BaseBulletInaccuracy = wd.Inaccuracy;
                HitscanHitregModule.DamageFalloffDistance = wd.DamageFalloffDistance;
            }

            Pickup.Destroy();
            firearmPickup.Spawn();
            Pickup = firearmPickup;
            Serial = Pickup.Serial;
            PropertiesSet = true;
        }

        public void CreateAndAttachLightToItem(Transform parentTransform, IFlashlightData data, Vector3 localPos)
        {
            Light newLight = Light.Create(parentTransform);
            ColorUtility.TryParseHtmlString(data.HexColor, out Color color);

            newLight.Color = color;
            newLight.Type = data.LightType;
            newLight.Range = data.Range;
            newLight.ShadowType = data.ShadowType;
            newLight.ShadowStrength = data.ShadowStrength;
            newLight.Intensity = data.Intensity;
            newLight.Shape = data.Shape;
            newLight.SpotAngle = data.SpotLightAngle;
            newLight.Transform.localPosition = localPos;
            newLight.SyncInterval = 0;
            Light = newLight;
            Timing.RunCoroutine(LightFacingForward());
        }

        public void HandlePickupLight()
        {
            if (Pickup.Type is ItemType.Flashlight && !PropertiesSet)
            {
                Pickup.Base.Info.ItemId.TryGetTemplate<InventorySystem.Items.ToggleableLights.Flashlight.FlashlightItem>(out var flashItem);
                IFlashlightData data = CustomItem.CustomData as IFlashlightData;
                LightSourceToy newLight = Light.Create(flashItem.gameObject.transform.position);
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
                LightSourceToy newLight = Light.Create(lantern.gameObject.transform.position);
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
        }

        public void HandleSCPItemForItem()
        {
            if (Item.Type == ItemType.SCP018 && CustomItem.CustomData is ISCP018Data scp018Data)
            {
                var scp018throwableItem = Item as LabApi.Features.Wrappers.ThrowableItem;
                var scp018 = scp018throwableItem.Base.Projectile as InventorySystem.Items.ThrowableProjectiles.Scp018Projectile;
                scp018._fuseTime = scp018Data.FuseTime;
                scp018._friendlyFireTime = scp018Data.FriendlyFireTime;
                return;
            }

            if ((Item.Type == ItemType.SCP244a || Item.Type == ItemType.SCP244b) && CustomItem.CustomData is ISCP244Data scp244Data)
            {
                LogManager.Debug($"SCPItem is SCP-244");
                Scp244 scp244 = Item as Scp244;
                scp244.Base._primed = scp244Data.Primed;
                return;
            }

            if (Item.Type == ItemType.GunSCP127 && CustomItem.CustomData is ISCP127Data scp127Data)
            {
                LogManager.Debug($"SCPItem is SCP-127");
                FirearmItem scpFirearm = Item as FirearmItem;
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
        }

        public void HandleSCPItemForPickup()
        {
            if ((Pickup.Type == ItemType.SCP244a || Pickup.Type == ItemType.SCP244b) && CustomItem.CustomData is ISCP244Data s244a)
            {
                LogManager.Debug($"SCPItem is SCP-244");
                Scp244Pickup scp244Pickup = (Scp244Pickup)Scp244Pickup.Create(CustomItem.Item, Pickup.Position);
                scp244Pickup.Base.MaxDiameter = s244a.MaxDiameter;
                scp244Pickup.Base._activationDot = s244a.ActivationDot;
                scp244Pickup.Base._health = s244a.Health;
                scp244Pickup.Base.enabled = s244a.Primed;
                Pickup.Destroy();
                scp244Pickup.Spawn();
                Pickup = scp244Pickup;
                Serial = Pickup.Serial;
                return;
            }

            if (Pickup.Type == ItemType.SCP018 && CustomItem.CustomData is ISCP018Data s018)
            {
                Scp018 scp018 = Pickup as Scp018;
                scp018.Base._fuseTime = s018.FuseTime;
                scp018.Base._friendlyFireTime = s018.FriendlyFireTime;
                return;
            }

            if (Pickup.Type == ItemType.GunSCP127 && CustomItem.CustomData is ISCP127Data s127)
            {
                LogManager.Debug($"SCPItem is SCP-127");
                var scpFirearmPickup = (LabApi.Features.Wrappers.FirearmPickup)LabApi.Features.Wrappers.FirearmPickup.Create(CustomItem.Item, Pickup.Position);
                scpFirearmPickup.Base.Info.ItemId.TryGetTemplate<Firearm>(out var scpFirearm);
                scpFirearm.ItemSerial = scpFirearmPickup.Serial;
                scpFirearm.TryGetModule<Scp127MagazineModule>(out var scp127magazine);
                Scp127MagazineModule = scp127magazine;
                scpFirearm.TryGetModule<Scp127Hitscan>(out var scp127hitscan);
                Scp127Hitscan = scp127hitscan;

                Scp127MagazineModule.MagazineInserted = true;
                if (!PropertiesSet)
                    Scp127MagazineModule.AmmoStored = s127.MaxAmmo;

                Scp127Hitscan.BaseDamage = s127.Damage;
                Scp127Hitscan.BasePenetration = s127.Penetration;
                Scp127Hitscan.BaseBulletInaccuracy = s127.Inaccuracy;
                Scp127Hitscan.DamageFalloffDistance = s127.DamageFalloffDistance;
                Scp127MagazineModule.ServerResyncData();

                Pickup.Destroy();
                scpFirearmPickup.Spawn();
                Pickup = scpFirearmPickup;
                Serial = Pickup.Serial;
                PropertiesSet = true;
            }
        }

        public void SaveProperties()
        {
            if (Item is null)
                return;

            switch (CustomItem.CustomItemType)
            {
                case CustomItemType.Armor when Item is Armor armor && CustomItem.CustomData is ArmorData armorData:
                    armorData.HeadProtection = armor.Base.HelmetEfficacy;
                    armorData.BodyProtection = armor.Base.VestEfficacy;
                    armorData.StaminaUseMultiplier = armor.Base._staminaUseMultiplier;
                    armorData.StaminaRegenMultiplier = armor.Base.StaminaRegenMultiplier;
                    break;

                case CustomItemType.Weapon when Item is FirearmItem firearm && CustomItem.CustomData is WeaponData weaponData:
                    foreach (ModuleBase module in firearm.Base.Modules)
                    {
                        switch (module)
                        {
                            case MagazineModule mag when MagazineModule == null:
                                MagazineModule = mag;
                                break;
                            case HitscanHitregModuleBase hitscan when HitscanHitregModule == null:
                                HitscanHitregModule = hitscan;
                                break;
                        }
                    }

                    if (MagazineModule != null && HitscanHitregModule != null)
                    {
                        weaponData.MaxAmmo = MagazineModule.AmmoStored;
                        weaponData.Damage = HitscanHitregModule.BaseDamage;
                        weaponData.Penetration = HitscanHitregModule.BasePenetration;
                        weaponData.Inaccuracy = HitscanHitregModule.BaseBulletInaccuracy;
                        weaponData.DamageFalloffDistance = HitscanHitregModule.DamageFalloffDistance;
                        MagazineModule.ServerResyncData();
                    }
                    break;

                case CustomItemType.MicroHID when Item is MicroHIDItem microHID && CustomItem.CustomData is MicroHIDData microHIDData:
                    microHIDData.Energy = microHID.Energy;
                    break;

                case CustomItemType.Light when Item.Type is ItemType.Flashlight && CustomItem.CustomData is FlashlightData:
                    Light.Intensity = 0;
                    break;

                case CustomItemType.Light when Item.Type is ItemType.Lantern && CustomItem.CustomData is FlashlightData:
                    Light.Intensity = 0;
                    break;

                case CustomItemType.SCPItem when Item is FirearmItem scpFirearm && CustomItem.CustomData is SCP127Data scp127Data:
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

                    if (Scp127MagazineModule != null && Scp127Hitscan != null)
                    {
                        scp127Data.MaxAmmo = Scp127MagazineModule.AmmoStored;
                        scp127Data.Damage = Scp127Hitscan.BaseDamage;
                        scp127Data.Penetration = Scp127Hitscan.BasePenetration;
                        scp127Data.Inaccuracy = Scp127Hitscan.BaseBulletInaccuracy;
                        scp127Data.DamageFalloffDistance = Scp127Hitscan.DamageFalloffDistance;
                        Scp127MagazineModule.ServerResyncData();
                    }
                    break;
            }
        }

        private AttachmentName[] GetAttachments()
        {
            if (CustomItem.CustomData is IWeaponData weaponData)
            {
                List<AttachmentName> names = [];
                string attachmentsString = weaponData.Attachments;

                if (string.IsNullOrWhiteSpace(attachmentsString))
                    return [];

                List<string> attachmentsList = attachmentsString
                    .Split(',')
                    .Select(att => att.Trim())
                    .Where(att => !string.IsNullOrEmpty(att))
                    .ToList();

                foreach (string attachment in attachmentsList)
                {
                    if (Enum.TryParse(attachment, out AttachmentName name))
                        names.Add(name);
                }

                return names.ToArray();
            }
            else
            {
                LogManager.Warn("CustomData is not in the expected IWeaponData format or is null.");
                return [];
            }
        }

        internal void StopAmmoRegen()
        {
            if (RegenHandle.IsRunning)
                Timing.KillCoroutines(RegenHandle);
        }

        internal void PauseAmmoRegen(FirearmItem firearm, float pauseTime)
        {
            if (RegenHandle.IsRunning)
            {
                Timing.KillCoroutines(RegenHandle);
                Timing.CallDelayed(pauseTime, () => StartAmmoRegen(firearm));
            }

        }

        internal void StartAmmoRegen(FirearmItem firearm)
        {
            if (HasModule(CustomFlags.AmmoRegen))
                RegenHandle = Timing.RunCoroutine(AmmoRegen(firearm));
        }

        internal IEnumerator<float> AmmoRegen(FirearmItem firearm)
        {
            for (; ; )
            {
                if (!HasModule(CustomFlags.AmmoRegen))
                    yield break;

                AmmoRegenSettings regen = CustomItem.FlagSettings.AmmoRegenSettings?.FirstOrDefault();
                if (firearm.StoredAmmo != firearm.MaxAmmo)
                    firearm.StoredAmmo = Math.Min(firearm.StoredAmmo + regen.AmmoPerInterval, firearm.MaxAmmo);

                yield return regen.RegenInterval;
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

            ServerRoles serverRoles = player.ReferenceHub?.serverRoles;
            if (serverRoles == null)
            {
                LogManager.Debug("LoadBadge aborted: ServerRole not available yet.");
                return;
            }

            if (CustomItem.BadgeName is not null && CustomItem.BadgeName.Length > 1 && CustomItem.BadgeColor is not null && CustomItem.BadgeColor.Length > 2)
            {
                LogManager.Debug($"Badge detected, putting {CustomItem.BadgeName}@{CustomItem.BadgeColor} to player {player.PlayerId}");

                player.GroupName = CustomItem.BadgeName;
                player.GroupColor = CustomItem.BadgeColor;

                if (CustomItem.BadgeName.Contains("@hidden"))
                    if (serverRoles.TryHideTag())
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

            if (Plugin.Instance.Config.EnableCreditTags)
                Plugin.HttpManager.ApplyCreditTag(player);

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

        public void OnThrew(PlayerThrewProjectileEventArgs ev)
        {
            Pickup = ev.Projectile;
            Item = null;
            Owner = ev.Projectile.LastOwner;
            Serial = ev.Projectile.Serial;
        }

        public void OnDetonated(ProjectileExplodedEventArgs ev)
        {
            Destroy();
        }

        public static bool HasFlagFast(CustomFlags flags, CustomFlags flag) => (flags & flag) == flag;

        public bool HasModule(CustomFlags flag)
        {
            if (CustomItem.CustomFlags.HasValue && HasFlagFast(CustomItem.CustomFlags.Value, flag))
            {
                CheckingCustomFlagEventArgs args = new(CustomItem, flag);
                Events.Handlers.CustomItemEvents.OnCheckingCustomFlag(args);

                if (!args.IsAllowed)
                    return false;

                LogManager.Silent($"{CustomItem.Name} has {flag}");
                Events.Handlers.CustomItemEvents.OnCheckedCustomFlag(new(CustomItem, flag));
                return true;
            }

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
                                .Replace("{pj_pos}", PlayerHandler.DetonationPosition.ToString())
                                .Replace("{pj_pos_mer}", PlayerHandler.DetonationPosition.ToString().Replace(",", " "))
                                .Replace("{p_pos_mer}", player.Position.ToString().Replace(",", " "));

                            if (data.Command.Contains("{p_id}") || data.Command.Contains("{rp_id}") ||
                                data.Command.Contains("{p_pos}") || data.Command.Contains("{p_role}") ||
                                data.Command.Contains("{p_health}") || data.Command.Contains("{p_zone}") ||
                                data.Command.Contains("{p_room}") || data.Command.Contains("{p_rotation}") ||
                                data.Command.Contains("{pj_pos}") || data.Command.Contains("{pj_pos_mer}") ||
                                data.Command.Contains("{p_pos_mer}"))
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
                _cooldownStates[player] = [];

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

        public void HandleSelectedDisplayHint(Player player)
        {
            if (!string.IsNullOrWhiteSpace(Plugin.Instance.Config.SelectedMessage))
                player.SendHint(Plugin.Instance.Config.SelectedMessage.Replace("%name%", CustomItem.Name).Replace("%desc%", CustomItem.Description).Replace("%description%", CustomItem.Description), Plugin.Instance.Config.SelectedMessageDuration);
        }

        public void HandlePickedUpDisplayHint(Player player)
        {
            if (!string.IsNullOrWhiteSpace(Plugin.Instance.Config.PickedUpMessage))
                player.SendHint(Plugin.Instance.Config.PickedUpMessage.Replace("%name%", CustomItem.Name).Replace("%desc%", CustomItem.Description).Replace("%description%", CustomItem.Description), Plugin.Instance.Config.PickedUpMessageDuration);
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
            StopAmmoRegen();
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