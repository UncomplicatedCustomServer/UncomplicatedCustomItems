using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.Elements.SpecificData
{
    public class ArmorData : Data, IArmorData
    {
        /// <summary>
        /// The armor's Head Protection value
        /// </summary>
        public int HeadProtection { get; set; } = 2;

        /// <summary>
        /// The armor's Body Protection value
        /// </summary>
        public int BodyProtection { get; set; } = 3;

        /// <summary>
        /// Do remove the ammo in excess when drop?
        /// </summary>
        public bool RemoveExcessOnDrop { get; set; } = true;

        /// <summary>
        /// The stamina that this armor drains
        /// </summary>
        public float StaminaUseMultiplier { get; set; } = 2f;
    }
}
