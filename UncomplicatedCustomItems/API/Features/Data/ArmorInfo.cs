using Exiled.API.Features.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncomplicatedCustomItems.API.Features.Data
{
    public class ArmorInfo : ThingInfo
    {
        public int HeadProtection { get; set; }

        public int BodyProtection { get; set; }

        public override void Set(Item item)
        {
            if (item is not Armor armor)
            {
                return;
            }

            armor.HelmetEfficacy = HeadProtection;
            armor.VestEfficacy = BodyProtection;
        }
    }
}
