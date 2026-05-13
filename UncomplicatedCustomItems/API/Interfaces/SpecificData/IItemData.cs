using System.Collections.Generic;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.API.Interfaces.SpecificData
{
    /// <summary>
    /// The interface associated with <see cref="CustomItemType.Item"/>
    /// </summary>
    public interface IItemData : IData
    {
        public List<ItemDataList> Data { get; set; }
    }
}
