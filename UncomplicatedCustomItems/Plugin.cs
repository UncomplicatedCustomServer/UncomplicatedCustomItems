using Exiled.API.Features;
using HarmonyLib;
using System;
using System.IO;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.Elements;
using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems
{
    public class Plugin : Plugin<Config>
    {
        public static Plugin Instance { get; private set; }

        public override string Author => "SpGerg & FoxWorn";

        public override Version RequiredExiledVersion { get; } = new(9, 5, 0);

        public override Version Version { get; } = new(2, 2, 0);

        private Harmony _harmony;

        public override void OnEnabled()
        {
            Instance = this;

            _harmony = new Harmony($"com.ucs.uci-{DateTime.Now}");
            _harmony.PatchAll();

            if (!File.Exists(Path.Combine(ConfigPath, "UncomplicatedCustomRoles", ".nohttp")))
            {   
                Managers.HttpManager httpManager = new Managers.HttpManager("uci");
                httpManager.Start();
            }

            Log.Info("===========================================");
            Log.Info(" Thanks for using UncomplicatedCustomItems");
            Log.Info("        by SpGerg & FoxWorn");
            Log.Info(" Updated to Exiled 9.5.0 by Mr. Baguetter");
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

            Instance = null;

            base.OnDisabled();
        }
    }
}
