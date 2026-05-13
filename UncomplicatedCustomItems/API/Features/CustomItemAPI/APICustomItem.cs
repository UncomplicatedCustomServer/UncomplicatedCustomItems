using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp914Events;
using LabApi.Features.Wrappers;
using MapGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features.Helper;
using UnityEngine;
using UncomplicatedCustomItems.Events.Handlers;
using MEC;

namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    /// <summary>
    /// The API version of a <see cref="CustomItem"/>
    /// Aims to simplify the process of making CustomItems in C#
    /// </summary>
    public abstract class APICustomItem
    {
        /// <summary>
        /// Gets a list of every registered <see cref="APICustomItem"/>
        /// </summary>
        public static IReadOnlyCollection<APICustomItem> List => CustomItems.Values.ToList();

        internal static Dictionary<uint, APICustomItem> CustomItems { get; set; } = [];

        public static void Register(APICustomItem item)
        {
            if (CustomItems.ContainsKey(item.Id) || CustomItem.CustomItems.ContainsKey(item.Id))
            {
                uint id = GetFirstFreeId();
                item.Id = id;
            }

            CustomItems.TryAdd(item.Id, item);
            LogManager.Info($"{nameof(APICustomItem)}: Successfully registered APICustomItem '{item.Name}' (Id: {item.Id}) into the plugin!");
        }

        /// <summary>
        /// Unregister a <see cref="APICustomItem"/> from the plugin by its class
        /// </summary>
        /// <param name="item"></param>
        public static void Unregister(APICustomItem item) => Unregister(item.Id);

        /// <summary>
        /// Unregister a <see cref="APICustomItem"/> from the plugin by its Id
        /// </summary>
        /// <param name="item"></param>
        public static void Unregister(uint item)
        {
            if (CustomItems.ContainsKey(item))
                CustomItems.Remove(item);
        }

        /// <summary>
        /// Gets the first free Id for a custom item
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public static uint GetFirstFreeId(uint from = 1)
        {
            for (uint i = from; i < uint.MaxValue; i++)
            {
                if (!CustomItems.ContainsKey(i) && !CustomItem.CustomItems.ContainsKey(i))
                    return i;
            }

            return 0;
        }

        public static void SummonItem(APICustomItem item, bool ignoreChance = false)
        {
            if (!ignoreChance)
            {
                float roll = UnityEngine.Random.Range(0f, 101f);
                if (roll >= item.ChanceToSpawn)
                    return;
            }

            if (item.SpawnLocations.Count() >= 1)
            {
                foreach (var kvp in item.SpawnLocations)
                {
                    Vector3 pos;
                    Room room = Utilities.GetRoomFromName(kvp.Key);

                    if (kvp.Value != Vector3.zero)
                        pos = room.WorldPosition(kvp.Value);

                    if (item.ReplaceExistingPickup)
                    {
                        Pickup? targetPickup = CustomItemUtils.FindTargetPickupInRoom(room, item);
                        if (targetPickup != null)
                            new SummonedAPICustomItem(item, targetPickup);
                    }

                }
            }
            if (item.Zones.Count() >= 1)
            {
                foreach (FacilityZone zone in item.Zones)
                {
                    List<Room> rooms = Room.List.Where(r => r != null && r.Zone == zone).ToList();
                    if (rooms.Count() > 1)
                    {
                        Room room = rooms.RandomItem();
                        if (item.ReplaceExistingPickup && !item.ForceSameItemType)
                        {
                            Pickup? targetPickup = CustomItemUtils.FindTargetPickupInRoom(room, item);
                            if (targetPickup != null)
                                new SummonedAPICustomItem(item, targetPickup);
                        }
                    }
                    else
                    {
                        Room room = rooms.FirstOrDefault();
                    }
                }
            }
            if (item.Coordinates.Count() >= 1)
            {
                foreach (Vector3 coords in item.Coordinates.Where(c => c != Vector3.zero))
                {
                    if (item.Rotation != Vector3.zero)
                    {
                        item.Rotation.Normalize();
                        new SummonedAPICustomItem(item, coords, Quaternion.Euler(item.Rotation));
                    }
                    else
                        new SummonedAPICustomItem(item, coords);
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="APICustomItem"/> by its unique Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An <see cref="APICustomItem"/></returns>
        public static APICustomItem? Get(uint id)
        {
            if (!CustomItems.ContainsKey(id))
                return null;

            return CustomItems[id];
        }

        /// <summary>
        /// Gets the <see cref="APICustomItem"/> by its class type
        /// </summary>
        /// <param name="t"></param>
        /// <returns><see cref="IEnumerable{BaseCustomItem}"/></returns>
        public static IEnumerable<APICustomItem> Get(Type t) => List.Where(i => i.GetType() == t);

        /// <summary>
        /// Tries to get a list of <see cref="APICustomItem"/> by its class type
        /// </summary>
        /// <param name="t"></param>
        /// <param name="items"></param>
        /// <returns><see langword="true"/> if found otherwise <see langword="false"/> if not found</returns>
        public static bool TryGet(Type t, out IEnumerable<APICustomItem> items)
        {
            items = Get(t);

            return items.Any();
        }

        /// <summary>
        /// Tries to get a <see cref="APICustomItem"/> by its unique Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <returns><see cref="APICustomItem"/> instance</returns>
        public static bool TryGet(uint id, out APICustomItem? item)
        {
            item = null;
            if (CustomItems.ContainsKey(id))
            {
                item = CustomItems[id];
                return true;
            }

            return false;
        }


        public virtual bool Check(Pickup? pickup)
        {
            if (pickup != null)
                return SummonedAPICustomItem.SerialList.Contains(pickup.Serial);

            return false;
        }

        public virtual bool Check(Item? item)
        {
            if (item != null)
                return SummonedAPICustomItem.SerialList.Contains(item.Serial);

            return false;
        }

        public virtual bool Check(Player? player) => Check(player?.CurrentItem);

        /// <summary>
        /// The unique Id of the Custom Item. Can't be less than 1
        /// </summary>
        public abstract uint Id { get; set; }

        /// <summary>
        /// The Name of the object. Can appears when for example you pick it up
        /// </summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// The description.
        /// </summary>
        public abstract string Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string ExtendedDescription { get; set; }

        /// <summary>
        /// Gets or sets the badge name
        /// </summary>
        public virtual string BadgeName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the badge color
        /// </summary>
        public virtual string BadgeColor { get; set; } = string.Empty;

        /// <summary>
        /// The weight of the item
        /// </summary>
        public abstract float Weight { get; set; }

        /// <summary>
        /// Gets or sets the scale of the <see cref="APICustomItem"/> as a <see cref="Pickup"/>
        /// </summary>
        public virtual Vector3 Scale { get; set; }

        /// <summary>
        /// Gets or sets wether the <see cref="APICustomItem"/> will naturally spawn
        /// </summary>
        public virtual bool Spawn { get; set; }

        /// <summary>
        /// Gets or sets the amount of <see cref="APICustomItem"/>s to spawn
        /// </summary>
        public virtual int AmountToSpawn { get; set; }

        /// <summary>
        /// Gets or sets the chance for the <see cref="APICustomItem"/> to spawn.
        /// </summary>
        public virtual float ChanceToSpawn { get; set; }

        /// <summary>
        /// Gets or sets the rotation of the <see cref="APICustomItem"/> when first spawned as a <see cref="Pickup"/>
        /// </summary>
        public virtual Vector3 Rotation { get; set; }

        /// <summary>
        /// Gets or sets the locations that the <see cref="APICustomItem"/> can spawn.
        /// The dictionary maps a <see cref="RoomName"/> or GameObject name (key) to a local <see cref="Vector3"/> position
        /// </summary>
        /// <remarks>
        /// Has the highest priority
        /// </remarks>
        public virtual Dictionary<string, Vector3> SpawnLocations { get; set; } = [];

        /// <summary>
        /// Gets or sets the zones the <see cref="APICustomItem"/> can spawn.
        /// Selects a random room in the <see cref="FacilityZone"/> to spawn the <see cref="APICustomItem"/> in.
        /// </summary>
        /// <remarks>
        /// Has the second highest priority
        /// </remarks>
        public virtual List<FacilityZone> Zones { get; set; } = [];

        /// <summary>
        /// Gets or sets the coordinates that the <see cref="APICustomItem"/> can spawn.
        /// </summary>
        /// <remarks>
        /// The only static coordinates are in the suface zone
        /// Has the least priority
        /// </remarks>
        public virtual List<Vector3> Coordinates { get; set; } = [];

        /// <summary>
        /// Gets or sets wether the <see cref="APICustomItem"/> will replace a <see cref="Pickup"/> when it spawns.
        /// </summary>
        public virtual bool ReplaceExistingPickup { get; set; }

        /// <summary>
        /// Gets or sets wether the <see cref="APICustomItem"/> can only replace a <see cref="Pickup"/> of the same <see cref="ItemType"/>
        /// </summary>
        public virtual bool ForceSameItemType { get; set; }

        /// <summary>
        /// Whether if the item won't be removed from the player's inventory
        /// </summary>
        public virtual bool Reusable { get; set; } = false;

        /// <summary>
        /// The <see cref="ItemType"/> (Base) of the Custom Item
        /// </summary>
        public abstract ItemType Item { get; set; }

        public virtual void RegisterEvents()
        {
            LabApi.Events.Handlers.PlayerEvents.InspectingItem += new LabApi.Events.LabEventHandler<PlayerInspectingItemEventArgs>(InternalOnInspecting);
            LabApi.Events.Handlers.PlayerEvents.Dying += new LabApi.Events.LabEventHandler<PlayerDyingEventArgs>(InternalOnDying);
            LabApi.Events.Handlers.PlayerEvents.DroppingItem += new LabApi.Events.LabEventHandler<PlayerDroppingItemEventArgs>(InternalOnDropping);
            LabApi.Events.Handlers.PlayerEvents.ChangingItem += new LabApi.Events.LabEventHandler<PlayerChangingItemEventArgs>(InternalOnChangingItem);
            LabApi.Events.Handlers.PlayerEvents.Escaping += new LabApi.Events.LabEventHandler<PlayerEscapingEventArgs>(InternalOnOwnerEscaping);
            LabApi.Events.Handlers.PlayerEvents.PickingUpItem += new LabApi.Events.LabEventHandler<PlayerPickingUpItemEventArgs>(InternalOnPickingUp);
            LabApi.Events.Handlers.PlayerEvents.PickedUpItem += new LabApi.Events.LabEventHandler<PlayerPickedUpItemEventArgs>(InternalOnPickup);
            LabApi.Events.Handlers.Scp914Events.ProcessingPickup += new LabApi.Events.LabEventHandler<Scp914ProcessingPickupEventArgs>(InternalOnUpgradingPickup);
            LabApi.Events.Handlers.PlayerEvents.Cuffed -= new LabApi.Events.LabEventHandler<PlayerCuffedEventArgs>(InternalOnOwnerHandCuffed);
            LabApi.Events.Handlers.PlayerEvents.Cuffing += new LabApi.Events.LabEventHandler<PlayerCuffingEventArgs>(InternalOnOwnerHandcuffing);
            LabApi.Events.Handlers.PlayerEvents.ChangingRole += new LabApi.Events.LabEventHandler<PlayerChangingRoleEventArgs>(InternalOnOwnerChangingRole);
            LabApi.Events.Handlers.Scp914Events.ProcessingInventoryItem += new LabApi.Events.LabEventHandler<Scp914ProcessingInventoryItemEventArgs>(InternalOnUpgradingInventoryItem);
            LabApi.Events.Handlers.PlayerEvents.InspectedItem += new LabApi.Events.LabEventHandler<PlayerInspectedItemEventArgs>(InternalOnInspected);
            LabApi.Events.Handlers.PlayerEvents.ThrowingItem += new LabApi.Events.LabEventHandler<PlayerThrowingItemEventArgs>(InternalOnThrowingItem);
            LabApi.Events.Handlers.PlayerEvents.ThrewItem += new LabApi.Events.LabEventHandler<PlayerThrewItemEventArgs>(InternalOnThrownItem);
            LabApi.Events.Handlers.PlayerEvents.DroppedItem += new LabApi.Events.LabEventHandler<PlayerDroppedItemEventArgs>(InternalOnDropped);
            LabApi.Events.Handlers.PlayerEvents.ChangedItem += new LabApi.Events.LabEventHandler<PlayerChangedItemEventArgs>(InternalOnChangedItem);
            LabApi.Events.Handlers.PlayerEvents.Death += new LabApi.Events.LabEventHandler<PlayerDeathEventArgs>(InternalOnDied);
            LabApi.Events.Handlers.PlayerEvents.Hurt += new LabApi.Events.LabEventHandler<PlayerHurtEventArgs>(InternalOnHurt);
            LabApi.Events.Handlers.PlayerEvents.Hurting += new LabApi.Events.LabEventHandler<PlayerHurtingEventArgs>(InternalOnHurting);
        }

        public virtual void UnregisterEvents()
        {
            LabApi.Events.Handlers.PlayerEvents.InspectingItem -= new LabApi.Events.LabEventHandler<PlayerInspectingItemEventArgs>(InternalOnInspecting);
            LabApi.Events.Handlers.PlayerEvents.Dying -= new LabApi.Events.LabEventHandler<PlayerDyingEventArgs>(InternalOnDying);
            LabApi.Events.Handlers.PlayerEvents.DroppingItem -= new LabApi.Events.LabEventHandler<PlayerDroppingItemEventArgs>(InternalOnDropping);
            LabApi.Events.Handlers.PlayerEvents.ChangingItem -= new LabApi.Events.LabEventHandler<PlayerChangingItemEventArgs>(InternalOnChangingItem);
            LabApi.Events.Handlers.PlayerEvents.Escaping -= new LabApi.Events.LabEventHandler<PlayerEscapingEventArgs>(InternalOnOwnerEscaping);
            LabApi.Events.Handlers.PlayerEvents.PickingUpItem -= new LabApi.Events.LabEventHandler<PlayerPickingUpItemEventArgs>(InternalOnPickingUp);
            LabApi.Events.Handlers.PlayerEvents.PickedUpItem -= new LabApi.Events.LabEventHandler<PlayerPickedUpItemEventArgs>(InternalOnPickup);
            LabApi.Events.Handlers.Scp914Events.ProcessingPickup -= new LabApi.Events.LabEventHandler<Scp914ProcessingPickupEventArgs>(InternalOnUpgradingPickup);
            LabApi.Events.Handlers.PlayerEvents.Cuffed -= new LabApi.Events.LabEventHandler<PlayerCuffedEventArgs>(InternalOnOwnerHandCuffed);
            LabApi.Events.Handlers.PlayerEvents.Cuffing -= new LabApi.Events.LabEventHandler<PlayerCuffingEventArgs>(InternalOnOwnerHandcuffing);
            LabApi.Events.Handlers.PlayerEvents.ChangingRole -= new LabApi.Events.LabEventHandler<PlayerChangingRoleEventArgs>(InternalOnOwnerChangingRole);
            LabApi.Events.Handlers.Scp914Events.ProcessingInventoryItem -= new LabApi.Events.LabEventHandler<Scp914ProcessingInventoryItemEventArgs>(InternalOnUpgradingInventoryItem);
            LabApi.Events.Handlers.PlayerEvents.InspectedItem -= new LabApi.Events.LabEventHandler<PlayerInspectedItemEventArgs>(InternalOnInspected);
            LabApi.Events.Handlers.PlayerEvents.ThrowingItem -= new LabApi.Events.LabEventHandler<PlayerThrowingItemEventArgs>(InternalOnThrowingItem);
            LabApi.Events.Handlers.PlayerEvents.ThrewItem -= new LabApi.Events.LabEventHandler<PlayerThrewItemEventArgs>(InternalOnThrownItem);
            LabApi.Events.Handlers.PlayerEvents.DroppedItem -= new LabApi.Events.LabEventHandler<PlayerDroppedItemEventArgs>(InternalOnDropped);
            LabApi.Events.Handlers.PlayerEvents.ChangedItem -= new LabApi.Events.LabEventHandler<PlayerChangedItemEventArgs>(InternalOnChangedItem);
            LabApi.Events.Handlers.PlayerEvents.Death -= new LabApi.Events.LabEventHandler<PlayerDeathEventArgs>(InternalOnDied);
            LabApi.Events.Handlers.PlayerEvents.Hurt -= new LabApi.Events.LabEventHandler<PlayerHurtEventArgs>(InternalOnHurt);
            LabApi.Events.Handlers.PlayerEvents.Hurting -= new LabApi.Events.LabEventHandler<PlayerHurtingEventArgs>(InternalOnHurting);
        }

        private void InternalOnOwnerHandCuffed(PlayerCuffedEventArgs ev)
        {
            if (Check(ev.Player))
                OnCuffed(ev);
        }
        private void InternalOnOwnerHandcuffing(PlayerCuffingEventArgs ev)
        {
            if (Check(ev.Player))
                OnCuffing(ev);
        }

        private void InternalOnUpgradingInventoryItem(Scp914ProcessingInventoryItemEventArgs ev)
        {
            if (Check(ev.Item))
            {
                ev.IsAllowed = false;
                OnUpgradingItem(ev);
            }
        }

        private void InternalOnUpgradingPickup(Scp914ProcessingPickupEventArgs ev) 
        {
            if (Check(ev.Pickup))
            {
                ev.IsAllowed = false;
                Timing.CallDelayed(3.5f, delegate
                {
                    ev.Pickup.Position = ev.NewPosition;
                    OnUpgradingPickup(ev);
                });
            }
        }

        private void InternalOnOwnerChangingRole(PlayerChangingRoleEventArgs ev)
        {
            foreach (Item item in ev.Player.Items.ToList())
            {
                if (Check(item))
                    OnChangingRole(ev);
            }
        }

        private void InternalOnOwnerEscaping(PlayerEscapingEventArgs ev)
        {
            foreach (Item item in ev.Player.Items.ToList())
            {
                if (Check(item))
                    OnEscaping(ev);
            }
            
        }
        private void InternalOnInspecting(PlayerInspectingItemEventArgs ev)
        {
            if (Check(ev.Item))
                OnInspecting(ev);

        }
        private void InternalOnInspected(PlayerInspectedItemEventArgs ev)
        {
            if (Check(ev.Item))
                OnInspected(ev);

        }
        private void InternalOnThrowingItem(PlayerThrowingItemEventArgs ev)
        {
            if (Check(ev.Pickup))
                OnThrowingItem(ev);

        }
        private void InternalOnThrownItem(PlayerThrewItemEventArgs ev)
        {
            if (Check(ev.Pickup))
                OnThrownItem(ev);

        }
        private void InternalOnPickingUp(PlayerPickingUpItemEventArgs ev)
        {
            if (Check(ev.Pickup))
                OnPickingUp(ev);

        }
        private void InternalOnPickup(PlayerPickedUpItemEventArgs ev)
        {
            if (Check(ev.Item))
                OnPickup(ev);
        }
        private void InternalOnDropping(PlayerDroppingItemEventArgs ev)
        {
            if (Check(ev.Item))
                OnDropping(ev);

        }
        private void InternalOnDropped(PlayerDroppedItemEventArgs ev)
        {
            if (Check(ev.Pickup))
                OnDropped(ev);

        }
        private void InternalOnChangedItem(PlayerChangedItemEventArgs ev)
        {
            OnChangedItem(ev);
        }
        private void InternalOnChangingItem(PlayerChangingItemEventArgs ev)
        {
            if (Check(ev.NewItem))
                OnChangingItem(ev);
        }
        private void InternalOnDying(PlayerDyingEventArgs ev) 
        {
            foreach (Item item in ev.Player.Items.ToList())
            {
                if (Check(item))
                    OnDying(ev);
            }
        }
        private void InternalOnDied(PlayerDeathEventArgs ev) 
        {
            foreach (Item item in ev.Player.Items.ToList())
            {
                if (Check(item))
                    OnDied(ev);
            }
        }

        private void InternalOnHurt(PlayerHurtEventArgs ev)
        {
            OnHurt(ev);
        }

        private void InternalOnHurting(PlayerHurtingEventArgs ev)
        {
            OnHurting(ev);
        }

        protected virtual void OnHurt(PlayerHurtEventArgs ev) { }
        protected virtual void OnHurting(PlayerHurtingEventArgs ev) { }
        protected virtual void OnEscaping(PlayerEscapingEventArgs ev) { }
        protected virtual void OnChangingRole(PlayerChangingRoleEventArgs ev) { }
        protected virtual void OnCuffed(PlayerCuffedEventArgs ev) { }
        protected virtual void OnCuffing(PlayerCuffingEventArgs ev) { }
        protected virtual void OnUpgradingItem(Scp914ProcessingInventoryItemEventArgs ev) { }
        protected virtual void OnUpgradingPickup(Scp914ProcessingPickupEventArgs ev) { }
        protected virtual void OnInspecting(PlayerInspectingItemEventArgs ev) { }
        protected virtual void OnInspected(PlayerInspectedItemEventArgs ev) { }
        protected virtual void OnThrowingItem(PlayerThrowingItemEventArgs ev) { }
        protected virtual void OnThrownItem(PlayerThrewItemEventArgs ev) { }
        protected virtual void OnPickingUp(PlayerPickingUpItemEventArgs ev) { }
        protected virtual void OnPickup(PlayerPickedUpItemEventArgs ev) { }
        protected virtual void OnDropping(PlayerDroppingItemEventArgs ev) { }
        protected virtual void OnDropped(PlayerDroppedItemEventArgs ev) { }
        protected virtual void OnChangedItem(PlayerChangedItemEventArgs ev) { }
        protected virtual void OnChangingItem(PlayerChangingItemEventArgs ev) { }
        protected virtual void OnDying(PlayerDyingEventArgs ev) { }
        protected virtual void OnDied(PlayerDeathEventArgs ev) { }
    }
}