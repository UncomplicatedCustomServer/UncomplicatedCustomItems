namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class WorkstationBanHintOverride : CustomModuleBase
    {
        public override string Name => "WorkstationBanHintOverride";

        public string HintOverride { get; set; } = string.Empty;
        public float DurationOverride { get; set; }
    }
}