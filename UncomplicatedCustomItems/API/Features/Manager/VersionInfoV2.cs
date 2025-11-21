namespace UncomplicatedCustomItems.API.Features.Helper
{
    public class VersionInfoV2
    {
        public string CodeName { get; }
        public string Version { get; }
        public bool Recalled { get; }
        public string RecallReason { get; }
        public bool PreRelease { get; }
        public bool ForceDebug { get; }

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