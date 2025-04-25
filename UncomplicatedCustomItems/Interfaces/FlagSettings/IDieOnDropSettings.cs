using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncomplicatedCustomItems.Interfaces.FlagSettings
{
    internal interface IDieOnDropSettings
    {
        public abstract string? DeathMessage { get; set; }
        public abstract bool? Vaporize { get; set; }
    }
}
