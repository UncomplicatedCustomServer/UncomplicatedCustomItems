using System.Text.Json.Serialization;

namespace UncomplicatedCustomItems.API.Features.Helper
{
    public class VersionInfoV2
    {
        [JsonPropertyName("codeName")]
        public string CodeName { get; set; }
        [JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonPropertyName("recalled")]
        public bool Recalled { get; set; }
        [JsonPropertyName("recallReason")]
        public string RecallReason { get; set; }
        [JsonPropertyName("preRelease")]
        public bool PreRelease { get; set; }
        [JsonPropertyName("forceDebug")]
        public bool ForceDebug { get; set; }
        
        [JsonConstructor]
        public VersionInfoV2(string codeName, string version, bool recalled, string recallReason, bool preRelease, bool forceDebug)
        {
            CodeName = codeName;
            Version = version;
            Recalled = recalled;
            RecallReason = recallReason;
            PreRelease = preRelease;
            ForceDebug = forceDebug;
        }
    }
}