namespace UncomplicatedCustomItems.Interfaces
{
    public interface IResponse
    {
        public abstract string ConsoleMessage { get; set; }

        public abstract string BroadcastMessage { get; set; }

        public abstract ushort BroadcastDuration { get; set; }

        public abstract string HintMessage { get; set; }

        public abstract float HintDuration { get; set; }
    }
}
