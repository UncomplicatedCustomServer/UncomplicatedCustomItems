using System.Collections.Generic;
using UncomplicatedCustomItems.API.Attributes;
using UncomplicatedCustomItems.API.Features;
using UnityEngine;
using InventorySystem.Items.Firearms.Attachments;
using UncomplicatedCustomItems.API.ToolGun;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;

namespace UncomplicatedCustomItems.Examples
{
    /// <summary>
    /// Example of how to make a <see cref="CustomItem"/> in C#
    /// You could also use the <see cref="ToolGun.ToolGun"/> as a example.
    /// </summary>
    [PluginCustomItem]
    public class ExampleCustomArmor : CustomArmor
    {
        /// <inheritdoc/>
        public override uint Id { get; set; } = 1;

        /// <inheritdoc/>
        public override string Name { get; set; } = "Fast Armor";

        /// <inheritdoc/>
        public override string Description { get; set; } = ":D";

        /// <inheritdoc/>
        public override float Weight { get; set; } = 1.5f;

        /// <inheritdoc/>
        public override ItemType Item { get; set; } = ItemType.ArmorLight;

        /// <inheritdoc/>
        public override Vector3 Scale { get; set; } = new Vector3(1.2f, 1.2f, 1.2f);

        /// <inheritdoc/>
        public override bool Spawn { get; set; } = false;

        /// <inheritdoc/>
        public override int HeadProtection { get; set; } = 100;

        /// <inheritdoc/>
        public override int BodyProtection { get; set; } = 100;

        /// <inheritdoc/>
        public override float StaminaUseMultiplier { get; set; } = 0.1f;

        /// <inheritdoc/>
        public override float StaminaRegenMultiplier { get; set; } = 30f;
    }
}