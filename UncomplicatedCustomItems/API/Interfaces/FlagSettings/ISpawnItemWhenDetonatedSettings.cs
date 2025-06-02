namespace UncomplicatedCustomItems.API.Interfaces
{
    public interface ISpawnItemWhenDetonatedSettings
    {
        public abstract string? ItemType { get; set; }
        public abstract uint? ItemId { get; set; }
        public abstract float? TimeTillDespawn { get; set; }
        public abstract uint? Chance { get; set; }
        public abstract bool? Pickupable { get; set; }
    }
}