using InventorySystem.Items.Usables.Scp330;
using UncomplicatedCustomItems.API.Features.CandySerialization;

namespace UncomplicatedCustomItems.API.Interfaces.SpecificData
{
    public interface ICandyData : IData
    {
        public CandyKindID CandyType { get; set; }
        public string EatingMessage { get; set; }
        public bool DestroyOnUse { get; set; }
        public int Chance { get; set; }
        public bool ApplyEffects { get; set; }
    }
}