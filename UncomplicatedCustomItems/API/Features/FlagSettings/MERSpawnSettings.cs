namespace UncomplicatedCustomItems.API.Features
{
    public class MERSpawnSettings
    {
        public bool LockerSpawning { get; set; }
        public bool ReplacePrimitive { get; set; }
        public string SchematicName { get; set; } = string.Empty;
        public string ObjectName { get; set; } = string.Empty;
    }
}