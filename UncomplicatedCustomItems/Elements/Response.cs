using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.Elements
{
    public class Response : IResponse
    {
        public string ConsoleMessage { get; set; } = "An example response";

        public string BroadcastMessage { get; set; } = "Yamato is rainbow ♥";

        public ushort BroadcastDuration { get; set; } = 5;

        public string HintMessage { get; set; } = "UWU";

        public float HintDuration { get; set; } = 3f;
    }
}
