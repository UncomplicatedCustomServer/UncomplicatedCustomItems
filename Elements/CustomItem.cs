using System.Collections.Generic;
using UncomplicatedCustomItems.Interfaces;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using UnityEngine;
using System.ComponentModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

#nullable enable


namespace UncomplicatedCustomItems.Elements
{
    public class CustomItem : ICustomItem
    {
        public uint Id { get; set; } = 2;

        [Description("The name of the custom item")]
        public string Name { get; set; } = "<color=#3BAAC4>FunnyGun</color>";

        [Description("The description of the custom item")]
        public string Description { get; set; } = "A magic gun that has a shotgun-like bullet spread";

        [Description("The weight of the custom item")]
        public float Weight { get; set; } = 2f;

        [Description("The Item base for the custom item")]
        public ItemType Item { get; set; } = ItemType.GunFRMG0;

        [Description("The scale of the custom item, 0 0 0 means disabled")]
        public Vector3 Scale { get; set; } = new Vector3(1, 1, 1);

        [Description("The spawn settings for the item")]
        public ISpawn Spawn { get; set; } = new Spawn();

        public CustomItemType CustomItemType { get; set; } = CustomItemType.Weapon;

        public IData CustomData { get; set; } = new CustomWeaponData
        {
            Damage = 2.75f,
            MaxBarrelAmmo = 10,
            MaxAmmo = 150,
            MaxMagazineAmmo = 150,
            AmmoDrain = 1,
            Penetration = 1.24f,
            Inaccuracy = 1.24f,
            DamageFalloffDistance = 1,
        };
    }

    public class CustomWeaponData : IData
    {
        public float Damage { get; set; }
        public byte MaxBarrelAmmo { get; set; }
        public byte MaxAmmo { get; set; }
        public byte MaxMagazineAmmo { get; set; }
        public int AmmoDrain { get; set; }
        public float Penetration { get; set; }
        public float Inaccuracy { get; set; }
        public float DamageFalloffDistance { get; set; }
    }
}
