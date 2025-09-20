using System;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.API.Attributes
{
    /// <summary>
    /// The <see cref="Attribute"/> used to register external <see cref="CustomItem"/>s
    /// Applied to the class of the <see cref="CustomItem"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PluginCustomItem : Attribute { }
}
