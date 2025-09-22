using System.Collections.Generic;
using UncomplicatedCustomItems.API.Attributes;
using InventorySystem.Items.Firearms.Attachments;

namespace UncomplicatedCustomItems.API.ToolGun
{
    [PluginCustomItem]
    public class ToolGun : Features.CustomItemAPI.ToolGun
    {
        /// <inheritdoc/>
        public override uint Id { get; set; } = 20;

        /// <inheritdoc/>
        public override string Name { get; set; } = "ToolGun";

        /// <inheritdoc/>
        public override string Description { get; set; } = "The UCI ToolGun";

        /// <inheritdoc/>
        public override float Weight { get; set; } = 1.5f;

        /// <inheritdoc/>
        public override bool Reusable { get; set; } = true;

        /// <inheritdoc/>
        public override ItemType Item { get; set; } = ItemType.GunCOM18;

        /// <inheritdoc/>
        public override bool Spawn { get; set; } = false;

        /// <inheritdoc/>
        public override float Damage { get; set; } = 0f;

        /// <inheritdoc/>
        public override int MaxAmmo { get; set; } = 2000;

        /// <inheritdoc/>
        public override int MaxMagazineAmmo { get; set; } = 2000;

        /// <inheritdoc/>
        public override int MaxBarrelAmmo { get; set; } = 1;

        /// <inheritdoc/>
        public override float Penetration { get; set; } = 1;
        
        /// <inheritdoc/>
        public override float Inaccuracy { get; set; } = 1;

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