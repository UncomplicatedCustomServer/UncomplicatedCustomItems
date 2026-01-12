using System;
using System.IO;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.API.ItemUpdater
{
    public class ItemUpdateManager
    {
        public static int UpdatedCount { get; set; }

        public static bool TryUpdate(string path)
        {
            bool updated;
            updated = CustomModuleUpdater.TryUpdateCustomModules(path, out _);
            return updated;
        }
    }
}