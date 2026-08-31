using InventorySystem.Items.Firearms.Attachments;
using UncomplicatedCustomItems.API.Enums;

namespace UncomplicatedCustomItems.API.YamlObjects
{
    public class ParameterObject
    {
        public AttachmentParam Parameter { get; set; }
        public MathType Type { get; set; }
        public float Value { get; set; }
    }
}