using Newtonsoft.Json;

namespace UncomplicatedCustomItems.API.Features.Helper
{
    public class VersionInfoV2
    {
        [JsonProperty("codeName")]
        public string CodeName { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("recalled")]
        public bool Recalled { get; set; }
        [JsonProperty("recallReason")]
        public string RecallReason { get; set; }
        [JsonProperty("preRelease")]
        public bool PreRelease { get; set; }
        [JsonProperty("forceDebug")]
        public bool ForceDebug { get; set; }
        
        public VersionInfoV2(string name, string version, bool recalled, string recallReason, bool preRelease, bool forceDebug)
        {
            CodeName = name;
            Version = version;
            Recalled = recalled;
            RecallReason = recallReason;
            PreRelease = preRelease;
            ForceDebug = forceDebug;
        }
    }
}