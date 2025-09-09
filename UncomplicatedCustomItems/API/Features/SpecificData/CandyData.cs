using InventorySystem.Items.Usables.Scp330;
using UncomplicatedCustomItems.API.Features.CandySerialization;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    public class CandyData : Data, ICandyData
    {
        /// <summary>
        /// Set when the user gets the candy
        /// </summary>
        internal SerializedCandy SerializedCandy { get; set; }

        public virtual CandyKindID CandyType { get; set; }
        public virtual string EatingMessage { get; set; }
        public virtual float EatingMessageDuration { get; set; }
        public virtual bool DestroyOnUse { get; set; }
        public virtual float Chance { get; set; }
        public virtual bool ApplyEffects { get; set; }
        public virtual bool AllowSpawningAsItem { get; set; }
    }
}