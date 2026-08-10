using System.Text.Json.Serialization;

namespace UncomplicatedCustomItems.API.Features.Manager
{
    public class VersionInfoV2
    {
        [JsonPropertyName("codeName")]
        public string CodeName { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
        
        [JsonPropertyName("pluginHash")]
        public string Hash { get; set; } = string.Empty;

        [JsonPropertyName("recallReason")]
        public string RecallReason { get; set; } = string.Empty;

        [JsonPropertyName("recalled")]
        public bool Recalled { get; set; }

        [JsonPropertyName("preRelease")]
        public bool PreRelease { get; set; }

        [JsonPropertyName("forceDebug")]
        public bool ForceDebug { get; set; }
    }
}