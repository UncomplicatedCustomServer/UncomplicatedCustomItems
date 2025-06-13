namespace UncomplicatedCustomItems.API.Interfaces.FlagSettings
{
    internal interface ICantDropSettings
    {
        public abstract string? HintOrBroadcast {get; set;}
        public abstract string? Message {get; set;}
        public abstract ushort? Duration {get; set;}
    }
}
