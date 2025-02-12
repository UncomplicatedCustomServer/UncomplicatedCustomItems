using Exiled.API.Enums;
using Exiled.API.Features;
using System;
using HarmonyLib;
using System.IO;
using UncomplicatedCustomItems.Events;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.HarmonyElements.Patches;
using Handler = UncomplicatedCustomItems.Events.EventHandler;
using UnityEngine;
using System.Threading.Tasks;

namespace UncomplicatedCustomItems
{
    public class Plugin : Plugin<Config>
    {
        public const bool IsPrerelease = false;
        public override string Name => "UncomplicatedCustomItems";

        public override string Prefix => "UncomplicatedCustomItems";

        public override string Author => "SpGerg, FoxWorn & Mr. Baguetter";

        public override Version RequiredExiledVersion { get; } = new(9, 5, 0);

        public override Version Version { get; } = new(3, 0, 0);

        internal Handler Handler;

        public override PluginPriority Priority => PluginPriority.First;

        public static Plugin Instance { get; private set; }

        public Harmony _harmony;

        internal static HttpManager HttpManager;

        internal FileConfig FileConfig;

        public override void OnEnabled()
        {
            Instance = this;

            _harmony = new($"com.ucs.uci_exiled-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
            _harmony.PatchAll();

            FileConfig = new();
            HttpManager = new("uci");
            Handler = new();

            if (!File.Exists(Path.Combine(ConfigPath, "UncomplicatedCustomItems", ".nohttp")))

            if (IsPrerelease)
            {
                Exiled.Events.Handlers.Server.WaitingForPlayers += Handler.OnWaitingForPlayers;
            }

            Exiled.Events.Handlers.Player.Hurt += Handler.OnHurt;
            Exiled.Events.Handlers.Player.TriggeringTesla += Handler.OnTriggeringTesla;
            Exiled.Events.Handlers.Player.Shooting += Handler.OnShooting;
            Exiled.Events.Handlers.Player.UsedItem += Handler.OnItemUse;
            Exiled.Events.Handlers.Item.ChangingAttachments += Handler.OnChangingAttachments;
            Exiled.Events.Handlers.Player.ActivatingWorkstation += Handler.OnWorkstationActivation;
            Exiled.Events.Handlers.Player.DroppedItem += Handler.OnDrop;
            Exiled.Events.Handlers.Map.PickupDestroyed += Handler.OnPickup;

            LogManager.History.Clear();

            LogManager.Info("===========================================");
            LogManager.Info(" Thanks for using UncomplicatedCustomItems");
            LogManager.Info("    by SpGerg, FoxWorn & Mr. Baguetter");
            LogManager.Info("===========================================");
            LogManager.Info(">> Join our discord: https://discord.gg/5StRGu8EJV <<");

            Events.Internal.Player.Register();
            Events.Internal.Server.Register();

            Task.Run(delegate
            {
                if (HttpManager.LatestVersion.CompareTo(Version) > 0)
                    LogManager.Warn($"You are NOT using the latest version of UncomplicatedCustomItems!\nCurrent: v{Version} | Latest available: v{HttpManager.LatestVersion}\nDownload it from GitHub: https://github.com/UncomplicatedCustomServer/UncomplicatedCustomItems/releases/latest");

                VersionManager.Init();
            });

            FileConfig.Welcome(loadExamples:true);
            FileConfig.Welcome(Server.Port.ToString());
            FileConfig.LoadAll();
            FileConfig.LoadAll(Server.Port.ToString());

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Events.Internal.Player.Unregister();
            Events.Internal.Server.Unregister();

            HttpManager.UnregisterEvents();
            _harmony.UnpatchAll();
            _harmony = null;

            Exiled.Events.Handlers.Player.Hurt -= Handler.OnHurt;
            Exiled.Events.Handlers.Player.TriggeringTesla -= Handler.OnTriggeringTesla;
            Exiled.Events.Handlers.Player.Shooting -= Handler.OnShooting;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= Handler.OnWaitingForPlayers;
            Exiled.Events.Handlers.Player.UsedItem -= Handler.OnItemUse;
            Exiled.Events.Handlers.Item.ChangingAttachments -= Handler.OnChangingAttachments;
            Exiled.Events.Handlers.Player.ActivatingWorkstation -= Handler.OnWorkstationActivation;
            Exiled.Events.Handlers.Player.DroppedItem -= Handler.OnDrop;
            Exiled.Events.Handlers.Map.PickupDestroyed -= Handler.OnPickup;

            Instance = null;
            base.OnDisabled();

        }
    }
}