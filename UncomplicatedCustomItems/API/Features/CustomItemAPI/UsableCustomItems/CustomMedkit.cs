using LabApi.Features.Wrappers;

namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public abstract class CustomMedkit : UsableItem
    {
        /// <summary>
        /// Gets or sets the <see cref="Player.Health"/> that will be regenerated with the use of the medikit
        /// </summary>
        public float Health { get; set; } = 50f;

        /// <summary>
        /// Gets or sets whether the value can surpass the <see cref="Player.MaxHealth"/>
        /// </summary>
        public bool MoreThanMax { get; set; } = false;
    }
}