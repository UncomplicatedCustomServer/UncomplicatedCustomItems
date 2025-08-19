using HarmonyLib;
using LabApi.Features.Wrappers;
using LabApi.Loader;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.Events;
using UncomplicatedCustomItems.Integrations;
using UnityEngine;
using UserSettings.ServerSpecific;
using Handler = UncomplicatedCustomItems.Events.EventHandler;

// Events
using PlayerEvent = LabApi.Events.Handlers.PlayerEvents;
using ServerEvent = LabApi.Events.Handlers.ServerEvents;


namespace UncomplicatedCustomItems
{
    public class Plugin : Plugin<Config>
    {
        public bool IsPrerelease = false;
        public override string Name => "UncomplicatedCustomItems";
#if LABAPI
        public override string Description => "Enables server owners to design and manage CustomItems with ease, no coding required.";
#elif EXILED
        public override string Prefix => "UncomplicatedCustomItems";
#endif
        public override string Author => "SpGerg, FoxWorn & Mr. Baguetter";
#if LABAPI
        public override Version RequiredApiVersion { get; } = LabApi.Features.LabApiProperties.CurrentVersion;
#elif EXILED
        public override Version RequiredExiledVersion { get; } = new(9, 6, 1);
#endif
        public override Version Version { get; } = new(4, 0, 0);

        internal Handler Handler;

        public Assembly Assembly => Assembly.GetExecutingAssembly();
#if LABAPI
        public override LoadPriority Priority => LoadPriority.Highest;
#elif EXILED
        public override PluginPriority Priority => PluginPriority.First;
#endif
        public static Plugin Instance { get; private set; }

        internal Arguments arguments;

        internal Harmony _harmony;

        internal static HttpManager HttpManager;

        internal FileConfig FileConfig;
        internal ServerSpecificSettingBase[] _playerSettings;
        internal ServerSpecificSettingBase[] _ToolGunSettings;
        internal ServerSpecificSettingBase[] _DebugSettings;
        internal List<ServerSpecificSettingBase> _settings;
        internal bool DebugMode;

        public override void Enable()
        {
            Instance = this;

            FileConfig = new();
            HttpManager = new("uci");
            Handler = new();

            if (!File.Exists(Path.Combine(ConfigurationLoader.GetConfigPath(Instance, "UncomplicatedCustomItems"), "UncomplicatedCustomItems", ".nohttp")))

            PlayerHandler.Register();
            ServerHandler.Register();
            ScpHandler.Register();

            ServerEvent.WaitingForPlayers += OnFinishedLoadingPlugins;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += Handler.OnValueReceived;

            // Debugging Events
            PlayerEvent.DroppingItem += Handler.OnDrop;
            PlayerEvent.PickedUpItem += Handler.OnDebuggingPickup;
            PlayerEvent.UsingItem += Handler.OnUse;
            PlayerEvent.ReloadingWeapon += Handler.OnReloading;
            PlayerEvent.ShootingWeapon += Handler.OnShooting;
            PlayerEvent.ThrewProjectile += Handler.OnThrown;

            Arguments.Initialize();
            Arguments.Register();

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
                new SSKeybindSetting(20, "Trigger CustomItem", KeyCode.K, hint: "When pressed this will trigger the CustomItem your holding", allowSpectatorTrigger: false)
            ];
            _DebugSettings =
            [
                new SSGroupHeader("UCI Debug Settings", hint: "If you can see this and are not a developer please notify the server staff or developers ASAP"),

            ];
            _settings =
            [
                new SSGroupHeader("UCI ToolGun Settings", hint: "If multiple are created any will work"),
                new SSPlaintextSetting(21, "Primitive Color", placeholder: "255, 0, 0, -1", hint: "The color of the primitives spawned by the ToolGun"),
                new SSTwoButtonsSetting(22, "Deletion Mode", "ADS", "FlashLight Toggle", hint: "Sets the deletion mode of the ToolGun"),
                new SSTwoButtonsSetting(23, "Delete Primitives when unequipped?", "Yes", "No"),

                new SSGroupHeader("UCI Debug Settings", hint: "If you can see this and are not a developer please notify the server staff or developers ASAP"),
                //new SSButton(24, "Give ToolGun", "Give"),
                new SSButton(28, "Dev Role", "Give"),
                new SSButton(30, "Manager Role", "Give"),

                new SSGroupHeader("CustomItem Settings"),
                new SSKeybindSetting(20, "Trigger CustomItem", KeyCode.K, hint: "When pressed this will trigger the CustomItem your holding", allowSpectatorTrigger: false)
            ];

            ServerSpecificSettingsSync.DefinedSettings = _playerSettings;
            ServerSpecificSettingsSync.SendToAll();

            LogManager.History.Clear();

            LogManager.Info("===========================================");
            LogManager.Info("Thanks for using UncomplicatedCustomItems");
            LogManager.Info($"    by {Author}");
            LogManager.Info("===========================================");
            LogManager.Info(">> Join our discord: https://discord.gg/5StRGu8EJV <<");

            if (IsPrerelease)
            {
                if (!Instance.Config.Debug)
                {
                    LogManager.Info("Debug logs have been activated!");
                    Instance.Config.Debug = true;
                    DebugMode = true;
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

            _harmony = new($"com.ucs.uci_labapi-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
            _harmony.PatchAll();
            ECRIntegration.Initialize(_harmony);
        }

        public override void Disable()
        {
            ECRIntegration.Cleanup();
            Events.Internal.Player.Unregister();
            Events.Internal.Server.Unregister();

            _playerSettings = null;

            HttpManager.UnregisterEvents();
            _harmony.UnpatchAll();
            _harmony = null;

            PlayerHandler.Unregister();
            ServerHandler.Unregister();
            ScpHandler.Unregister();

            ServerEvent.WaitingForPlayers -= OnFinishedLoadingPlugins;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived -= Handler.OnValueReceived;

            // Debugging Events
            PlayerEvent.DroppingItem -= Handler.OnDrop;
            PlayerEvent.PickedUpItem -= Handler.OnDebuggingPickup;
            PlayerEvent.UsingItem -= Handler.OnUse;
            PlayerEvent.ReloadingWeapon -= Handler.OnReloading;
            PlayerEvent.ShootingWeapon -= Handler.OnShooting;
            PlayerEvent.ThrewProjectile -= Handler.OnThrown;


            Arguments.Cleanup();

            Instance = null;
            Handler = null;
        }
        public void OnFinishedLoadingPlugins()
        {
            ImportManager.Init();
            _ = UpdateChecker.CheckForUpdatesAsync();
        }
    }
}