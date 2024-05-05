using Exiled.API.Enums;
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

        [Description("Do enable the developer (debug) mode?")]
        public bool Debug { get; set; } = true;
        [Description("A list of custom items")]
        public List<YAMLCustomItem> CustomItems { get; set; } = new()
        {
            new()
            {
                CustomData = YAMLCaster.Encode(new ItemData()
                {
                    Event = ItemEvents.Command,
                    Command = "/SERVER_EVENT DETONATION_INSTANT",
                    ConsoleMessage = "UHUHUHUH!"
                })
            },
            new()
            {
                Id = 2,
                Name = "MagicWeapon",
                Description = "A magic weapon with an incredible firerate",
                Item = ItemType.GunCOM18,
                CustomItemType = CustomItemType.Weapon,
                Scale = Vector3.one,
                CustomData = YAMLCaster.Encode(new WeaponData())
            },
            new()
            {
                Id = 3,
                Name = "Titanium Armor",
                Description = "A super heavy armor",
                Item = ItemType.ArmorHeavy,
                CustomItemType = CustomItemType.Armor,
                Scale = Vector3.one,
                CustomData = YAMLCaster.Encode(new ArmorData()
                {
                    HeadProtection = 150,
                    BodyProtection = 200,
                    RemoveExcessOnDrop = false,
                    StaminaUseMultiplier = 2
                })
            },
            new()
            {
                Id = 4,
                Name = "Incredible beautiful keycard",
                Description = "UWU owo keycard",
                Item = ItemType.KeycardJanitor,
                CustomItemType = CustomItemType.Keycard,
                Scale = Vector3.one,
                CustomData = YAMLCaster.Encode(new KeycardData()
                {
                    Permissions = KeycardPermissions.AlphaWarhead | KeycardPermissions.Checkpoints
                })
            },
            new()
            {
                Id = 5,
                Name = "My favourite grenade",
                Description = "Throw it my friend :)",
                Item = ItemType.GrenadeHE,
                CustomItemType = CustomItemType.ExplosiveGrenade,
                Scale = Vector3.one,
                CustomData = YAMLCaster.Encode(new ExplosiveGrenadeData()
                {
                    MaxRadius = 250f
                })
            },
            new()
            {
                Id = 6,
                Name = "Blinder",
                Description = "Make every people in the facility blind",
                Item = ItemType.GrenadeFlash,
                CustomItemType = CustomItemType.FlashGrenade,
                Scale = Vector3.one,
                CustomData = YAMLCaster.Encode(new FlashGrenadeData() 
                {
                    AdditionalBlindedEffect = 250f
                })
            },
            new()
            {
                Id = 7,
                Name = "Overpowered medikit",
                Description = "This medikit will heal you 100%",
                Item = ItemType.Medkit,
                CustomItemType = CustomItemType.Medikit,
                Scale = new(2, 2, 2),
                CustomData = YAMLCaster.Encode(new MedikitData()
                {
                    Health = 250f
                })
            },
            new()
            {
                Id = 8,
                Name = "Really fast painkillers",
                Description = "This painkillers regenerate lots of health within seconds but you'll have to wait...",
                Item = ItemType.Painkillers,
                CustomItemType = CustomItemType.Painkillers,
                Scale = new(5, 5, 5),
                CustomData = YAMLCaster.Encode(new PainkillersData()
                {
                    TickHeal = 1f,
                    TickTime = 0.1f,
                    TimeBeforeStartHealing = 7.5f,
                    TotalHealing = 50f
                })
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