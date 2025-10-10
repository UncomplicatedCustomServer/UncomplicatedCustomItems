using UncomplicatedCustomItems.API.Attributes;
using UncomplicatedCustomItems.API.Features;
using UnityEngine;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;

namespace UncomplicatedCustomItems.Examples
{
    /// <summary>
    /// Example of how to make a <see cref="CustomItem"/> in C#
    /// You could also use the <see cref="API.ToolGun.ToolGun"/> as a example.
    /// </summary>
    [PluginCustomItem]
    public class ExampleCustomKeycard : CustomKeycard
    {
        /// <inheritdoc/>
        public override uint Id { get; set; } = 1;

        /// <inheritdoc/>
        public override string Name { get; set; } = "Blue";

        /// <inheritdoc/>
        public override string Description { get; set; } = ":D";

        /// <inheritdoc/>
        public override float Weight { get; set; } = 1.5f;

        /// <inheritdoc/>
        public override ItemType Item { get; set; } = ItemType.KeycardCustomTaskForce;

        /// <inheritdoc/>
        public override Vector3 Scale { get; set; } = new Vector3(1.2f, 1.2f, 1.2f);

        /// <inheritdoc/>
        public override bool Spawn { get; set; } = false;

        /// <inheritdoc/>
        public override int Admin { get; set; } = 3;

        /// <inheritdoc/>
        public override int Armory { get; set; } = 2;

        /// <inheritdoc/>
        public override int Containment { get; set; } = 2;

        /// <inheritdoc/>
        public override string PermissionsColor { get; set; } = "#00FF00";

        /// <inheritdoc/>
        public override string TintColor { get; set; } = "#0000FF";

        /// <inheritdoc/>
        public override string HolderName { get; set; } = "%name%";

        /// <inheritdoc/>
        public override string SerialNumber { get; set; } = "24839210239";

        public override string Label { get; set; } = ":D";

        /// <inheritdoc/>
        public override byte WearDetail { get; set; } = 5;

        /// <inheritdoc/>
        public override string LabelColor { get; set; } = "#FFFFFF";

        /// <inheritdoc/>
        public override int Rank { get; set; } = 2;


    }
}