using Exiled.API.Features.Items;

namespace UncomplicatedCustomItems.API.Extensions
{
    public static class ItemExtensions
    {
        /// <summary>
        /// Is custom item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsCustomItem(this Item item) => Plugin.API.IsCustomItem(item);
    }
}
