using Exiled.API.Features;
using System;
using UncomplicatedCustomItems.API;

namespace UncomplicatedCustomItems
{
    public class Plugin : Plugin<Config>
    {
        public static Plugin Instance { get; private set; }

        public override string Author => "SpGerg & FoxWorn";

        public override Version RequiredExiledVersion { get; } = new(8, 8, 0);

        public override Version Version { get; } = new(1, 5, 0);

        public override void OnEnabled()
        {
            Instance = this;

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

            Instance = null;

            base.OnDisabled();
        }
    }
}
