using InventorySystem.Items.Usables.Scp330;

namespace UncomplicatedCustomItems.API.Interfaces.SpecificData
{
    public interface ICandyData : IData
    {
        public CandyKindID CandyType { get; set; }
        public string EatingMessage { get; set; }
        public float EatingMessageDuration { get; set; }
        public bool DestroyOnUse { get; set; }
        public float Chance { get; set; }
        public bool ApplyEffects { get; set; }
        public bool AllowSpawningAsItem { get; set; }
    }
}