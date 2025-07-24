namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    /// <summary>
    /// The interface associated with <see cref="CustomItemType.MicroHID"/>
    /// </summary>
    public interface IMicroHIDData
    {
        public abstract float Damage { get; set; }
        public abstract float Energy { get; set; }
        public abstract bool CanExplode { get; set; }
    }
}
