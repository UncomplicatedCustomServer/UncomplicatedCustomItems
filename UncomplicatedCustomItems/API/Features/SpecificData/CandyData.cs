using InventorySystem.Items.Usables.Scp330;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    public class CandyData : Data, ICandyData
    {
        public virtual CandyKindID CandyType { get; set; }
        public virtual string EatingMessage { get; set; } = string.Empty;
        public virtual float EatingMessageDuration { get; set; }
        public virtual bool DestroyOnUse { get; set; }
        public virtual float Chance { get; set; }
        public virtual bool ApplyEffects { get; set; }
        public virtual bool AllowSpawningAsItem { get; set; }
    }
}