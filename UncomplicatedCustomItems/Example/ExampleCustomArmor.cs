using CustomPlayerEffects;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using MEC;
using System.Linq;
using UncomplicatedCustomItems.API.Attributes;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.ToolGun;
using UnityEngine;

namespace UncomplicatedCustomItems.Examples
{
    /// <summary>
    /// Example of how to make a <see cref="CustomItem"/> in C#
    /// You could also use the <see cref="API.ToolGun.ToolGun"/> as a example.
    /// </summary>
    [PluginCustomItem]
    public class ExampleCustomArmor : CustomArmor
    {
        /// <inheritdoc/>
        public override uint Id { get; set; } = 1;

        /// <inheritdoc/>
        public override string Name { get; set; } = "Fast Armor";

        /// <inheritdoc/>
        public override string Description { get; set; } = "This will explode when you die...";

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

        protected override void OnPickup(PlayerPickedUpItemEventArgs ev)
        {
            foreach (Item item in ev.Player.Items.ToList())
            {
                if (Check(item))
                    ev.Player.EnableEffect<MovementBoost>(20, float.MaxValue, false);
            }

            base.OnPickup(ev);
        }

        protected override void OnDropped(PlayerDroppedItemEventArgs ev)
        {
            foreach (Item item in ev.Player.Items.ToList())
            {
                if (Check(item))
                    ev.Player.DisableEffect<MovementBoost>();
            }

            base.OnDropped(ev);
        }

        protected override void OnDying(PlayerDyingEventArgs ev)
        {
            foreach (Item item in ev.Player.Items.ToList())
            {
                if (Check(item))
                {
                    ev.Player.RemoveItem(item);
                    ExplosiveGrenadeProjectile grenade = (ExplosiveGrenadeProjectile)TimedGrenadeProjectile.SpawnActive(ev.Player.Position, ItemType.GrenadeHE, ev.Player, 0.1);
                    grenade.MaxRadius = 20f;
                }
            }

            base.OnDying(ev);
        }
    }
}