using CustomPlayerEffects;
using PlayerRoles;

namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public abstract class CustomExplosiveGrenade : BaseCustomItem
    {
        /// <summary>
        /// Gets or sets the maximum radius of the grenade.
        /// </summary>
        public abstract float MaxRadius { get; set; }

        /// <summary>
        /// Gets or sets the multiplier for damage against <see cref="Team.SCPs"/> players.
        /// </summary>
        public abstract float ScpDamageMultiplier { get; set; }

        /// <summary>
        /// Gets or sets how long the <see cref="Burned"/> effect will last.
        /// </summary>
        public abstract float BurnDuration { get; set; }

        /// <summary>
        /// Gets or sets how long the <see cref="Deafened"/> effect will last.
        /// </summary>
        public abstract float DeafenDuration { get; set; }

        /// <summary>
        /// Gets or sets how long the <see cref="Concussed"/> effect will last.
        /// </summary>
        public abstract float ConcussDuration { get; set; }

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

        /// <summary>
        /// Gets or sets the player damage multiplier applied.
        /// </summary>
        public abstract float PlayerDamageMultiplier { get; set; }

        /// <summary>
        /// Gets or sets the door damage multiplier applied.
        /// </summary>
        public abstract float DoorDamageMultiplier { get; set; }
    }
}