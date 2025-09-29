using MapGeneration;
using CustomPlayerEffects;

namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public abstract class CustomFlashGrenade : APICustomItem
    {
        /// <summary>
        /// Gets or sets the minimum duration of player can take the effect.
        /// </summary>
        public abstract float MinimalDurationEffect { get; set; }

        /// <summary>
        /// Gets or sets the additional duration of the <see cref="Blindness"/> effect.
        /// </summary>
        public abstract float AdditionalBlindedEffect { get; set; }

        /// <summary>
        /// Gets or sets the how mush the flash grenade going to be intensified when explode at <see cref="RoomName.Surface"/>.
        /// </summary>
        public abstract float SurfaceDistanceIntensifier { get; set; }

        /// <summary>
        /// Gets or sets how long the fuse will last.
        /// </summary>
        public abstract float FuseTime { get; set; }

        /// <summary>
        /// Gets or sets wether or not the grenade will explode on impact
        /// </summary>
        public abstract bool ExplodeOnImpact { get; set; }

        /// <summary>
        /// Gets or sets the time to pull out the pin
        /// </summary>
        public abstract float PinPullTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether players can pickup grenade after throw.
        /// </summary>
        public abstract bool Repickable { get; set; }
    }
}