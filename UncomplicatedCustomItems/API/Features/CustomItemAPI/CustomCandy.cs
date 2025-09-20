using InventorySystem.Items.Usables.Scp330;
using UncomplicatedCustomItems.API.Features.CandySerialization;

namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public abstract class CustomCandy : BaseCustomItem
    {
        /// <summary>
        /// Set when the user gets the candy
        /// </summary>
        public SerializedCandy SerializedCandy { get; internal set; }
        
        /// <summary>
        /// Gets or sets the Candy Type to spawn as
        /// </summary>
        public abstract CandyKindID CandyType { get; set; }

        /// <summary>
        /// Gets or sets the hint message shown when the candy is eaten
        /// </summary>
        public virtual string EatingMessage { get; set; }

        /// <summary>
        /// Gets or sets the hint message duration
        /// </summary>
        public virtual float EatingMessageDuration { get; set; }

        /// <summary>
        /// Gets or sets whether the candy will be destroyed when the player uses it.
        /// </summary>
        public abstract bool DestroyOnUse { get; set; }

        /// <summary>
        /// Gets or sets the chance for the candy to spawn.
        /// </summary>
        public abstract float Chance { get; set; }

        /// <summary>
        /// Gets or sets whether the candy will apply its effects to the player.
        /// </summary>
        public abstract bool ApplyEffects { get; set; }

        /// <summary>
        /// Gets or sets whether the candy can spawn as a candy bag on the map
        /// </summary>
        public abstract bool AllowSpawningAsItem { get; set; }
    }
}