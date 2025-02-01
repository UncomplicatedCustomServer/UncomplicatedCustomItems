using Exiled.API.Features;
using HarmonyLib;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.Elements;
using UncomplicatedCustomItems.Interfaces;
using UncomplicatedCustomItems.Manager;

namespace UncomplicatedCustomItems
{
    public class Plugin : Plugin<Config>
    {
        public static Plugin Instance { get; private set; }

        public override string Author => "SpGerg & FoxWorn";

        public override Version RequiredExiledVersion { get; } = new(9, 5, 0);

        public override Version Version { get; } = new(2, 1, 1);

        internal static HttpManager HttpManager;

        internal static FileConfigs FileConfigs;

        private Harmony _harmony;

        public override void OnEnabled()
        {

            FileConfigs = new();
            HttpManager = new("uci");
            Instance = this;

            Utilities.List.Clear();

            _harmony = new Harmony($"com.ucs.uci-{DateTime.Now}");
            _harmony.PatchAll();

            Log.Info("===========================================");
            Log.Info(" Thanks for using UncomplicatedCustomItems");
            Log.Info("        by SpGerg & FoxWorn");
            Log.Info(" Updated to Exiled 9.5.0 by Mr. Baguetter");
            Log.Info("===========================================");
            Log.Info(">> Join our discord: https://discord.gg/5StRGu8EJV <<");

            Task.Run(delegate
            {
                if (HttpManager.LatestVersion.CompareTo(Version) > 0)
                    LogManager.Warn($"You are NOT using the latest version of UncomplicatedCustomItems!\nCurrent: v{Version} | Latest available: v{HttpManager.LatestVersion}\nDownload it from GitHub: https://github.com/UncomplicatedCustomServer/UncomplicatedCustomItems/releases/latest");

                VersionManager.Init();
            });

            FileConfigs.Welcome();
            FileConfigs.Welcome(Server.Port.ToString());
            FileConfigs.LoadAll();
            FileConfigs.LoadAll(Server.Port.ToString());

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
