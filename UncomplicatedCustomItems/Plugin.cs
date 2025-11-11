#if EXILED
using Exiled.API.Features;
using Exiled.API.Enums;
#else
using LabApi.Features.Wrappers;
using LabApi.Loader;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
#endif
#if DEBUG
using UncomplicatedCustomItems.Events.Handlers;
using UncomplicatedCustomItems.Events.Arguments.JailbirdEvents;
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.Commands;
using UncomplicatedCustomItems.Events;
using UncomplicatedCustomItems.Integrations;
using UnityEngine;
using UserSettings.ServerSpecific;
using Handler = UncomplicatedCustomItems.Events.EventHandler;
using HarmonyLib;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
// Events
using PlayerEvent = LabApi.Events.Handlers.PlayerEvents;
using ServerEvent = LabApi.Events.Handlers.ServerEvents;


// Building for remote development. You can ignore this :)
// & "C:\Program Files\Microsoft Visual Studio\18\Insiders\MSBuild\Current\Bin\MSBuild.exe" UncomplicatedCustomItems.csproj /p:Configuration=LabApi
// & "C:\Program Files\Microsoft Visual Studio\18\Insiders\MSBuild\Current\Bin\MSBuild.exe" UncomplicatedCustomItems.csproj /p:Configuration=Exiled

namespace UncomplicatedCustomItems
{
	public class Plugin : Plugin<Config>
	{
		public bool IsPrerelease = true;
		public override string Name => "UncomplicatedCustomItems";
#if EXILED
        public override string Prefix => "UncomplicatedCustomItems";
#else
		public override string Description => "Enables server owners to design and manage CustomItems with ease, no coding required.";
#endif
		public override string Author => "SpGerg, FoxWorn & Mr. Baguetter";
#if EXILED
        public override Version RequiredExiledVersion { get; } = new(9, 10, 1);
#else
		public override Version RequiredApiVersion { get; } = LabApi.Features.LabApiProperties.CurrentVersion;
#endif
		public override Version Version { get; } = new(4, 0, 0);

		internal Handler Handler;

		public Assembly Assembly => Assembly.GetExecutingAssembly();
#if EXILED
        public override PluginPriority Priority => PluginPriority.First;
#else
		public override LoadPriority Priority => LoadPriority.Highest;
#endif
		/// <summary>
		/// The <see cref="Plugin"/> instance.
		/// </summary>
		public static Plugin Instance { get; private set; }

		internal Harmony _harmony;

		internal static HttpManager HttpManager;

		internal FileConfig FileConfig;

		internal List<ServerSpecificSettingBase> _settings;
		

#if EXILED
        public override void OnEnabled()
#else
		public override void Enable()
#endif
		{
			Instance = this;

			FileConfig = new();
			HttpManager = new("uci");
			Handler = new();
#if EXILED
            if (!File.Exists(Path.Combine(ConfigPath, "UncomplicatedCustomItems", ".nohttp")))
#else
			if (!File.Exists(Path.Combine(ConfigurationLoader.GetConfigPath(this, "UncomplicatedCustomItems"), "UncomplicatedCustomItems", ".nohttp")))
#endif

			PlayerHandler.Register();
			ServerHandler.Register();
			ScpHandler.Register();

			ServerEvent.WaitingForPlayers += OnFinishedLoading;
			ServerSpecificSettingsSync.ServerOnSettingValueReceived += Handler.OnValueReceived;

#if DEBUG
			JailbirdEvents.ChangingWearState += OnChangingWearState;
			JailbirdEvents.ChangedWearState += OnChangedWearState;
#endif

            // Debugging Events
            PlayerEvent.DroppingItem += Handler.OnDrop;
			PlayerEvent.PickedUpItem += Handler.OnDebuggingPickup;
			PlayerEvent.UsingItem += Handler.OnUse;
			PlayerEvent.ReloadingWeapon += Handler.OnReloading;
			PlayerEvent.ShootingWeapon += Handler.OnShooting;
			PlayerEvent.ThrewProjectile += Handler.OnThrown;

			Arguments.Initialize();
			Arguments.Register();

			if (Config.EnablesssSettings)
			{
				try
				{
					_settings =
					[
						new SSGroupHeader(Config.KeybingSettingHeaderName),
						new SSKeybindSetting(Config.KeybindSettingId, Config.KeybindSettingName, KeyCode.K, hint: Config.KeybindSettingHint, allowSpectatorTrigger: false)
					];
				}
				catch (Exception e)
				{
					LogManager.Error($"Failed to initialize settings: {e.Message}\n{e.StackTrace}");
				}

				try
				{
					ServerSpecificSettingsSync.DefinedSettings = _settings.ToArray();
					ServerSpecificSettingsSync.SendToAll();
				}
				catch (Exception e)
				{
					LogManager.Error($"Failed to send settings: {e.Message}\n{e.StackTrace}");
				}
			}

			LogManager.History.Clear();

			LogManager.Info("===========================================");
			LogManager.Info("Thanks for using UncomplicatedCustomItems");
			LogManager.Info($"    by {Author}");
			LogManager.Info("===========================================");
#if EXILED
            LogManager.Info($"Loaded from Exiled! [{Exiled.Loader.Loader.Version} - {RequiredExiledVersion}]");
#else
			LogManager.Info($"Loaded from LabApi! [{LabApi.Features.LabApiProperties.CurrentVersion} - {RequiredApiVersion}]");
#endif
			LogManager.Info(">> Join our discord: https://discord.gg/5StRGu8EJV <<");

			/*
			if (IsPrerelease)
			{
				if (!Instance.Config.Debug)
				{
					LogManager.Info("Debug logs have been activated!");
					Instance.Config.Debug = true;
					DebugMode = true;
				}
			}
			*/

			FileConfig.Welcome(loadExamples: true);
			FileConfig.Welcome(Server.Port.ToString());
			FileConfig.Welcome("Actions");
			FileConfig.LoadAll();
			FileConfig.LoadAll(Server.Port.ToString());
			FileConfig.LoadAll("Actions");

#if EXILED
            _harmony = new($"com.ucs.uci_exiled-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
#else
			_harmony = new($"com.ucs.uci_labapi-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
#endif
			_harmony.PatchAll();
#if EXILED
            if (Round.IsStarted)
#else
			if (Round.IsRoundStarted)
#endif
				ServerHandler.SpawnItemsOnRoundStarted();

			if (Instance.Config.AllowDevPermissions)
				LogManager.Security($"Allow Dev Permissions is enabled in your config! Any UCI developers can run commands on your server. If this was not intended, please disable it.");

#if EXILED
            base.OnEnabled();
#endif
		}
#if EXILED
        public override void OnDisabled()
#else
		public override void Disable()
#endif
		{
			HttpManager.StopPresence();
			ECRIntegration.Cleanup();

			// Cleanup
			CustomItem.List.Clear();
			CustomItem.UnregisteredCustomItems.Clear();
			CustomItem.CustomItems.Clear();
			CustomItem.UnregisteredCustomItems.Clear();
			CustomAction.CustomActions.Clear();
			CustomAction.List.Clear();
			CustomAction.UnregisteredCustomActions.Clear();
			CustomAction.UnregisteredList.Clear();
			SummonedCustomItem.List.ForEach(sci => sci.Destroy());
			ArgumentManager._actionHandlers.Clear();
			ArgumentManager._eventArgPropertyCache.Clear();
			BaseCommand.Subcommands.Clear();
			PlayerHandler._capybaras.Clear();
			PlayerHandler._damageTimes.Clear();
			PlayerHandler._toolGunPrimitives.Clear();
			PlayerExtensions.PlayerKills.Clear();
			PlayerHandler.CustomScp268Effects.Clear();

			_settings = null;

			HttpManager.UnregisterEvents();
			_harmony.UnpatchAll();
			_harmony = null;

			PlayerHandler.Unregister();
			ServerHandler.Unregister();
			ScpHandler.Unregister();
			MERIntergration.Unregister();

#if DEBUG
            JailbirdEvents.ChangingWearState -= OnChangingWearState;
			JailbirdEvents.ChangedWearState -= OnChangedWearState;
#endif

			ServerEvent.WaitingForPlayers -= OnFinishedLoading;
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
#if EXILED
            base.OnDisabled();
#endif
		}

#if DEBUG
		public void OnChangingWearState(ChangingWearStateEventArgs ev) =>
			LogManager.Debug($"Attempted to set Wearstate to {ev.NewWearState} from {ev.OldWearState}");

        public void OnChangedWearState(ChangedWearStateEventArgs ev) =>
			LogManager.Debug($"Set Wearstate to {ev.NewWearState} from {ev.OldWearState}");
#endif

		public void OnFinishedLoading()
		{
			HttpManager.StartPresence();
			if (Instance.Config.AllowDevPermissions)
				LogManager.Security($"Allow Dev Permissions is enabled in your config! Any UCI developers can run commands on your server. If this was not intended, please disable it.");

			ImportManager.Init();
			_ = Task.Run(Updater.CheckForUpdatesAsync);

			LabAPIExtensions.Init();
			MERIntergration.Init();
			ECRIntegration.Init();
			ECIIntegration.Init();
			AudioApi.Init();
#if EXILED
            CommonUtilitiesPatch.Initialize();
#endif
		}
	}
}