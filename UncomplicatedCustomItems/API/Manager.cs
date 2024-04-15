using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.API
{
    public class Manager
    {
        /// <summary>
        /// Register a new <see cref="ICustomItem"/>
        /// </summary>
        /// <param name="CustomItem"></param>
        /// <returns>A <see cref="true"/> if the role has been registered, a <see cref="false"/> if not.</returns>
        public static bool Register(ICustomItem CustomItem)
        {
            return Helper.Helper.RegisterItem(CustomItem);
        }

        /// <summary>
        /// Unrgister a new <see cref="ICustomItem"/>
        /// </summary>
        /// <param name="CustomItem"></param>
        public static bool Unregister(ICustomItem CustomItem)
        {
            return Helper.Helper.UnregisterItem(CustomItem);
        }

        /// <summary>
        /// Try to get a <see cref="ICustomItem"/> by it's Id
        /// </summary>
        public static bool TryGet(uint Id, out ICustomItem Result)
        {
            if (Plugin.Items.ContainsKey(Id))
            {
                Result = Plugin.Items[Id];
                return true;
            }
            Result = null;
            return false;
        }

        /// <summary>
        /// Get a <see cref="ICustomItem"/> by it's Id
        /// </summary>
        /// <returns></returns>
        public static ICustomItem Get(uint Id)
        {
            return Plugin.Items[Id];
        }

        /// <summary>
        /// Is the <see cref="uint">Id</see> is a <see cref="ICustomItem"/>
        /// </summary>
        /// <returns></returns>
        public static bool IsCustomItem(uint Id)
        {
            return Plugin.Items.ContainsKey(Id);
        }
    }
}
