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
    public class ExampleCustomGrenade : CustomExplosiveGrenade
    {
        /// <inheritdoc/>
        public override uint Id { get; set; } = 1;

        /// <inheritdoc/>
        public override string Name { get; set; } = "The Grenade";

        /// <inheritdoc/>
        public override string Description { get; set; } = ":D";

        /// <inheritdoc/>
        public override float Weight { get; set; } = 1.5f;

        /// <inheritdoc/>
        public override ItemType Item { get; set; } = ItemType.GrenadeHE;

        /// <inheritdoc/>
        public override Vector3 Scale { get; set; } = new Vector3(1.2f, 1.2f, 1.2f);

        /// <inheritdoc/>
        public override bool Spawn { get; set; } = false;

        /// <inheritdoc/>
        public override float MaxRadius { get; set; } = 10f;

        /// <inheritdoc/>
        public override float ScpDamageMultiplier { get; set; } = 10f;

        /// <inheritdoc/>
        public override float BurnDuration { get; set; } = 10f;

        /// <inheritdoc/>
        public override float DeafenDuration { get; set; } = 16f;

        /// <inheritdoc/>
        public override float ConcussDuration { get; set; } = 5.6f;

        /// <inheritdoc/>
        public override float FuseTime { get; set; } = 10f;

        /// <inheritdoc/>
        public override bool ExplodeOnImpact { get; set; } = true;

        /// <inheritdoc/>
        public override float PinPullTime { get; set; } = 1.5f;

        /// <inheritdoc/>
        public override bool Repickable { get; set; } = true;

        /// <inheritdoc/>
        public override float PlayerDamageMultiplier { get; set; } = 3f;

        /// <inheritdoc/>
        public override float DoorDamageMultiplier { get; set; } = 10f;
    }
}