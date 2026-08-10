namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class MERSpawn : CustomModuleBase
    {
        public override string Name => "MERSpawn";

        public bool LockerSpawning { get; set; }
        public bool ReplacePrimitive { get; set; }
        public string SchematicName { get; set; } = string.Empty;
        public string ObjectName { get; set; } = string.Empty;
    }
}
