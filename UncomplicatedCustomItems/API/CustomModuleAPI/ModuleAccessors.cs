using System;
using System.Collections.Generic;

namespace UncomplicatedCustomItems.API.CustomModuleAPI
{
    public readonly struct ModuleAccessors
    {
        public readonly Dictionary<string, (Action<CustomModuleBase, object?> Set, Type Type)> Setters;
        public readonly Dictionary<string, Func<CustomModuleBase, object?>> Getters;
        
        public ModuleAccessors(Dictionary<string, (Action<CustomModuleBase, object?>, Type)> setters, Dictionary<string, Func<CustomModuleBase, object?>> getters)
        {
            Setters = setters;
            Getters = getters;
        }
    }
}