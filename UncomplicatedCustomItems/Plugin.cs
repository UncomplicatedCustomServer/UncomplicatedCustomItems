using Exiled.API.Features;
using System;
using UncomplicatedCustomItems.API;

namespace UncomplicatedCustomItems
{
    public class Plugin : Plugin<Config>
    {
        public static Plugin Instance { get; private set; }

        public static UncomplicatedCustomItemsAPI API { get; private set; }

        public override string Author => "SpGergo & FoxWorno";

        public override Version RequiredExiledVersion { get; } = new(8, 8, 0);

        public override Version Version { get; } = new(1, 3, 1);

        public override void OnEnabled()
        {
            Instance = this;
            API = new UncomplicatedCustomItemsAPI();

            Log.Info("===========================================");
            Log.Info(" Thanks for using UncomplicatedCustomItems");
            Log.Info("        by SpGerg & FoxWorn");
            Log.Info("===========================================");
            Log.Info(">> Join our discord: https://discord.gg/5StRGu8EJV <<");

            Events.Internal.Player.Register();
            Events.Internal.Server.Register();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Events.Internal.Player.Unregister();
            Events.Internal.Server.Unregister();

            base.OnDisabled();
        }
    }
}
