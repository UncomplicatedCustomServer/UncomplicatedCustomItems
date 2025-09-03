using InventorySystem.Items.Usables.Scp330;
using UncomplicatedCustomItems.API.Features.CandySerialization;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using YamlDotNet.Serialization;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    public class CandyData : Data, ICandyData
    {
        /// <summary>
        /// Set when the user gets the candy
        /// </summary>
        [YamlIgnore]
        public SerializedCandy SerializedCandy { get; internal set; }

        public virtual CandyKindID CandyType { get; set; }
        public virtual string EatingMessage { get; set; }
        public virtual bool DestroyOnUse { get; set; }
        public virtual int Chance { get; set; }
        public virtual bool ApplyEffects { get; set; } = true;
    }
}