using System.Collections.Generic;
using UncomplicatedCustomItems.Interfaces.FlagSettings;

namespace UncomplicatedCustomItems.API.Features
{
    public class CantDropSettings : ICantDropSettings
    {
        public string? HintOrBroadcast {get; set;} = "Hint";
        public string? Message {get; set;} = "You cant drop %name%!";
        public ushort? Duration {get; set;} = 10;
    }
}