using Exiled.API.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using UncomplicatedCustomItems.API.Features.Data;
using UncomplicatedCustomItems.API.Serializable;
using UnityEngine;

namespace UncomplicatedCustomItems
{
    public class Config : IConfig
    {
        [Description("Is enabled or not")]
        public bool IsEnabled { get; set; }

        [Description("Is debug or not")]
        public bool Debug { get; set; }

        [Description("List of custom armors")]
        public Dictionary<int, SerializableCustomArmor> CustomArmors { get; set; } = new Dictionary<int, SerializableCustomArmor>()
        {
            {
                0, 
                new SerializableCustomArmor()
                {
                    Name = "Very heavy armor",
                    Description = "Very heavy armor",
                    Id = 0,
                    Model = ItemType.ArmorHeavy,
                    Info = new ArmorInfo()
                    {
                        BodyProtection = 99,
                        HeadProtection = 99
                    }
                }
            }
        };

        [Description("List of custom weapons")]
        public Dictionary<int, SerializableCustomWeapon> CustomWeapons { get; set; } = new Dictionary<int, SerializableCustomWeapon>()
        {
            {
                0,
                new SerializableCustomWeapon()
                {
                    Name = "Kalashnikov",
                    Description = "Kalashnikov",
                    Id = 0,
                    Model = ItemType.GunAK,
                    Scale = Vector3.one,
                    Info = new WeaponInfo()
                    {
                        FireRate = 5,
                        MaxAmmo = 35,
                        Damage = 5
                    }
                }
            }
        };

        [Description("List of custom items")]
        public Dictionary<int, SerializableCustomItem> CustomItems { get; set; } = new Dictionary<int, SerializableCustomItem>()
        {
            {
                0,
                new SerializableCustomItem()
                {
                    Name = "Note",
                    Description = "Just contains text from admin or other clown",
                    Id = 0,
                    Model = ItemType.KeycardGuard,
                    Scale = new Vector3(1, 0.1f, 1),
                    Info = new ItemInfo()
                    {
                        Command = string.Empty,
                        Response = "Hello, welcome to UncomplicatedCustomItems by SpGerg!"
                    }
                }
            },
            {
                1,
                new SerializableCustomItem()
                {
                    Name = "Detonator",
                    Description = "Boom.... lol",
                    Id = 1,
                    Model = ItemType.KeycardChaosInsurgency,
                    Scale = new Vector3(1, 1, 1),
                    Info = new ItemInfo()
                    {
                        Command = "/SERVER_EVENT DETONATION_INSTANT",
                        Response = "Flashbacks..."
                    }
                }
            }
        };
    }
}
