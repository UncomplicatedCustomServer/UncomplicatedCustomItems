namespace UncomplicatedCustomItems.Interfaces
{
    public interface ISpawnItemWhenDetonatedSettings
    {
        public abstract ItemType ItemToSpawn { get; set; }
        public abstract float? TimeTillDespawn { get; set; }
    }
}