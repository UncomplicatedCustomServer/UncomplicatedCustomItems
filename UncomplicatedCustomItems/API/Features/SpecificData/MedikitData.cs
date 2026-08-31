namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    /// <summary>
    /// The data associated with <see cref="CustomItemType.Medikit"/> <see cref="CustomItem"/>s
    /// </summary>
    public class MedikitData : Data
    {
        /// <summary>
        /// Gets or sets the <see cref="Player.Health"/> that will be regenerated with the use of the medikit
        /// </summary>
        public virtual float Health { get; set; } = 50f;
    }
}
