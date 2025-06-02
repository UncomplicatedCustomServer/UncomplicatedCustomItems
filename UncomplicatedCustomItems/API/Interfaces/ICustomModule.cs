using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.API.Interfaces
{
    public interface ICustomModule
    {
        public abstract SummonedCustomItem Instance { get; }

        public abstract string Name { get; }

        public abstract void Execute();
    }
}