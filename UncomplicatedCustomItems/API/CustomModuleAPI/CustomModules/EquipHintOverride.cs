namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class EquipHintOverride : CustomModuleBase
    {
        public override string Name => "EquipHintOverride";

        public string Hint { get; set; } = string.Empty;
        public uint Duration { get; set; }
    }
}
