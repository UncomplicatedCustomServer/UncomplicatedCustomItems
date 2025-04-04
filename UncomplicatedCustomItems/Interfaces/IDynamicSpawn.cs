using Exiled.API.Enums;

namespace UncomplicatedCustomItems.Interfaces
{
    public interface IDynamicSpawn
    {
        public abstract RoomType Room { get; set; }
        public abstract int Chance { get; set; }
    }
}
