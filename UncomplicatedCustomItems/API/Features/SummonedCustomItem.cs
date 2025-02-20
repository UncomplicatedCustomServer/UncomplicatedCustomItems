using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Items.FirearmModules.Primary;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;
using MEC;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomItems.Interfaces;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using UnityEngine;
using Exiled.API.Enums;
using UncomplicatedCustomItems.API.Struct;
using UncomplicatedCustomItems.Events;
using Exiled.API.Features.Roles;
using UncomplicatedCustomItems.API.Features.Helper;
using System;
using UncomplicatedCustomItems.API.Features.CustomModules;
using UncomplicatedCustomItems.Enums;
using InventorySystem.Items.Firearms.Attachments;
using System.Net.Mail;
using UncomplicatedCustomItems.API.Features.SpecificData;
using HarmonyLib;

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
        private static readonly List<CustomItemType> _managedItems = [CustomItemType.Painkillers, CustomItemType.Medikit];

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

        internal static List<Tuple<string, string, string, string>> NotLoadedItems { get; } = new();

        /// <summary>
        /// Gets the badge of the player if it has one
        /// </summary>
        public Triplet<string, string, bool>? Badge { get; private set; }

        public IReadOnlyCollection<ICustomModule> CustomModules => _customModules;

        private List<ICustomModule> _customModules { get; set; }

        public static readonly List<IFlagSettings> _flagSettings = new();

        public static void Register(IFlagSettings flagSettings)
        {
            if (flagSettings == null)
                throw new ArgumentNullException(nameof(flagSettings));

            if (!_flagSettings.Contains(flagSettings))
            {
                _flagSettings.Add(flagSettings);
            }
            LogManager.Debug($"added {_flagSettings}");
        }

        public static bool Unregister(IFlagSettings flagSettings)
        {
            return _flagSettings.Remove(flagSettings);
        }

        public static IReadOnlyList<IFlagSettings> GetAllFlagSettings()
        {
            LogManager.Debug("Retrieving all loaded Flag Settings");
            
            return _flagSettings.AsReadOnly();
        }

        public static void ClearAllFlagSettings()
        {
            _flagSettings.Clear();
        }
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

        public long LastDamageTime { get; internal set; }


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
            GetAllFlagSettings();
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
                        Armor.VestEfficacy = ArmorData.BodyProtection;
                        Armor.RemoveExcessOnDrop = ArmorData.RemoveExcessOnDrop;
                        Armor.StaminaUseMultiplier = ArmorData.StaminaUseMultiplier;
                        Armor.StaminaRegenMultiplier = ArmorData.StaminaRegenMultiplier;
                        break;

                    case CustomItemType.Weapon:
                        Firearm Firearm = Item as Firearm;
                        IWeaponData WeaponData = CustomItem.CustomData as IWeaponData;

                        Firearm.MagazineAmmo = WeaponData.MaxAmmo;
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
        private void SaveProperties()
        {
            if (Item is not null)
            {
                switch (CustomItem.CustomItemType)
                {
                    case CustomItemType.Keycard:
                        {
                            var keycard = Item as Keycard;
                            var keycardData = CustomItem.CustomData as IKeycardData;
                            if (keycard != null && keycardData != null)
                            {
                                keycardData.Permissions = keycard.Permissions;
                            }
                            break;
                        }
                    case CustomItemType.Armor:
                        {
                            var armor = Item as Armor;
                            var armorData = CustomItem.CustomData as IArmorData;
                            if (armor != null && armorData != null)
                            {
                                armorData.HeadProtection = armor.HelmetEfficacy;
                                armorData.BodyProtection = armor.VestEfficacy;
                                armorData.RemoveExcessOnDrop = armor.RemoveExcessOnDrop;
                                armorData.StaminaUseMultiplier = armor.StaminaUseMultiplier;
                                armorData.StaminaRegenMultiplier = armor.StaminaRegenMultiplier;
                            }
                            break;
                        }
                    case CustomItemType.Weapon:
                        {
                            var firearm = Item as Firearm;
                            var weaponData = CustomItem.CustomData as IWeaponData;
                            if (firearm != null && weaponData != null)
                            {
                                weaponData.MaxMagazineAmmo = (byte)firearm.MaxMagazineAmmo;
                                weaponData.MaxBarrelAmmo = (byte)firearm.MaxBarrelAmmo;
                                weaponData.AmmoDrain = firearm.AmmoDrain;
                                weaponData.Penetration = firearm.Penetration;
                                weaponData.Inaccuracy = firearm.Inaccuracy;
                                weaponData.DamageFalloffDistance = firearm.DamageFalloffDistance;
                            }
                            break;
                        }
                    case CustomItemType.Jailbird:
                        {
                            var jailbird = Item as Jailbird;
                            var jailbirdData = CustomItem.CustomData as IJailbirdData;
                            if (jailbird != null && jailbirdData != null)
                            {
                                jailbirdData.TotalDamageDealt = jailbird.TotalDamageDealt;
                                jailbirdData.TotalCharges = jailbird.TotalCharges;
                                jailbirdData.Radius = jailbird.Radius;
                                jailbirdData.ChargeDamage = jailbird.ChargeDamage;
                                jailbirdData.MeleeDamage = jailbird.MeleeDamage;
                                jailbirdData.FlashDuration = jailbird.FlashDuration;
                            }
                            break;
                        }
                    case CustomItemType.ExplosiveGrenade:
                        {
                            var explosiveGrenade = Item as ExplosiveGrenade;
                            var explosiveData = CustomItem.CustomData as IExplosiveGrenadeData;
                            if (explosiveGrenade != null && explosiveData != null)
                            {
                                explosiveData.MaxRadius = explosiveGrenade.MaxRadius;
                                explosiveData.PinPullTime = explosiveGrenade.PinPullTime;
                                explosiveData.ScpDamageMultiplier = explosiveGrenade.ScpDamageMultiplier;
                                explosiveData.ConcussDuration = explosiveGrenade.ConcussDuration;
                                explosiveData.BurnDuration = explosiveGrenade.BurnDuration;
                                explosiveData.DeafenDuration = explosiveGrenade.DeafenDuration;
                                explosiveData.FuseTime = explosiveGrenade.FuseTime;
                                explosiveData.Repickable = explosiveGrenade.Repickable;
                            }
                            break;
                        }
                    case CustomItemType.FlashGrenade:
                        {
                            var flashGrenade = Item as FlashGrenade;
                            var flashData = CustomItem.CustomData as IFlashGrenadeData;
                            if (flashGrenade != null && flashData != null)
                            {
                                flashData.PinPullTime = flashGrenade.PinPullTime;
                                flashData.Repickable = flashGrenade.Repickable;
                                flashData.MinimalDurationEffect = flashGrenade.MinimalDurationEffect;
                                flashData.AdditionalBlindedEffect = flashGrenade.AdditionalBlindedEffect;
                                flashData.SurfaceDistanceIntensifier = flashGrenade.SurfaceDistanceIntensifier;
                                flashData.FuseTime = flashGrenade.FuseTime;
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
        private void LoadProperties()
        {
            if (Item is not null)
            {
                switch (CustomItem.CustomItemType)
                {
                    case CustomItemType.Keycard:
                        {
                            var keycard = Item as Keycard;
                            var keycardData = CustomItem.CustomData as IKeycardData;
                            if (keycard != null && keycardData != null)
                            {
                                keycard.Permissions = keycardData.Permissions;
                            }
                            break;
                        }

                    case CustomItemType.Armor:
                        {
                            var armor = Item as Armor;
                            var armorData = CustomItem.CustomData as IArmorData;
                            if (armor != null && armorData != null)
                            {
                                armor.HelmetEfficacy = armorData.HeadProtection;
                                armor.VestEfficacy = armorData.BodyProtection;
                                armor.RemoveExcessOnDrop = armorData.RemoveExcessOnDrop;
                                armor.StaminaUseMultiplier = armorData.StaminaUseMultiplier;
                                armor.StaminaRegenMultiplier = armorData.StaminaRegenMultiplier;
                            }
                            break;
                        }

                    case CustomItemType.Weapon:
                        {
                            var firearm = Item as Firearm;
                            var weaponData = CustomItem.CustomData as IWeaponData;
                            if (firearm != null && weaponData != null)
                            {
                                firearm.MaxMagazineAmmo = weaponData.MaxMagazineAmmo;
                                firearm.MaxBarrelAmmo = weaponData.MaxBarrelAmmo;
                                firearm.AmmoDrain = weaponData.AmmoDrain;
                                firearm.Penetration = weaponData.Penetration;
                                firearm.Inaccuracy = weaponData.Inaccuracy;
                                firearm.DamageFalloffDistance = weaponData.DamageFalloffDistance;
                            }
                            break;
                        }

                    case CustomItemType.Jailbird:
                        {
                            var jailbird = Item as Jailbird;
                            var jailbirdData = CustomItem.CustomData as IJailbirdData;
                            if (jailbird != null && jailbirdData != null)
                            {
                                jailbird.TotalDamageDealt = jailbirdData.TotalDamageDealt;
                                jailbird.TotalCharges = jailbirdData.TotalCharges;
                                jailbird.Radius = jailbirdData.Radius;
                                jailbird.ChargeDamage = jailbirdData.ChargeDamage;
                                jailbird.MeleeDamage = jailbirdData.MeleeDamage;
                                jailbird.FlashDuration = jailbirdData.FlashDuration;
                            }
                            break;
                        }

                    case CustomItemType.ExplosiveGrenade:
                        {
                            var explosiveGrenade = Item as ExplosiveGrenade;
                            var explosiveData = CustomItem.CustomData as IExplosiveGrenadeData;
                            if (explosiveGrenade != null && explosiveData != null)
                            {
                                explosiveGrenade.MaxRadius = explosiveData.MaxRadius;
                                explosiveGrenade.PinPullTime = explosiveData.PinPullTime;
                                explosiveGrenade.ScpDamageMultiplier = explosiveData.ScpDamageMultiplier;
                                explosiveGrenade.ConcussDuration = explosiveData.ConcussDuration;
                                explosiveGrenade.BurnDuration = explosiveData.BurnDuration;
                                explosiveGrenade.DeafenDuration = explosiveData.DeafenDuration;
                                explosiveGrenade.FuseTime = explosiveData.FuseTime;
                                explosiveGrenade.Repickable = explosiveData.Repickable;
                            }
                            break;
                        }

                    case CustomItemType.FlashGrenade:
                        {
                            var flashGrenade = Item as FlashGrenade;
                            var flashData = CustomItem.CustomData as IFlashGrenadeData;
                            if (flashGrenade != null && flashData != null)
                            {
                                flashGrenade.PinPullTime = flashData.PinPullTime;
                                flashGrenade.Repickable = flashData.Repickable;
                                flashGrenade.MinimalDurationEffect = flashData.MinimalDurationEffect;
                                flashGrenade.AdditionalBlindedEffect = flashData.AdditionalBlindedEffect;
                                flashGrenade.SurfaceDistanceIntensifier = flashData.SurfaceDistanceIntensifier;
                                flashGrenade.FuseTime = flashData.FuseTime;
                            }
                            break;
                        }

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



        public string LoadBadge(Player player)
        {
            LogManager.Debug("LoadBadge() Triggered");
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

            CustomItemBadgeApplier(player, CustomItem);

            return output;
        }

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

        public void ResetBadge(Player Player)
        {
            Player.ReferenceHub.serverRoles.RefreshLocalTag();
            LogManager.Debug("Badge successfully reset");
        }
        
        internal void OnPickup(ItemAddedEventArgs pickedUp)
        {
            Pickup = null;
            Item = pickedUp.Item;
            Owner = pickedUp.Player;
            GetAllFlagSettings();
            LoadProperties();
            Serial = Item.Serial;
            HandleEvent(pickedUp.Player, ItemEvents.Pickup);
        }

        public void OnDrop(DroppedItemEventArgs dropped)
        {
            Pickup = dropped.Pickup;
            Item = null;
            Owner = null;
            ClearAllFlagSettings();
            SaveProperties();
            Serial = Pickup.Serial;
            HandleEvent(dropped.Player, ItemEvents.Drop);

        }
        
        public string LoadItemFlags()
        {
            List<string> output = new();

            if (_customModules.Count > 0)
                output.Add("<color=#a343f7>[CUSTOM MODULES]</color>");

            if (output.Count > 0)
            {
                output.Insert(0, "                                ");
            }

            return string.Join(" ", output);
        }
        public void ReloadItemFlags()
        {
            LogManager.Debug("Reload Item Flags Function Triggered");
            _customModules = CustomModule.Load(CustomItem.CustomFlags ?? CustomFlags.None, this);
            List.Add(this);

            LogManager.Debug("Item Flag(s) Reloaded");
            LogManager.Debug($"Loaded Flag(s): {CustomItem.CustomFlags}");
        }

        public void UnloadItemFlags()
        {
            LogManager.Debug("Unload Item Flags Triggered");
            _customModules.Clear(); 
            LogManager.Debug("Item Flags Cleared");
        }

        /// <summary>
        /// Gets a <see cref="CustomModule"/> that this custom item implements
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetModule<T>() where T : CustomModule => _customModules.Where(cm => cm.GetType() == typeof(T)).FirstOrDefault() as T;

        /// <summary>
        /// Gets a <see cref="CustomModule"/> array that contains every custom module with the same type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] GetModules<T>() where T : CustomModule
        {
            T[] result = new T[] { };
            foreach (ICustomModule module in _customModules.Where(cm => cm.GetType() == typeof(T)))
                result.AddItem(module);
            return result;
        }

        /// <summary>
        /// Try to get a <see cref="CustomModule"/> if its implemented
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="module"></param>
        /// <returns></returns>
        public bool GetModule<T>(out T module) where T : CustomModule
        {
            module = GetModule<T>();
            return module != null;
        }

        /// <summary>
        /// Gets if the current <see cref="SummonedCustomItem"/> implements the given <see cref="CustomModule"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool HasModule<T>() where T : CustomModule => _customModules.Any(cm => cm.GetType() == typeof(T));

        /// <summary>
        /// Add a new <see cref="CustomModule"/> to the current <see cref="SummonedCustomItem"/> instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddModule<T>() where T : CustomModule => _customModules.Add(CustomModule.Load(typeof(T), this));

        /// <summary>
        /// Try to remove the first <see cref="CustomModule"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RemoveModule<T>() where T : CustomModule
        {
            if (GetModule(out T module))
            {
                if (module is CoroutineModule coroutineModule && coroutineModule.CoroutineHandler.IsRunning)
                    Timing.KillCoroutines(coroutineModule.CoroutineHandler);
                _customModules.Remove(module);
            }
        }

        /// <summary>
        /// Remove every <see cref="CustomModule"/> with the same given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RemoveModules<T>() where T : CustomModule
        {
            foreach (ICustomModule _ in GetModules<T>())
                RemoveModule<T>();
        }
        internal void HandleEvent(Player player, ItemEvents itemEvent)
        {
            if (CustomItem.CustomItemType == CustomItemType.Item && ((IItemData)CustomItem.CustomData).Event == itemEvent)
            {
                IItemData Data = CustomItem.CustomData as IItemData;
                Log.Debug($"Firing events for item {CustomItem.Name}");
                if (Data.Command is not null && Data.Command.Length > 2)
                    if (!Data.Command.Contains("{p_id}"))
                        Server.ExecuteCommand(Data.Command.Replace("%id%", player.Id.ToString()));
                    else
                        Server.ExecuteCommand(Data.Command.Replace("%id%", player.Id.ToString()).Replace("{p_id}", ""), player.Sender);
                    if (!Data.Command.Contains("{p_pos}"))
                        Server.ExecuteCommand(Data.Command.Replace("%pos%", player.Position.ToString()));
                    else
                        Server.ExecuteCommand(Data.Command.Replace("%pos%", player.Position.ToString()).Replace("{p_pos}", ""), player.Sender);
                    if (!Data.Command.Contains("{p_role}"))
                        Server.ExecuteCommand(Data.Command.Replace("%role%", player.Role.ToString()));
                    else
                        Server.ExecuteCommand(Data.Command.Replace("%role%", player.Role.ToString()).Replace("{p_role}", ""), player.Sender);
                    if (!Data.Command.Contains("{p_health}"))
                        Server.ExecuteCommand(Data.Command.Replace("%health%", player.Health.ToString()));
                    else
                        Server.ExecuteCommand(Data.Command.Replace("%health%", player.Health.ToString()).Replace("{p_health}", ""), player.Sender);
                    if (!Data.Command.Contains("{p_zone}"))
                        Server.ExecuteCommand(Data.Command.Replace("%zone%", player.Zone.ToString()));
                    else
                        Server.ExecuteCommand(Data.Command.Replace("%zone%", player.Zone.ToString()).Replace("{p_zone}", ""), player.Sender);
                    if (!Data.Command.Contains("{p_room}"))
                        Server.ExecuteCommand(Data.Command.Replace("%room%", player.CurrentRoom.ToString()));
                    else
                        Server.ExecuteCommand(Data.Command.Replace("%room%", player.CurrentRoom.ToString()).Replace("{p_room}", ""), player.Sender);

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

        internal bool HandleCustomAction(InventorySystem.Items.Usables.Consumable item)
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
                    default:
                        return false;
                }

                // Runs also the event as it gets suppressed
                HandleEvent(Owner, ItemEvents.Use);
                if (!CustomItem.Reusable)
                    Item.Get(item).Destroy();

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
