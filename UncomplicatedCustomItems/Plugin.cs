using Exiled.API.Enums;
using Exiled.API.Features;
using System;
using HarmonyLib;
using System.IO;
using UncomplicatedCustomItems.API.Features.Helper;
using System.Threading.Tasks;
using UncomplicatedCustomItems.Integration;
using System.Collections.Generic;
using UnityEngine;
using UserSettings.ServerSpecific;
using UncomplicatedCustomItems.Events;
using PlayerEvent = Exiled.Events.Handlers.Player;
using ServerEvent = Exiled.Events.Handlers.Server;

namespace UncomplicatedCustomItems
{
    public class Plugin : Plugin<Config>
    {
        public bool IsPrerelease = false;
        public override string Name => "UncomplicatedCustomItems";

        public override string Prefix => "UncomplicatedCustomItems";

        public override string Author => "SpGerg, FoxWorn & Mr. Baguetter";

        public override Version RequiredExiledVersion { get; } = new(9, 6, 0);

        public override Version Version { get; } = new(3, 5, 3);

        internal Events.EventHandler Handler;

        public override PluginPriority Priority => PluginPriority.First;

        public static Plugin Instance { get; private set; }

        internal Harmony _harmony;

        internal static HttpManager HttpManager;

        internal FileConfig FileConfig;

        internal ServerSpecificSettingBase[] _playerSettings;
        internal ServerSpecificSettingBase[] _ToolGunSettings;
        internal ServerSpecificSettingBase[] _DebugSettings;
        internal List<ServerSpecificSettingBase> _settings;

        public override void OnEnabled()
        {
            Instance = this;

            FileConfig = new();
            HttpManager = new("uci");
            Handler = new();

            if (!File.Exists(Path.Combine(ConfigPath, "UncomplicatedCustomItems", ".nohttp")))

            SCPHandler.Register();
            PlayerHandler.Register();
            ServerHandler.Register();
            MapHandler.Register();
            ItemHandler.Register();
            
            ServerEvent.WaitingForPlayers += OnFinishedLoadingPlugins;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += Handler.OnValueReceived;

            // Debugging Events
            PlayerEvent.DroppingItem += Handler.Ondrop;
            PlayerEvent.ItemAdded += Handler.OnDebuggingpickup;
            PlayerEvent.UsingItem += Handler.Onuse;
            PlayerEvent.ReloadingWeapon += Handler.Onreloading;
            PlayerEvent.Shooting += Handler.Onshooting;
            PlayerEvent.ThrownProjectile += Handler.Onthrown;

            CustomItemEventHandler.Init<Examples.Events>();

            _ToolGunSettings =
            [
                new SSGroupHeader("UCI ToolGun Settings", hint: "If multiple are created any will work"),
                new SSPlaintextSetting(21, "Primitive Color", placeholder: "255, 0, 0, -1", hint: "The color of the primitives spawned by the ToolGun"),
                new SSTwoButtonsSetting(22, "Deletion Mode", "ADS", "FlashLight Toggle", hint: "Sets the deletion mode of the ToolGun"),
                new SSTwoButtonsSetting(23, "Delete Primitives when unequipped?", "Yes", "No")
            ];
            _playerSettings =
            [
                new SSGroupHeader("CustomItem Settings"),
                new SSKeybindSetting(20, "Trigger CustomItem", KeyCode.K, hint: "When pressed this will trigger the CustomItem your holding")
            ];
            if (IsPrerelease)
                _DebugSettings =
                [
                    new SSGroupHeader("UCI Debug Settings", hint: "If you can see this and are not a developer please notify the server staff or developers ASAP"),
                    new SSButton(24, "Give ToolGun", "Give"),
                    new SSButton(28, "Dev Role", "Give"),
                    new SSButton(30, "Manager Role", "Give"),
                    new SSTextArea(29, "Default Message")
                ];
            _settings =
            [
                new SSGroupHeader("UCI ToolGun Settings", hint: "If multiple are created any will work"),
                new SSPlaintextSetting(21, "Primitive Color", placeholder: "255, 0, 0, -1", hint: "The color of the primitives spawned by the ToolGun"),
                new SSTwoButtonsSetting(22, "Deletion Mode", "ADS", "FlashLight Toggle", hint: "Sets the deletion mode of the ToolGun"),
                new SSTwoButtonsSetting(23, "Delete Primitives when unequipped?", "Yes", "No"),

                new SSGroupHeader("UCI Debug Settings", hint: "If you can see this and are not a developer please notify the server staff or developers ASAP"),
                new SSButton(24, "Give ToolGun", "Give"),
                new SSButton(28, "Dev Role", "Give"),
                new SSButton(30, "Manager Role", "Give"),
                new SSTextArea(29, "Default Message"),

                new SSGroupHeader("CustomItem Settings"),
                new SSKeybindSetting(20, "Trigger CustomItem", KeyCode.K, hint: "When pressed this will trigger the CustomItem your holding")
            ];

            ServerSpecificSettingsSync.DefinedSettings = _settings.ToArray();
            ServerSpecificSettingsSync.SendToAll();

            LogManager.History.Clear();

            LogManager.Info("===========================================");
            LogManager.Info(" Thanks for using UncomplicatedCustomItems");
            LogManager.Info($"    by {Author}");
            LogManager.Info("===========================================");
            LogManager.Info(">> Join our discord: https://discord.gg/5StRGu8EJV <<");

            if (IsPrerelease)
            {
                if (!Log.DebugEnabled.Contains(Instance.Assembly))
                {
                    LogManager.Info("Debug logs have been activated!");
                    Instance.Config.Debug = true;
                    Log.DebugEnabled.Add(Instance.Assembly);
                }
            }

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

            if (IsPrerelease)
            {
                Harmony.DEBUG = true;
            }

            _harmony = new($"com.ucs.uci_exiled-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
            _harmony.PatchAll();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Events.Internal.Player.Unregister();
            Events.Internal.Server.Unregister();

            HttpManager.UnregisterEvents();
            _harmony.UnpatchAll();
            _harmony = null;

            SCPHandler.Unregister();
            PlayerHandler.Unregister();
            ServerHandler.Unregister();
            MapHandler.Unregister();
            ItemHandler.Unregister();

            // Debugging Events
            PlayerEvent.DroppingItem -= Handler.Ondrop;
            PlayerEvent.ItemAdded -= Handler.OnDebuggingpickup;
            PlayerEvent.UsingItem -= Handler.Onuse;
            PlayerEvent.ReloadingWeapon -= Handler.Onreloading;
            PlayerEvent.Shooting -= Handler.Onshooting;
            PlayerEvent.ThrownProjectile -= Handler.Onthrown;

            CustomItemEventHandler.Dispose();

            Instance = null;
            Handler = null;
            base.OnDisabled();

        }
        public void OnFinishedLoadingPlugins()
        {
            ImportManager.Init();
            CommonUtilitiesPatch.Initialize();
            Server.ExecuteCommand("uciupdatecheck");
        }
    }
}