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
                    Command = string.Empty,
                    Response = "Hello, welcome to UncomplicatedCustomItems by SpGerg!"
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
                    Command = "/SERVER_EVENT DETONATION_INSTANT",
                    Response = "Flashbacks..."
                }
            },
            {
                2,
                new SerializableCustomItem()
                {
                    Name = "Detonator",
                    Description = "Boom.... lol",
                    Id = 2,
                    Model = ItemType.SCP207,
                    Scale = new Vector3(1, 1, 1),
                    Command = "/SERVER_EVENT DETONATION_INSTANT",
                    Response = "Flashbacks..."
                }
            }
        };
    }
}
