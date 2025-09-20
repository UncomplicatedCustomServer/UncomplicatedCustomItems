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
    public class ExampleCustomWeapon : CustomWeapon
    {
        /// <inheritdoc/>
        public override uint Id { get; set; } = 1;

        /// <inheritdoc/>
        public override string Name { get; set; } = "Com-19";

        /// <inheritdoc/>
        public override string Description { get; set; } = ":D";

        /// <inheritdoc/>
        public override float Weight { get; set; } = 1.5f;

        /// <inheritdoc/>
        public override ItemType Item { get; set; } = ItemType.GunCOM18;

        /// <inheritdoc/>
        public override Vector3 Scale { get; set; } = new Vector3(1.2f, 1.2f, 1.2f);

        /// <inheritdoc/>
        public override bool Spawn { get; set; } = false;

        /// <inheritdoc/>
        public override float Damage { get; set; } = 4f;

        /// <inheritdoc/>
        public override int MaxAmmo { get; set; } = 18;

        /// <inheritdoc/>
        public override int MaxMagazineAmmo { get; set; } = 18;

        /// <inheritdoc/>
        public override int MaxBarrelAmmo { get; set; } = 1;

        /// <inheritdoc/>
        public override float Penetration { get; set; } = 1;
        
        /// <inheritdoc/>
        public override float Inaccuracy { get; set; } = 3;

        /// <inheritdoc/>
        public override float AimingInaccuracy { get; set; } = 1;

        /// <inheritdoc/>
        public override float DamageFalloffDistance { get; set; } = 100;

        /// <inheritdoc/>
        public override List<AttachmentName> Attachments { get; set; } = [AttachmentName.Flashlight];

        /// <inheritdoc/>
        public override bool EnableFriendlyFire { get; set; } = false;
    }
}