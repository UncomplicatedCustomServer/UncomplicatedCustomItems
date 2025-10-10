using System;
using System.Collections.Generic;
using System.Linq;
using Interactables.Interobjects.DoorUtils;
using InventorySystem;
using InventorySystem.Items.Autosync;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Scp127;
using InventorySystem.Items.Jailbird;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.ThrowableProjectiles;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using Mirror;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Struct;
using UnityEngine;
using Armor = LabApi.Features.Wrappers.BodyArmorItem;
using KeycardItem = LabApi.Features.Wrappers.KeycardItem;
using Scp244 = LabApi.Features.Wrappers.Scp244Item;

namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public class SummonedAPICustomItem
    {
        /// <summary>
        /// Gets a list of every summoned <see cref="APICustomItem"/>
        /// </summary>
        public static List<SummonedAPICustomItem> List => SummonedCustomItems.Values.ToList();

        public static List<ushort> SerialList => SummonedCustomItems.Keys.ToList();

        internal static Dictionary<ushort, SummonedAPICustomItem> SummonedCustomItems { get; set; } = [];

        /// <summary>
        /// The owner of this <see cref="SummonedAPICustomItem"/> instance
        /// </summary>
        public Player Owner;

        /// <summary>
        /// The <see cref="Item"/> of this <see cref="SummonedAPICustomItem"/> instance
        /// </summary>
        public Item Item;

        /// <summary>
        /// The <see cref="Pickup"/> of this <see cref="SummonedAPICustomItem"/> instance
        /// </summary>
        public Pickup Pickup;

        /// <summary>
        /// The serial of this <see cref="SummonedAPICustomItem"/> instance
        /// </summary>
        public ushort Serial;

        /// <summary>
        /// The <see cref="APICustomItem"/> that this <see cref="SummonedAPICustomItem"/> instance represents. 
        /// </summary>
        public APICustomItem CustomItem;

        /// <summary>
        /// Gets whether or not the <see cref="SummonedAPICustomItem"/> instance is a <see cref="Pickup"/>
        /// </summary>
        public bool IsPickup => Pickup is not null;

        private bool NameApplied;

        private bool PropertiesSet;

        public SummonedAPICustomItem(APICustomItem customItem, Player owner, Item item, Pickup pickup, Quaternion rotation = new())
        {
            CustomItem = customItem;
            Owner = owner;
            Item = item;
            Serial = item is not null ? item.Serial : pickup.Serial;
            Pickup = pickup;

            if (!IsPickup)
                SetItemProperties();
            else
                SetPickupProperties();

            
            switch (CustomItem)
            {
                case CustomSCP018 scp018:
                    scp018.RegisterEvents();
                    break;
                case CustomJailbird jailbird:
                    jailbird.RegisterEvents();
                    break;
                case CustomWeapon weapon:
                    weapon.RegisterEvents();
                    break;
                case CustomFlashGrenade flash:
                    flash.RegisterEvents();
                    break;
                case CustomExplosiveGrenade grenade:
                    grenade.RegisterEvents();
                    break;
                case SCPCustomItem scp:
                    scp.RegisterEvents();
                    break;
                default:
                    CustomItem.RegisterEvents();
                    break;
            }

            if (IsPickup)
                Pickup.Rotation = rotation;

            SummonedCustomItems.TryAdd(Serial, this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SummonedAPICustomItem"/> class
        /// using an existing <see cref="Pickup"/>.
        /// </summary>
        /// <param name="customItem">The <see cref="APICustomItem"/> definition this instance represents.</param>
        /// <param name="pickup">The world <see cref="Pickup"/> that represents the item.</param>
        public SummonedAPICustomItem(APICustomItem customItem, Pickup pickup)
            : this(customItem, null, null, pickup) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SummonedAPICustomItem"/> class
        /// by spawning the item in the world at the specified position and rotation.
        /// </summary>
        /// <param name="customItem">The <see cref="APICustomItem"/> definition this instance represents.</param>
        /// <param name="position">The world position to spawn the pickup at.</param>
        /// <param name="rotation">The rotation to apply to the spawned pickup. Defaults to <see cref="Quaternion.identity"/>.</param>
        public SummonedAPICustomItem(APICustomItem customItem, Vector3 position, Quaternion rotation = new())
            : this(customItem, null, null, customItem.Item.CreateAndSpawn(position), rotation) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SummonedAPICustomItem"/> class
        /// by giving the item directly to a player.
        /// </summary>
        /// <param name="customItem">The <see cref="APICustomItem"/> definition this instance represents.</param>
        /// <param name="player">The player who will receive the item.</param>
        public SummonedAPICustomItem(APICustomItem customItem, Player player)
            : this(customItem, player, player.AddItem(customItem.Item), null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SummonedAPICustomItem"/> class
        /// using a pre-created <see cref="Item"/> that belongs to a player.
        /// </summary>
        /// <param name="customItem">The <see cref="APICustomItem"/> definition this instance represents.</param>
        /// <param name="player">The player who owns the item.</param>
        /// <param name="item">The in-inventory <see cref="Item"/> that represents the custom item.</param>
        public SummonedAPICustomItem(APICustomItem customItem, Player player, Item item)
            : this(customItem, player, item, null) { }

        public void Destroy()
        {
            List.Remove(this);
            SummonedCustomItems.Remove(Serial);
            APICustomItem._processedThrowables.Remove(Serial);
            switch (CustomItem)
            {
                case CustomSCP018 scp018:
                    scp018.UnregisterEvents();
                    break;
                case CustomJailbird jailbird:
                    jailbird.UnregisterEvents();
                    break;
                case CustomWeapon weapon:
                    weapon.UnregisterEvents();
                    break;
                case CustomFlashGrenade flash:
                    flash.UnregisterEvents();
                    break;
                case CustomExplosiveGrenade grenade:
                    grenade.UnregisterEvents();
                    break;
                case SCPCustomItem scp:
                    scp.UnregisterEvents();
                    break;
                default:
                    CustomItem.UnregisterEvents();
                    break;
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

        private void SetItemProperties()
        {
            switch (CustomItem)
            {
                case CustomJailbird jailbirdData:
                    LabApi.Features.Wrappers.JailbirdItem jailbird = Item as LabApi.Features.Wrappers.JailbirdItem;
                    ApplyJailbirdStats(jailbird, jailbirdData);
                    break;

                case CustomSCP127 scp127Data:
                    LogManager.Debug($"SCPItem is SCP-127");
                    FirearmItem scpFirearm = Item as FirearmItem;
                    scpFirearm.Base.TryGetModule<Scp127MagazineModule>(out var scp127magazine);
                    scpFirearm.Base.TryGetModule<Scp127Hitscan>(out var scp127hitscan);
                    
                    if (!PropertiesSet)
                        scp127magazine.AmmoStored = scp127Data.MaxAmmo;
                    
                    ApplyFirearmStats(scp127hitscan, scp127Data);
                    scp127magazine.ServerResyncData();
                    PropertiesSet = true;
                    break;

                case CustomKeycard keycardData:
                    KeycardItem keycard = Item as KeycardItem;
                    if (!keycard.Base.Customizable)
                    {
                        LogManager.Warn($"{CustomItem.Name} is not customizable!\nThe item field must be 'KeycardCustomMetalCase', 'KeycardCustomManagement', 'KeycardCustomSite02', or 'KeycardCustomTaskForce'!");
                        return;
                    }

                    Wrappers.CustomKeycard customKeycard = new(keycard.Base);
                    if (!NameApplied)
                        customKeycard.NameTag = keycardData.HolderName;

                    ApplyKeycardData(customKeycard, keycardData);
                    KeycardDetailSynchronizer.Database.Remove(keycard.Serial);
                    KeycardDetailSynchronizer.ServerProcessItem(keycard.Base);
                    NameApplied = true;
                    break;

                case CustomArmor armorData:
                    Armor armor = Item as Armor;
                    armor.Base.HelmetEfficacy = armorData.HeadProtection;
                    armor.Base.VestEfficacy = armorData.BodyProtection;
                    armor.Base._staminaUseMultiplier = armorData.StaminaUseMultiplier;
                    break;

                case CustomWeapon weaponData:
                    FirearmItem firearm = Item as FirearmItem;
                    firearm.Base.TryGetModule<MagazineModule>(out var magazine);
                    firearm.Base.TryGetModule<HitscanHitregModuleBase>(out var hitscan);

                    ApplyAttachments(firearm.Base, weaponData.Attachments);
                    
                    if (!PropertiesSet)
                        magazine.AmmoStored = weaponData.MaxAmmo;
                    
                    ApplyFirearmStats(hitscan, weaponData);
                    magazine.ServerResyncData();
                    PropertiesSet = true;
                    break;

                case CustomSCP244 scp244Data:
                    LogManager.Debug($"SCPItem is SCP-244");
                    Scp244 scp244 = Item as Scp244;
                    scp244.Base._primed = scp244Data.Primed;
                    break;
            }
        }

        private void SetPickupProperties()
        {
            switch (CustomItem)
            {
                case CustomJailbird jailbirdData:
                    LabApi.Features.Wrappers.JailbirdPickup jailbird = Pickup as LabApi.Features.Wrappers.JailbirdPickup;
                    ApplyJailbirdStats(jailbird, jailbirdData);
                    break;

                case CustomKeycard keycardData:
                    var keycard = (LabApi.Features.Wrappers.KeycardPickup)LabApi.Features.Wrappers.KeycardPickup.Create(CustomItem.Item, Pickup.Position);
                    keycard.Base.Info.ItemId.TryGetTemplate<InventorySystem.Items.Keycards.KeycardItem>(out var item);
                    item.ItemSerial = keycard.Serial;

                    Wrappers.CustomKeycard customKeycard = new(item);
                    ApplyKeycardData(customKeycard, keycardData);
                    
                    KeycardDetailSynchronizer.Database.Remove(keycard.Serial);
                    KeycardDetailSynchronizer.ServerProcessPickup(keycard.Base);
                    ReplacePickup(keycard);
                    break;

                case CustomSCP127 scp127Data:
                    LogManager.Debug($"SCPItem is SCP-127");
                    var scpFirearmPickup = (LabApi.Features.Wrappers.FirearmPickup)LabApi.Features.Wrappers.FirearmPickup.Create(CustomItem.Item, Pickup.Position);
                    scpFirearmPickup.Base.Info.ItemId.TryGetTemplate<Firearm>(out var scpFirearm);
                    scpFirearm.ItemSerial = scpFirearmPickup.Serial;
                    scpFirearm.TryGetModule<Scp127MagazineModule>(out var scp127magazine);
                    scpFirearm.TryGetModule<Scp127Hitscan>(out var scp127hitscan);

                    scp127magazine.MagazineInserted = true;
                    if (!PropertiesSet)
                        scp127magazine.AmmoStored = scp127Data.MaxAmmo;
                    
                    ApplyFirearmStats(scp127hitscan, scp127Data);
                    scp127magazine.ServerResyncData();
                    ReplacePickup(scpFirearmPickup);
                    PropertiesSet = true;
                    break;

                case CustomWeapon weaponData:
                    var firearmPickup = (LabApi.Features.Wrappers.FirearmPickup)LabApi.Features.Wrappers.FirearmPickup.Create(CustomItem.Item, Pickup.Position);
                    firearmPickup.Base.Info.ItemId.TryGetTemplate<Firearm>(out var firearm);
                    firearm.ItemSerial = firearmPickup.Serial;
                    firearm.TryGetModule<MagazineModule>(out var magazine);
                    firearm.TryGetModule<HitscanHitregModuleBase>(out var hitscan);

                    if (weaponData.Attachments.Count() > 1)
                        ApplyAttachments(firearm, weaponData.Attachments);
                    else
                    {
                        LogManager.Debug($"No attachments found for {CustomItem.Name} - {CustomItem.Id} applying random attachments...");
                        AttachmentCodeSync.ServerSetCode(firearmPickup.Base.Info.Serial, AttachmentsUtils.GetRandomAttachmentsCode(firearmPickup.Base.Info.ItemId));
                    }

                    magazine.MagazineInserted = true;
                    if (!PropertiesSet)
                        magazine.AmmoStored = weaponData.MaxAmmo;
                    
                    ApplyFirearmStats(hitscan, weaponData);
                    magazine.ServerResyncData();
                    ReplacePickup(firearmPickup);
                    PropertiesSet = true;
                    break;

                case CustomSCP244 scp244Data:
                    LogManager.Debug($"SCPItem is SCP-244");
                    Scp244Pickup scp244Pickup = (Scp244Pickup)Scp244Pickup.Create(CustomItem.Item, Pickup.Position);
                    scp244Pickup.Base.MaxDiameter = scp244Data.MaxDiameter;
                    scp244Pickup.Base._activationDot = scp244Data.ActivationDot;
                    scp244Pickup.Base._health = scp244Data.Health;
                    scp244Pickup.Base.enabled = scp244Data.Primed;
                    ReplacePickup(scp244Pickup);
                    break;
            }
        }

        private void ApplyJailbirdStats(object jailbird, CustomJailbird data)
        {
            if (jailbird is LabApi.Features.Wrappers.JailbirdItem jailbirditem)
            {
                LogManager.Debug($"Jailbird - {jailbirditem.Serial} is a Item");
                jailbirditem.Base._hitreg._flashedDuration = data.FlashDuration;
                jailbirditem.Base._hitreg._hitregRadius = data.Radius;
                jailbirditem.Base._hitreg._damageCharge = data.ChargeDamage;
                jailbirditem.Base._hitreg._damageMelee = data.MeleeDamage;
                JailbirdDeteriorationTracker.ReceivedStates[jailbirditem.Serial] = data.WearState;
                JailbirdDeteriorationTracker._anyReceived = true;
                using (new AutosyncRpc(jailbirditem.Base.ItemId, out NetworkWriter writer))
                {
                    writer.WriteByte(0);
                    writer.WriteByte((byte)data.WearState);
                }
            }
            else if (jailbird is LabApi.Features.Wrappers.JailbirdPickup jailbirdpickup)
            {
                LogManager.Debug($"Jailbird - {jailbirdpickup.Serial} is a Pickup");
                if (jailbirdpickup.Base.TryGetTemplate<InventorySystem.Items.Jailbird.JailbirdItem>(out var jailbirdpickupitem))
                {
                    jailbirdpickupitem._hitreg._flashedDuration = data.FlashDuration;
                    jailbirdpickupitem._hitreg._hitregRadius = data.Radius;
                    jailbirdpickupitem._hitreg._damageCharge = data.ChargeDamage;
                    jailbirdpickupitem._hitreg._damageMelee = data.MeleeDamage;
                }

                jailbirdpickup.WearState = data.WearState;
            }
        }

        private void ApplyFirearmStats(HitscanHitregModuleBase hitscan, dynamic weaponData)
        {
            hitscan.BaseDamage = weaponData.Damage;
            hitscan.BasePenetration = weaponData.Penetration;
            hitscan.BaseBulletInaccuracy = weaponData.Inaccuracy;
            hitscan.DamageFalloffDistance = weaponData.DamageFalloffDistance;
        }

        private void ApplyAttachments(Firearm firearm, IEnumerable<AttachmentName> attachments)
        {
            foreach (AttachmentName attachment in attachments)
            {
                if (firearm.TryApplyAttachment(attachment))
                    LogManager.Debug($"Added {attachment} to {CustomItem.Name}");
                else
                    LogManager.Error($"Failed to add {attachment} to {CustomItem.Name}");
            }
        }

        private void ApplyKeycardData(Wrappers.CustomKeycard customKeycard, CustomKeycard keycardData)
        {
            ColorUtility.TryParseHtmlString(keycardData.PermissionsColor, out var permissionsColor);
            ColorUtility.TryParseHtmlString(keycardData.TintColor, out var tintColor);
            ColorUtility.TryParseHtmlString(keycardData.LabelColor, out var labelColor);

            KeycardLevels permissions = new(keycardData.Containment, keycardData.Armory, keycardData.Admin);

            customKeycard.SerialNumber = keycardData.SerialNumber;
            customKeycard.WearIndex = keycardData.WearDetail;
            customKeycard.RankIndex = keycardData.Rank;
            customKeycard.LabelColor = (Color32)labelColor;
            customKeycard.LabelText = keycardData.Label;
            customKeycard.ItemName = CustomItem.Name;
            customKeycard.CardColor = (Color32)tintColor;
            customKeycard.PermissionsColor = (Color32)permissionsColor;
            customKeycard.Permissions = permissions;
            
            LogManager.Debug($"{(Color32)labelColor} {labelColor} {keycardData.LabelColor}");
        }

        private void ReplacePickup(Pickup newPickup)
        {
            Pickup.Destroy();
            newPickup.Spawn();
            Pickup = newPickup;
            Serial = Pickup.Serial;
        }

        public void SaveProperties()
        {
            if (Item is null) return;

            switch (CustomItem)
            {
                case CustomSCP127 scp127Data:
                    var scpFirearm = Item as FirearmItem;
                    scpFirearm.Base.TryGetModule<MagazineModule>(out var scpmagazine);
                    scpFirearm.Base.TryGetModule<HitscanHitregModuleBase>(out var scphitscan);

                    SaveFirearmStats(scp127Data, scpmagazine, scphitscan);
                    break;

                case CustomWeapon weaponData:
                    var firearm = Item as FirearmItem;
                    firearm.Base.TryGetModule<MagazineModule>(out var magazine);
                    firearm.Base.TryGetModule<HitscanHitregModuleBase>(out var hitscan);

                    SaveFirearmStats(weaponData, magazine, hitscan);
                    break;
            }
        }

        private void SaveFirearmStats(dynamic weaponData, MagazineModule magazine, HitscanHitregModuleBase hitscan)
        {
            weaponData.MaxAmmo = magazine.AmmoStored;
            weaponData.Damage = hitscan.BaseDamage;
            weaponData.Penetration = hitscan.BasePenetration;
            weaponData.Inaccuracy = hitscan.BaseBulletInaccuracy;
            weaponData.DamageFalloffDistance = hitscan.DamageFalloffDistance;
            magazine.ServerResyncData();
        }

        internal void OnPickup(PlayerPickedUpItemEventArgs pickedUp)
        {
            Pickup = null;
            Item = pickedUp.Item;
            Owner = pickedUp.Player;
            Serial = Item.Serial;
            SetItemProperties();
        }

        public void OnDrop(PlayerDroppedItemEventArgs dropped)
        {
            Pickup = dropped.Pickup;
            Item = null;
            Owner = null;
            Serial = Pickup.Serial;
            SaveProperties();
            APICustomItem._processedThrowables.Remove(Serial);
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

            if (Plugin.Instance.Config.EnableCreditTags)
                Plugin.HttpManager.ApplyCreditTag(player);

            LogManager.Debug($"{player.Nickname} Badge successfully reset");
        }

        public void HandleSelectedDisplayHint()
        {
            if (!string.IsNullOrWhiteSpace(Plugin.Instance.Config.SelectedMessage))
                Owner.SendHint(Plugin.Instance.Config.SelectedMessage.Replace("%name%", CustomItem.Name).Replace("%desc%", CustomItem.Description).Replace("%description%", CustomItem.Description), Plugin.Instance.Config.SelectedMessageDuration);
        }

        public void HandlePickedUpDisplayHint()
        {
            if (!string.IsNullOrEmpty(Plugin.Instance.Config.PickedUpMessage))
                Owner.SendHint(Plugin.Instance.Config.PickedUpMessage.Replace("%name%", CustomItem.Name).Replace("%desc%", CustomItem.Description).Replace("%description%", CustomItem.Description), Plugin.Instance.Config.PickedUpMessageDuration);
        }

        /// <summary>
        /// Tries to get the <see cref="SummonedAPICustomItem"/> by its <see cref="Serial"/>
        /// </summary>
        /// <param name="serial"></param>
        /// <param name="item"></param>
        /// <returns><see langword="false"/> if not found : <see langword="true"/> if found</returns>
        public static bool TryGet(ushort serial, out SummonedAPICustomItem item) => SummonedCustomItems.TryGetValue(serial, out item);

        /// <summary>
        /// Tries to get the <see cref="SummonedAPICustomItem"/> by its <see cref="APICustomItem"/>
        /// </summary>
        /// <param name="serial"></param>
        /// <param name="item"></param>
        /// <returns><see langword="false"/> if not found : <see langword="true"/> if found</returns>
        public static bool TryGet(APICustomItem baseitem, out SummonedAPICustomItem item)
        {
            foreach (SummonedAPICustomItem summoneditem in List)
            {
                if (summoneditem.CustomItem == baseitem)
                {
                    item = summoneditem;
                    return true;
                }
            }

            item = null;
            return false;
        }

        /// <summary>
        /// Gets a <see cref="SummonedAPICustomItem"/> by its <see cref="Serial"/>
        /// </summary>
        /// <param name="serial"></param>
        /// <returns><see cref="SummonedAPICustomItem"/></returns>
        public static SummonedAPICustomItem Get(ushort serial) => SummonedCustomItems[serial];

        /// <summary>
        /// Gets a list of every summoned <see cref="APICustomItem"/> where the <see cref="Owner"/> equals the parameter
        /// </summary>
        /// <param name="owner"></param>
        /// <returns>SummonedCustomItem List</returns>
        public static List<SummonedAPICustomItem> Get(Player owner) => List.Where(i => i.Owner == owner).ToList();

        /// <summary>
        /// Gets a list of every summoned <see cref="APICustomItem"/> where the <see cref="CustomItem.Item"/> equals the parameter
        /// </summary>
        /// <param name="item"></param>
        /// <returns>SummonedCustomItem List</returns>
        public static List<SummonedAPICustomItem> Get(ItemType item) => List.Where(i => i.CustomItem.Item == item).ToList();
    }
}