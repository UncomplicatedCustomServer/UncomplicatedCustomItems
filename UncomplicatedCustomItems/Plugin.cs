using Exiled.API.Enums;
using Exiled.API.Features;
using System;
using HarmonyLib;
using System.IO;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.Elements;
using UncomplicatedCustomItems.Managers;

namespace UncomplicatedCustomItems
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "UncomplicatedCustomItems";

        public override string Prefix => "uci";

        public override string Author => "SpGerg & FoxWorn";

        public override Version RequiredExiledVersion { get; } = new(8, 2, 1);

        public override Version Version { get; } = new(2, 0, 1);

        public override PluginPriority Priority => PluginPriority.First;

        public static Plugin Instance { get; private set; }

        private Harmony _harmony;

        internal HttpManager HttpManager;

        public override void OnEnabled()
        {
            Instance = this;

            _harmony = new("com.ucs.uci");
            _harmony.PatchAll();

            if (!File.Exists(Path.Combine(ConfigPath, "UncomplicatedCustomRoles", ".nohttp")))
            {   
                HttpManager = new("uci", uint.MaxValue);
                HttpManager.Start();
            }

            LogManager.History.Clear();

            Log.Info("===========================================");
            Log.Info(" Thanks for using UncomplicatedCustomItems");
            Log.Info("        by SpGerg & FoxWorn");
            Log.Info("===========================================");
            Log.Info(">> Join our discord: https://discord.gg/5StRGu8EJV <<");

            foreach (YAMLCustomItem Item in Config.CustomItems)
            {
                Manager.Register(YAMLCaster.Converter(Item));
            }

            Events.Internal.Player.Register();
            Events.Internal.Server.Register();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Events.Internal.Player.Unregister();
            Events.Internal.Server.Unregister();

            HttpManager.Stop();

            _harmony.UnpatchAll();
            _harmony = null;

            Instance = null;

            base.OnDisabled();
        }
    }
}