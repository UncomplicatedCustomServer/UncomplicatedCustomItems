using Exiled.API.Features;
using UncomplicatedCustomItems.API;

namespace UncomplicatedCustomItems
{
    public class Plugin : Plugin<Config>
    {
        public static Plugin Instance { get; private set; }

        public static UncomplicatedCustomItemsAPI API { get; private set; }

        public override void OnEnabled()
        {
            Instance = this;
            API = new UncomplicatedCustomItemsAPI();

            Events.Internal.Player.Register();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Events.Internal.Player.Unregister();

            base.OnDisabled();
        }
    }
}
