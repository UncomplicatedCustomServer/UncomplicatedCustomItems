using System;
using InventorySystem.Items.Usables.Scp330;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.API.Features.CandySerialization
{
#nullable enable
    public class SerializedCandy
    {
        public Guid Id { get; set; }
        public CandyKindID CandyType { get; set; }
        public long AddedTimestamp { get; set; }
        public ushort BagSerial { get; set; }
        public bool IsCustom { get; set; }
        public uint CustomItemId { get; set; }
    }
}