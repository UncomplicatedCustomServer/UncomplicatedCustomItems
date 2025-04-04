using Exiled.API.Enums;
using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.API.Features
{
    public class DynamicSpawn : IDynamicSpawn
    {
        public RoomType Room { get; set; } = RoomType.Lcz330;
        public int Chance { get; set; } = 30;
    }
}
