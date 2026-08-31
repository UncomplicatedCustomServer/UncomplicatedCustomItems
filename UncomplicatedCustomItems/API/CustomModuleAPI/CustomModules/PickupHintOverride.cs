namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class PickupHintOverride : CustomModuleBase
    {
        public override string Name => "PickupHintOverride";

        public string Hint { get; set; } = string.Empty;
        public uint Duration { get; set; }
    }
}
