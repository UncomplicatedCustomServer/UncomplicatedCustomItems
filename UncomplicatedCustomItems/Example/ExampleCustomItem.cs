using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.Events.EventArgs.Player;
using UncomplicatedCustomItems.API.Attributes;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.Events;
using UncomplicatedCustomItems.API.Interfaces;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using UncomplicatedCustomItems.API.Extensions;
using UnityEngine;
using InventorySystem.Items.Firearms.Attachments;

namespace UncomplicatedCustomItems.Examples
{
    [PluginCustomItem]
    public class ExampleCustomItem : CustomItem // You could also use the toolgun as a example.
    {
        public override uint Id { get; set; } = 1;
        
        public override string Name { get; set; } = "My Custom Detonator";
        
        public override string Description { get; set; } = "My custom device - 05/09/2025";
        
        public override string BadgeName { get; set; } = "My Custom Items";
        
        public override string BadgeColor { get; set; } = "blue";
        
        public override float Weight { get; set; } = 1.5f;
        
        public override bool Reusable { get; set; } = true;
        
        public override ItemType Item { get; set; } = ItemType.Coin;

        public override ISpawn Spawn { get; set; } = new SpawnData();

        public override CustomFlags? CustomFlags { get; set; } = API.Enums.CustomFlags.DieOnUse | API.Enums.CustomFlags.DieOnDrop;

        public override IFlagSettings FlagSettings { get; set; } = new FlagSettingsData();

        public override Vector3 Scale { get; set; } = new Vector3(1.2f, 1.2f, 1.2f);
        
        public override CustomItemType CustomItemType { get; set; } = CustomItemType.Item;
        
        public override IData CustomData { get; set; } = new CustomItemData();
    }

    public class CustomItemData : ItemData
    {
        public override ItemEvents Event { get; set; } = ItemEvents.Use;
        public override string? Command { get; set; } = "/customcommand %p_id%";
        public override float CoolDown { get; set; } = 2.5f;
        public override string ConsoleMessage { get; set; } = "Player has used the custom detonator!";
        public override string BroadcastMessage { get; set; } = "A custom detonator has been activated!";
        public override ushort BroadcastDuration { get; set; } = 5;
        public override string HintMessage { get; set; } = "You've activated the custom detonator";
        public override float HintDuration { get; set; } = 3.5f;
        public override bool DestroyAfterUse { get; set; } = false;
    }

    public class SpawnData : Spawn
    {
        public override bool DoSpawn { get; set; } = false;
        public override uint Count { get; set; } = 1;
        public override bool? PedestalSpawn { get; set; } = false;
        public override List<Vector3> Coords { get; set; } = new();
        public override List<DynamicSpawn> DynamicSpawn { get; set; } =
        [
            new()
            {
                Room = RoomType.Lcz914,
                Chance = 30,
                Coords = Vector3.zero
            }
        ];
        public override List<ZoneType> Zones { get; set; } = new()
        {
            ZoneType.HeavyContainment,
            ZoneType.Entrance
        };
        public override bool ReplaceExistingPickup { get; set; } = false;
        public override bool ForceItem { get; set; } = false;
    }

    public class FlagSettingsData : FlagSettings
    {
        public override List<DieOnDropSettings?> DieOnDropSettings { get; set; } =
        [
            new()
            {
                DeathMessage = "Dropped %name%",
                Vaporize = true
            }
        ];
        public override List<CantDropSettings?> CantDropSettings { get; set; } =
        [
            new()
            {
                HintOrBroadcast = "hint",
                Message = "You cant drop %name%!",
                Duration = 10
            }
        ];
    }
    public class Events : CustomItemEventHandler
    {
        // Register with CustomItemEventHandler.Init<CUSTOMITEMNAMESPACE.Events>();
        /// <summary>
        /// This is a example of how to setup the <see cref="CustomItemEventHandler.OnShot"/> event from <see cref="CustomItemEventHandler"/>
        /// Please note that the event will trigger for all items and you will have to add your own checks
        /// </summary>
        /// <param name="ev"></param>
        public override void OnShot(ShotEventArgs ev)
        {
            SummonedCustomItem item = ev.Item.TryGetSummonedCustomItem();
            if (item != null && item.CustomItem.Id == 1)
                ev.Player.Kill(ev.Firearm.HitscanHitregModule.GetHandler(ev.Damage));

            base.OnShot(ev);
        }

        /// <summary>
        /// This is a example of how to setup the <see cref="CustomItemEventHandler.OnOwnerDroppedItem"/> event to give the <see cref="ICustomItem"/> the <see cref="AttachmentName.HoloSight"/> attachment.
        /// Please note that the event will trigger for all items and you will have to add your own checks
        /// </summary>
        /// <param name="ev"></param>
        public override void OnOwnerDroppedItem(DroppedItemEventArgs ev)
        {
            SummonedCustomItem item = ev.Pickup.TryGetSummonedCustomItem();
            if (item != null && item.CustomItem.Id == 1)
                item.AddAttachment("HoloSight");

            base.OnOwnerDroppedItem(ev);
        }
    }
}