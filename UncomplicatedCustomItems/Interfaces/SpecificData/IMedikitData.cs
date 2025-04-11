namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    public interface IMedikitData
    {
        /// <summary>
    /// The interface associated with <see cref="CustomItemType.Medikit"/>
    /// </summary>
        public abstract float Health { get; set; }

        public abstract bool MoreThanMax { get; set; }
    }
}
