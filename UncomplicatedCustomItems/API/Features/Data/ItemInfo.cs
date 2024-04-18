using Exiled.API.Features.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncomplicatedCustomItems.API.Features.Data
{
    public class ItemInfo : ThingInfo
    {
        public string Response { get; set; }

        public string Command { get; set; }

        public override void Set(Item item)
        {
            return;
        }
    }
}
