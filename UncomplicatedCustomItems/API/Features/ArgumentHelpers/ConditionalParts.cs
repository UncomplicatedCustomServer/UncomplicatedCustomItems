namespace UncomplicatedCustomItems.API.Features.ArgumentHelpers
{
    internal class ConditionalParts
    {
        public bool IsUnless { get; set; }
        public string Condition { get; set; } = string.Empty;
        public string[] ThenActions { get; set; } = [];
        public string[]? ElseActions { get; set; }
    }
}