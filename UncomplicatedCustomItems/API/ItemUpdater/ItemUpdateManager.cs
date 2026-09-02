using System.Threading.Tasks;

namespace UncomplicatedCustomItems.API.ItemUpdater
{
    public class ItemUpdateManager
    {
        public static int UpdatedCount { get; set; }

        public static bool TryUpdate(string path)
        {
            bool updated = CustomModuleUpdater.TryUpdateCustomModules(path, out _);
            if (updated)
                UpdatedCount++;

            return updated;
        }

        public static async Task<bool> TryUpdateAsync(string path)
        {
            (bool Updated, _) = await CustomModuleUpdater.TryUpdateCustomModulesAsync(path).ConfigureAwait(false);
            if (Updated)
                UpdatedCount++;

            return Updated;
        }
    }
}