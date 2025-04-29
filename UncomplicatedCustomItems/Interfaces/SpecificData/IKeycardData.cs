namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    /// <summary>
    /// The interface associated with <see cref="CustomItemType.Keycard"/>
    /// </summary>
    public interface IKeycardData : IData
    {
        public abstract string TintColor { get; set; }
        public abstract int Containment { get; set; }
        public abstract int Armory { get; set; }
        public abstract int Admin { get; set; }
        public abstract string PermissionsColor { get; set; }
        public abstract string Name { get; set; }
        public abstract string Label { get; set; }
        public abstract string SerialNumber { get; set; }
        public abstract byte WearDetail { get; set; }
        public abstract string LabelColor { get; set; }
        public abstract int Rank { get; set; }
    }
}
