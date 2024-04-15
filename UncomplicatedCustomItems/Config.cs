using Exiled.API.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using UncomplicatedCustomItems.Elements;

namespace UncomplicatedCustomItems
{
    public class Config : IConfig
    {
        [Description("Is enabled or not")]
        public bool IsEnabled { get; set; }

        [Description("Is debug or not")]
        public bool Debug { get; set; }

        [Description("List of custom items")]
        public List<CustomItem> CustomItems { get; set; } = new()
        {
            new()
        };
    }
}
