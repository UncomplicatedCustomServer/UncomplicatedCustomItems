#if EXILED
using Exiled.API.Features;
using Exiled.API.Enums;
#else
using LabApi.Loader.Features.Plugins;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins.Enums;
using LabApi.Features;
#endif

using System;
using System.Collections.Generic;
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
using HarmonyLib;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.CustomModuleAPI;
using System.Linq;
using ServerEvent = LabApi.Events.Handlers.ServerEvents;
using UncomplicatedCustomItems.API.Components;
using System.Collections;

// Building for remote development. You can ignore this :)
// & "C:\Program Files\Microsoft Visual Studio\18\Insiders\MSBuild\Current\Bin\MSBuild.exe" UncomplicatedCustomItems.csproj /p:Configuration=LabApi
// & "C:\Program Files\Microsoft Visual Studio\18\Insiders\MSBuild\Current\Bin\MSBuild.exe" UncomplicatedCustomItems.csproj /restore /p:Configuration=Exiled

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
        public override Version RequiredExiledVersion { get; } = new(9, 12, 6);
#else
		public override Version RequiredApiVersion { get; } = LabApiProperties.CurrentVersion;
#endif
		public override Version Version { get; } = new(4, 1, 0);

		public Assembly Assembly => Assembly.GetExecutingAssembly();
#if EXILED
        public override PluginPriority Priority => PluginPriority.First;
#else
		public override LoadPriority Priority => LoadPriority.Highest;
#endif

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

#if EXILED
			if (LabApi.Loader.PluginLoader.EnabledPlugins.Any(p => p.Name == "UncomplicatedCustomItems"))
			{
				LogManager.Warn($"You have both Exiled and LabApi versions of UCI installed this is not supported! Remove one of these for UCI to be enabled.");
				OnDisabled();
				return;
			}
#else
            Type loaderType = Type.GetType("Exiled.Loader.Loader, Exiled.Loader");
            if (loaderType != null)
            {
                PropertyInfo pluginsProperty = loaderType.GetProperty("Plugins", BindingFlags.Public | BindingFlags.Static);
                if (pluginsProperty != null)
                {
                    if (pluginsProperty.GetValue(null) is IEnumerable plugins)
                    {
                        foreach (object plugin in plugins)
                        {
                            PropertyInfo nameProperty = plugin.GetType().GetProperty("Name");
                            if (nameProperty != null)
                            {
                                string name = nameProperty.GetValue(plugin) as string;
                                if (!string.IsNullOrEmpty(name))
                                {
                                    if (name == "UncomplicatedCustomItems")
                                    {
                                        LogManager.Warn($"You have both Exiled and LabApi versions of UCI installed this is not supported! Remove one of these for UCI to be enabled.");
                                        Disable();
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
#endif

            Instance = this;
			FileConfig = new();
			HttpManager = new("uci");

			try
			{
#if EXILED
            	_harmony = new($"com.ucs.uci_exiled-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
#else
				_harmony = new($"com.ucs.uci_labapi-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
#endif
				_harmony.PatchAll();
				LogManager.Debug($"Successfully enabled {_harmony.GetPatchedMethods().Count()} patches");
			}
			catch (HarmonyException ex)
			{
				LogManager.Error($"Failed to enable patches! \n\n {ex.Message} \n\n {ex.StackTrace}");
			}

			PlayerHandler.Register();
			ServerHandler.Register();
			ScpHandler.Register();
			SSSHandler.Register();
			CustomModuleManager.Init();

			ServerEvent.WaitingForPlayers += OnFinishedLoading;

			Arguments.Initialize();
			Arguments.Register();

			if (Config.EnablesssSettings)
			{
				try
				{
					_settings =
					[
						new SSGroupHeader(Config.KeybindSettingHeaderName),
						new SSKeybindSetting(Config.KeybindSettingId, Config.KeybindSettingName, KeyCode.K, hint: Config.KeybindSettingHint, allowSpectatorTrigger: false)
					];
				}
				catch (Exception e)
				{
					LogManager.Error($"Failed to initialize SSS settings: {e.Message}\n{e.StackTrace}");
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
			LogManager.Info($"Loaded from LabApi! [{LabApiProperties.CurrentVersion} - {RequiredApiVersion}]");
#endif
			LogManager.Info(">> Join our discord: https://discord.gg/5StRGu8EJV <<");

			FileConfig.Welcome(loadExamples: true);
			FileConfig.Welcome(Server.Port.ToString());
			FileConfig.Welcome("Actions");
			FileConfig.LoadAll();
			FileConfig.LoadAll(Server.Port.ToString());
			FileConfig.LoadAll("Actions");

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
			if (LabApi.Features.Wrappers.Player.Host != null && LabApi.Features.Wrappers.Player.Host.GameObject.TryGetComponent<Presence>(out var presence))
                UnityEngine.Object.Destroy(presence);

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
			SSSHandler.Unregister();
			MERIntergration.Unregister();

			ServerEvent.WaitingForPlayers -= OnFinishedLoading;

			Arguments.Cleanup();

			Instance = null;
#if EXILED
            base.OnDisabled();
#endif
		}
		
		public void OnFinishedLoading()
		{
            LabApi.Features.Wrappers.Player.Host.GameObject.AddComponent<Presence>().Init(30, 5);
			//HttpManager.StartPresence();
			if (Instance.Config.AllowDevPermissions)
				LogManager.Security($"Allow Dev Permissions is enabled in your config! Any UCI developers can run commands on your server. If this was not intended, please disable it.");

			ImportManager.Init();
#if EXILED
			Server.Host?.ReferenceHub.StartCoroutine(Updater.CheckForUpdatesCoroutine());
#else
			Player.Host?.ReferenceHub.StartCoroutine(Updater.CheckForUpdatesCoroutine());
#endif
			_ = Task.Run(VersionManager.Init);

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