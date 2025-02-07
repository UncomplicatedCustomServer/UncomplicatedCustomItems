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

namespace UncomplicatedCustomItems
{
    public class Plugin : Plugin<Config>
    {
        public const bool IsPrerelease = true;
        public override string Name => "UncomplicatedCustomItems";

        public override string Prefix => "UncomplicatedCustomItems";

        public override string Author => "SpGerg & FoxWorn";

        public override Version RequiredExiledVersion { get; } = new(9, 5, 0);

        public override Version Version { get; } = new(3, 0, 0, 8);

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
            HttpManager = new("uci", uint.MaxValue);
            Handler = new();

            if (!File.Exists(Path.Combine(ConfigPath, "UncomplicatedCustomItems", ".nohttp")))
                HttpManager.Start();

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

            LogManager.History.Clear();

            Log.Info("===========================================");
            Log.Info(" Thanks for using UncomplicatedCustomItems");
            Log.Info("        by SpGerg & FoxWorn");
            Log.Info(" Updated to Exiled 9.5.0 by Mr. Baguetter");
            Log.Info("===========================================");
            Log.Info(">> Join our discord: https://discord.gg/5StRGu8EJV <<");

            Events.Internal.Player.Register();
            Events.Internal.Server.Register();

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

            HttpManager.Stop();

            _harmony.UnpatchAll();
            _harmony = null;

            Exiled.Events.Handlers.Player.Hurt -= Handler.OnHurt;
            Exiled.Events.Handlers.Player.TriggeringTesla -= Handler.OnTriggeringTesla;
            Exiled.Events.Handlers.Player.Shooting -= Handler.OnShooting;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= Handler.OnWaitingForPlayers;
            Exiled.Events.Handlers.Player.UsedItem -= Handler.OnItemUse;
            Exiled.Events.Handlers.Item.ChangingAttachments -= Handler.OnChangingAttachments;
            Exiled.Events.Handlers.Player.ActivatingWorkstation -= Handler.OnWorkstationActivation;

            Instance = null;
            base.OnDisabled();

        }
    }
}