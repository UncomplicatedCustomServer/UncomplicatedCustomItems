using Exiled.API.Features.Items;

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
