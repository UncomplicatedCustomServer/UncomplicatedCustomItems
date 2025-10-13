using CustomPlayerEffects;
using InventorySystem.Items.Usables.Scp330;
using LabApi.Events.Arguments.PlayerEvents;
using UncomplicatedCustomItems.API.Attributes;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UnityEngine;

namespace UncomplicatedCustomItems.Examples
{
    [PluginCustomItem]
    public class ExampleCustomCandy : CustomCandy
    {
        /// <inheritdoc/>
        public override uint Id { get; set; } = 30;

        /// <inheritdoc/>
        public override string Name { get; set; } = "Boom Candy";

        /// <inheritdoc/>
        public override string Description { get; set; } = "Yes Rico, Kaboom";

        /// <inheritdoc/>
        public override float Weight { get; set; } = 1.5f;

        /// <inheritdoc/>
        public override ItemType Item { get; set; } = ItemType.SCP330;

        /// <inheritdoc/>
        public override Vector3 Scale { get; set; } = new Vector3(1.2f, 1.2f, 1.2f);

        /// <inheritdoc/>
        public override bool Spawn { get; set; } = false;

        /// <inheritdoc/>
        public override CandyKindID CandyType { get; set; } = CandyKindID.Red;

        /// <inheritdoc/>
        public override string EatingMessage { get; set; } = "Boom!";

        /// <inheritdoc/>
        public override float EatingMessageDuration { get; set; } = 2;

        /// <inheritdoc/>
        public override bool DestroyOnUse { get; set; } = true;

        /// <inheritdoc/>
        public override float Chance { get; set; } = 100;

        /// <inheritdoc/>
        public override bool ApplyEffects { get; set; } = false;

        protected override void OnEffectsApplying(PlayerItemUsageEffectsApplyingEventArgs ev)
        {
            ev.Player.EnableEffect<Scp207>(1, 20);
            base.OnEffectsApplying(ev);
        }
    }
}