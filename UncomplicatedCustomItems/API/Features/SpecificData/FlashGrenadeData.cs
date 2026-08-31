using MapGeneration;
using UncomplicatedCustomItems.API.Enums;
namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    /// <summary>
    /// The data associated with <see cref="CustomItemType.FlashGrenade"/> <see cref="CustomItem"/>s
    /// </summary>
    public class FlashGrenadeData : Data
    {
        /// <summary>
        /// Gets or sets the minimum duration of player can take the effect.
        /// </summary>
        public virtual float MinimalDurationEffect { get; set; } = 5f;

        /// <summary>
        /// Gets or sets the additional duration of the <see cref="CustomPlayerEffects.Blindness"/> effect.
        /// </summary>
        public virtual float AdditionalBlindedEffect { get; set; } = 10f;

        /// <summary>
        /// Gets or sets the how mush the flash grenade going to be intensified when detonated on <see cref="RoomName.Outside"/>.
        /// </summary>
        public virtual float SurfaceDistanceIntensifier { get; set; } = 15f;

        /// <summary>
        /// Gets or sets how long the fuse will last.
        /// </summary>
        public virtual float FuseTime { get; set; } = 3f;

        /// <summary>
        /// The time to pull out the pin
        /// </summary>
        public virtual float PinPullTime { get; set; } = 0.5f;

        /// <summary>
        /// Gets or sets a value indicating whether players can pickup grenade after throw.
        /// </summary>
        public virtual bool Repickable { get; set; } = false;

        public virtual bool ExplodeOnImpact { get; set; }
    }
}
