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
        public bool Debug { get; set; } = false;
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
                Name = "<color=#3BAAC4>FunnyGun</color>",
                Description = "A magic gun that has a shotgun like bullet spread",
                Item = ItemType.GunFRMG0,
                CustomItemType = CustomItemType.Weapon,
                Scale = Vector3.one,
                CustomData = YAMLCaster.Encode(new WeaponData())
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