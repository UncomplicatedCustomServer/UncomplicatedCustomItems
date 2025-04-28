using System.Collections.Generic;
using Exiled.API.Enums;
using Interactables.Interobjects.DoorUtils;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using UnityEngine;
using YamlDotNet.Serialization;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    /// <summary>
    /// The data associated with <see cref="CustomItemType.Keycard"/> <see cref="CustomItem"/>s
    /// </summary>
    public class KeycardData : Data, IKeycardData
    {
        public int Containment { get; set; } = 1;
        public int Armory { get; set; } = 1;
        public int Admin { get; set; } = 1;
        public string TintColor { get; set; } = "Red";
        public string PermissionsColor { get; set; } = "Red";
        public string Name { get; set; } = "";
        public string Label { get; set; } = "";

    }
}
