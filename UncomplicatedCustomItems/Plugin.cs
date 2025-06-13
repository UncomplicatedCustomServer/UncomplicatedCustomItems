using System;
using HarmonyLib;
using System.IO;
using UncomplicatedCustomItems.API.Features.Helper;
using System.Threading.Tasks;
using Handler = UncomplicatedCustomItems.Events.EventHandler;
using UnityEngine;
using UserSettings.ServerSpecific;
using UncomplicatedCustomItems.Manager;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using LabApi.Features.Wrappers;
using LabApi.Loader;
using System.Collections.Generic;
using System.Reflection;

// Events
using PlayerEvent = LabApi.Events.Handlers.PlayerEvents;
using ServerEvent = LabApi.Events.Handlers.ServerEvents;
using MapEvent = LabApi.Events.Handlers.ServerEvents;
using Scp914Event = LabApi.Events.Handlers.Scp914Events;
using UncomplicatedCustomItems.Events;


namespace UncomplicatedCustomItems
{
    public class Plugin : Plugin<Config>
    {
        public bool IsPrerelease = false;
        public override string Name => "UncomplicatedCustomItems";

        public override string Description => "Allows server owners to create CustomItems without the hassel of coding";

        public override string Author => "SpGerg, FoxWorn & Mr. Baguetter";

        public override Version RequiredApiVersion { get; } = new(1, 0, 2);

        public override Version Version { get; } = new(3, 5, 3);

        internal Handler Handler;

        public Assembly Assembly => Assembly.GetExecutingAssembly();

        public override LoadPriority Priority => LoadPriority.Highest;

        public static Plugin Instance { get; private set; }

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

            PlayerEvent.Hurt += Handler.OnHurt;
            PlayerEvent.TriggeringTesla += Handler.OnTriggeringTesla;
            PlayerEvent.ShootingWeapon += Handler.OnShooting;
            PlayerEvent.UsedItem += Handler.OnItemUse;
            PlayerEvent.DroppedItem += Handler.OnDrop;
            MapEvent.PickupDestroyed += Handler.OnPickup;
            PlayerEvent.ShotWeapon += Handler.OnShot;
            PlayerEvent.UpdatingEffect += Handler.Receivingeffect;
            PlayerEvent.ThrewProjectile += Handler.ThrownProjectile;
            MapEvent.ProjectileExploding += Handler.GrenadeExploding;
            ServerEvent.WaitingForPlayers += OnFinishedLoadingPlugins;
            PlayerEvent.Dying += Handler.OnDying;
            PlayerEvent.ChangedItem += Handler.OnChangedItem;
            PlayerEvent.DroppingItem += Handler.OnDropping;
            PlayerEvent.Hurting += Handler.OnHurting;
            PlayerEvent.InteractedDoor += Handler.OnDoorInteracting;
            PlayerEvent.UnlockingGenerator += Handler.OnGeneratorUnlock;
            PlayerEvent.InteractingLocker += Handler.OnLockerInteracting;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += Handler.OnValueReceived;
            PlayerEvent.Joined += Handler.OnVerified;
            PlayerEvent.PickedUpItem += Handler.Onpickup;
            PlayerEvent.Spawned += Handler.OnSpawned;
            PlayerEvent.Left += Handler.OnLeft;
            PlayerEvent.FlippedCoin += Handler.FlippedCoin;
            PlayerEvent.ToggledFlashlight += Handler.ToggledFlashlight;
            Scp914Event.ProcessingPickup += Handler.OnPickupUpgrade;
            Scp914Event.ProcessingInventoryItem += Handler.OnItemUpgrade;
            ServerEvent.RoundEnding += Handler.OnRoundEnd;
            MapEvent.PickupCreated += Handler.OnPickupCreation;
            PlayerEvent.ToggledWeaponFlashlight += Handler.WeaponFlashLight;

            // Debugging Events
            PlayerEvent.DroppingItem += Handler.Ondrop;
            PlayerEvent.PickedUpItem += Handler.OnDebuggingpickup;
            PlayerEvent.UsingItem += Handler.Onuse;
            PlayerEvent.ReloadingWeapon += Handler.Onreloading;
            PlayerEvent.ShootingWeapon += Handler.Onshooting;
            PlayerEvent.ThrewProjectile += Handler.Onthrown;

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
                new SSKeybindSetting(20, "Trigger CustomItem", KeyCode.K, hint: "When pressed this will trigger the CustomItem your holding", allowSpectatorTrigger: false)
            ];
            _DebugSettings =
            [
                new SSGroupHeader("UCI Debug Settings", hint: "If you can see this and are not a developer please notify the server staff or developers ASAP"),
                //new SSButton(24, "Give ToolGun", "Give"),
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
                //new SSButton(24, "Give ToolGun", "Give"),
                new SSButton(28, "Dev Role", "Give"),
                new SSButton(30, "Manager Role", "Give"),
                new SSTextArea(29, "Default Message"),

                new SSGroupHeader("CustomItem Settings"),
                new SSKeybindSetting(20, "Trigger CustomItem", KeyCode.K, hint: "When pressed this will trigger the CustomItem your holding", allowSpectatorTrigger: false)
            ];

            ServerSpecificSettingsSync.DefinedSettings = _settings.ToArray();
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
        }

        public override void Disable()
        {
            Events.Internal.Player.Unregister();
            Events.Internal.Server.Unregister();

            _playerSettings = null;

            HttpManager.UnregisterEvents();
            _harmony.UnpatchAll();
            _harmony = null;

            PlayerEvent.Hurt -= Handler.OnHurt;
            PlayerEvent.TriggeringTesla -= Handler.OnTriggeringTesla;
            PlayerEvent.ShootingWeapon -= Handler.OnShooting;
            PlayerEvent.UsedItem -= Handler.OnItemUse;
            PlayerEvent.DroppedItem -= Handler.OnDrop;
            PlayerEvent.ShotWeapon -= Handler.OnShot;
            PlayerEvent.UpdatingEffect -= Handler.Receivingeffect;
            PlayerEvent.ThrewProjectile -= Handler.ThrownProjectile;
            MapEvent.ProjectileExploding -= Handler.GrenadeExploding;
            ServerEvent.WaitingForPlayers -= OnFinishedLoadingPlugins;
            MapEvent.PickupDestroyed -= Handler.OnPickup;
            PlayerEvent.Dying -= Handler.OnDying;
            PlayerEvent.ChangedItem -= Handler.OnChangedItem;
            PlayerEvent.DroppingItem -= Handler.OnDropping;
            PlayerEvent.Hurting -= Handler.OnHurting;
            PlayerEvent.InteractedDoor -= Handler.OnDoorInteracting;
            PlayerEvent.UnlockingGenerator -= Handler.OnGeneratorUnlock;
            PlayerEvent.InteractingLocker -= Handler.OnLockerInteracting;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived -= Handler.OnValueReceived;
            PlayerEvent.Joined -= Handler.OnVerified;
            PlayerEvent.PickedUpItem -= Handler.Onpickup;
            PlayerEvent.Spawned -= Handler.OnSpawned;
            PlayerEvent.Left -= Handler.OnLeft;
            PlayerEvent.FlippedCoin -= Handler.FlippedCoin;
            PlayerEvent.ToggledFlashlight -= Handler.ToggledFlashlight;
            Scp914Event.ProcessingPickup -= Handler.OnPickupUpgrade;
            Scp914Event.ProcessingInventoryItem -= Handler.OnItemUpgrade;
            ServerEvent.RoundEnding -= Handler.OnRoundEnd;
            MapEvent.PickupCreated -= Handler.OnPickupCreation;
            PlayerEvent.ToggledWeaponFlashlight -= Handler.WeaponFlashLight;

            // Debugging Events
            PlayerEvent.DroppingItem -= Handler.Ondrop;
            PlayerEvent.PickedUpItem -= Handler.OnDebuggingpickup;
            PlayerEvent.UsingItem -= Handler.Onuse;
            PlayerEvent.ReloadingWeapon -= Handler.Onreloading;
            PlayerEvent.ShootingWeapon -= Handler.Onshooting;
            PlayerEvent.ThrewProjectile -= Handler.Onthrown;

            CustomItemEventHandler.Dispose();

            Instance = null;
            Handler = null;
        }
        public void OnFinishedLoadingPlugins()
        {
            ImportManager.Init();
            Server.RunCommand("uciupdatecheck");
        }
    }
}