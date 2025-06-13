using System.Collections.Generic;
using Exiled.API.Enums;
using UncomplicatedCustomItems.API.Attributes;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UncomplicatedCustomItems.API.Interfaces;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using UnityEngine;

namespace UncomplicatedCustomItems.API.ToolGun
{
    [PluginCustomItem]
    public class ToolGun : CustomItem
    {
        public override uint Id { get; set; } = 20;
        public override string Name { get; set; } = "ToolGun";
        public override string Description { get; set; } = "The UCI ToolGun";
        public override string BadgeName { get; set; } = "";
        public override string BadgeColor { get; set; } = "";
        public override float Weight { get; set; } = 1.5f;
        public override bool Reusable { get; set; } = true;
        public override ItemType Item { get; set; } = ItemType.GunCOM18;
        public override ISpawn Spawn { get; set; } = new SpawnData();
        public override CustomFlags? CustomFlags { get; set; } = Enums.CustomFlags.ToolGun | Enums.CustomFlags.InfiniteAmmo | Enums.CustomFlags.WorkstationBan;
        public override IFlagSettings FlagSettings { get; set; } = new FlagSettingsData();
        public override Vector3 Scale { get; set; } = new Vector3(1f, 1f, 1f);
        public override CustomItemType CustomItemType { get; set; } = CustomItemType.Weapon;
        public override IData CustomData { get; set; } = new CustomItemData();
    }

    public class CustomItemData : WeaponData
    {
        public override float Damage { get; set; } = 2.75f;
        public override int MaxBarrelAmmo { get; set; } = 1;
        public override int MaxAmmo { get; set; } = 150;
        public override int MaxMagazineAmmo { get; set; } = 150;
        public override int AmmoDrain { get; set; } = 1;
        public override float Penetration { get; set; } = 1.24f;
        public override float Inaccuracy { get; set; } = 1.24f;
        public override float AimingInaccuracy { get; set; } = 1.24f;
        public override float DamageFalloffDistance { get; set; } = 1f;
        public override string Attachments { get; set; } = "DotScope, Flashlight";
        public override bool EnableFriendlyFire { get; set; } = false;
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

    }
}