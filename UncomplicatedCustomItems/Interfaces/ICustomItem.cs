#nullable enable
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Interfaces;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Elements;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using UnityEngine;

namespace UncomplicatedCustomItems.Interfaces
{
    public interface ICustomItem : IData
    {
        public abstract uint Id { get; set; }

        public abstract string Name { get; set; }

        public abstract string Description { get; set; }

        public abstract float Weight { get; set; }

        public abstract ItemType Item { get; set; }

        public abstract Vector3 Scale { get; set; }

        public abstract CustomItemType CustomItemType { get; set; }

        public abstract IData CustomData { get; set; }

        public abstract SummonedCustomItem Summon(Player Player);

        public abstract SummonedCustomItem Summon(Pickup Pickup);

        public abstract SummonedCustomItem Summon(Vector3 Position, Quaternion Rotation);
    }
}