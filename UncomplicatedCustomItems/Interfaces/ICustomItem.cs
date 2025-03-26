using UncomplicatedCustomItems.Interfaces.SpecificData;
using UnityEngine;
using UncomplicatedCustomItems.Enums;

namespace UncomplicatedCustomItems.Interfaces
{
#nullable enable
    public interface ICustomItem
    {
        public abstract uint Id { get; set; }

        public abstract string Name { get; set; }

        public abstract string Description { get; set; }

        public abstract string BadgeName { get; set; }

        public abstract string BadgeColor { get; set; }

        public abstract float Weight { get; set; }

        public abstract bool Reusable { get; set; }

        public abstract ItemType Item { get; set; }

        public abstract Vector3 Scale { get; set; }
        
        public abstract ISpawn Spawn { get; set; }

        public abstract CustomFlags? CustomFlags { get; set; }
        
        public abstract IFlagSettings FlagSettings { get; set; }

        public abstract CustomItemType CustomItemType { get; set; }

        public abstract IData CustomData { get; set; }
    }
}