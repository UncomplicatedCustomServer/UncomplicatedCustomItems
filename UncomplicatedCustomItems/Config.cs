using Exiled.API.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.Elements;
using UncomplicatedCustomItems.Elements.SpecificData;
using UnityEngine;

namespace UncomplicatedCustomItems
{
    public class Config : IConfig
    {
        [Description("Is enabled or not")]
        public bool IsEnabled { get; set; } = true;

<<<<<<< HEAD
        [Description("Do enable the developer (debug) mode?")]
        public bool Debug { get; set; } = true;
        [Description("A list of custom items")]
        public List<YAMLCustomItem> CustomItems { get; set; } = new()
=======
        [Description("Is debug or not")]
        public bool Debug { get; set; }

        [Description("List of custom keycards")]
        public Dictionary<int, SerializableCustomKeycard> CustomKeycards { get; set; } = new Dictionary<int, SerializableCustomKeycard>()
>>>>>>> 2ff79ee1861a7035b48d099ce36d6c27c501525a
        {
            new()
            {
                CustomData = YAMLCaster.Encode(new ItemData()
                {
<<<<<<< HEAD
                    Event = ItemEvents.Command,
                    Command = "/SERVER_EVENT DETONATION_INSTANT",
                    ConsoleMessage = "UHUHUHUH!"
                })
=======
                    Name = "Exiled keycard staff",
                    Description = "Useless card",
                    Id = 0,
                    Model = ItemType.KeycardFacilityManager,
                    Scale = Vector3.one,
                    SpawnPoint = new[]
                    {
                        new ItemSpawnPoint()
                        {
                            Position = Vector3.zero,
                            Location = Exiled.API.Enums.SpawnLocationType.InsideLczWc
                        },
                    },
                    Info = new KeycardInfo()
                    {
                        Permissions = Exiled.API.Enums.KeycardPermissions.Checkpoints
                    }
                }
            }
        };

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
                    },
                    SpawnPoint = new[] {
                        new ItemSpawnPoint()
                        {
                            Location = Exiled.API.Enums.SpawnLocationType.Inside914,
                            Chance = 100,
                            Position = Vector3.one,
                            Name = "Chipi chapa"
                        }
                    },
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
                        Commands = new string[0],
                        Response = "Hello, welcome to UncomplicatedCustomItems by SpGerg!"
                    }
                }
>>>>>>> 2ff79ee1861a7035b48d099ce36d6c27c501525a
            },
            new()
            {
<<<<<<< HEAD
                Id = 2,
                Name = "MagicWeapon",
                Description = "A magic weapon with an incredible firerate",
                Item = ItemType.GunCOM18,
                CustomItemType = CustomItemType.Weapon,
                Scale = Vector3.one,
                CustomData = YAMLCaster.Encode(new WeaponData())
=======
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
                        Commands = new []
                        {
                            "/SERVER_EVENT DETONATION_INSTANT"
                        },
                        Response = "Flashbacks..."
                    }
                }
>>>>>>> 2ff79ee1861a7035b48d099ce36d6c27c501525a
            }
        };
        [Description("The hint message that will appear every time that you pick up a custom item. %name% is the item's name, %desc% is the item's description")]
        public string PickedUpMessage { get; set; } = "You have picked up a %name% who's a %desc%";
        [Description("The duration of that hint")]
        public float PickedUpMessageDuration { get; set; } = 3f;
        [Description("The hint message that will appear every time that you select a custom item. %name% is the item's name, %desc% is the item's description")]
        public string SelectedMessage { get; set; } = "You have picked up a %name% who's a %desc%";
        [Description("The duration of that hint")]
        public float SelectedMessageDuration { get; set; } = 3f;
    }
}
