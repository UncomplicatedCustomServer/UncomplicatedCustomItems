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
using MEC;
using UncomplicatedCustomItems.API.Features.Networking;

// Commands for remote development. You can ignore this :)
// cd "C:\Program Files (x86)\steam\steamapps\common\SCP Secret Laboratory Dedicated Server"

// Release Building
// & "C:\Program Files\Microsoft Visual Studio\18\Insiders\MSBuild\Current\Bin\MSBuild.exe" UncomplicatedCustomItems.csproj /p:Configuration=LabApi
// & "C:\Program Files\Microsoft Visual Studio\18\Insiders\MSBuild\Current\Bin\MSBuild.exe" UncomplicatedCustomItems.csproj /restore /p:Configuration=Exiled

// Debug Building
// & "C:\Program Files\Microsoft Visual Studio\18\Insiders\MSBuild\Current\Bin\MSBuild.exe" UncomplicatedCustomItems.csproj /p:Configuration=LabApi-Debug
// & "C:\Program Files\Microsoft Visual Studio\18\Insiders\MSBuild\Current\Bin\MSBuild.exe" UncomplicatedCustomItems.csproj /restore /p:Configuration=Exiled-Debug

namespace UncomplicatedCustomItems
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "UncomplicatedCustomItems";
#if EXILED
        public override string Prefix => "UncomplicatedCustomItems";
#else
        public override string Description => "Enables server owners to design and manage CustomItems with ease, no coding required.";
#endif
        public override string Author => "SpGerg, FoxWorn & Mr. Baguetter";
#if EXILED
        public override Version RequiredExiledVersion => new(9, 13, 1);
#else
        public override Version RequiredApiVersion => LabApiProperties.CurrentVersion;
#endif
        public override Version Version => new(4, 1, 0);

        public Assembly Assembly => Assembly.GetExecutingAssembly();
#if EXILED
        public override PluginPriority Priority => PluginPriority.First;
#else
        public override LoadPriority Priority => LoadPriority.Highest;
#endif

#nullable disable
        public static Plugin Instance { get; private set; }

        internal Harmony _harmony;

        internal FileConfig FileConfig;
// #nullable enable

        internal List<ServerSpecificSettingBase> _settings = [];

        private bool FailedToPatch { get; set; }

#if EXILED
        public override void OnEnabled()
#else
        public override void Enable()
#endif
        {
            Instance = this;
            FileConfig = new();

            try
            {
#if EXILED
    			_harmony = new($"com.ucs.uci_exiled-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
#else
                _harmony = new($"com.ucs.uci_labapi-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
#endif

#if DEBUG
                Harmony.DEBUG = true;
#endif
                _harmony.PatchAll();
                LogManager.Debug($"Successfully enabled {_harmony.GetPatchedMethods().Count()} Harmony patches");
                FailedToPatch = false;
            }
            catch (HarmonyException ex)
            {
                FailedToPatch = true;
                LogManager.Error($"Failed to enable Harmony patches! \nMessage: {ex.Message}\n InnerException: {ex.InnerException?.Message}\n Full Exception: {ex}\n");

                if (ex.InnerException != null)
                    LogManager.Error($"Inner Exception Details: \n{ex.InnerException}");
            }
            catch (Exception ex)
            {
                FailedToPatch = true;
                LogManager.Error($"Unexpected error while enabling Harmony patches: \n{ex}");
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

            _settings = null!;

            CreditsRequest.Unregister();
            _harmony.UnpatchAll();
            _harmony = null;

            PlayerHandler.Unregister();
            ServerHandler.Unregister();
            ScpHandler.Unregister();
            SSSHandler.Unregister();

            ServerEvent.WaitingForPlayers -= OnFinishedLoading;

            Arguments.Cleanup();

            Instance = null;
#if EXILED
            base.OnDisabled();
#endif
        }

        public void OnFinishedLoading()
        {
            PresenceRequest request = new();
            request.SendRequest();

            if (Instance.Config.AllowDevPermissions)
                LogManager.Security($"Allow Dev Permissions is enabled in your config! Any UCI developers can run commands on your server. If this was not intended, please disable it.");

            if (Config.AllowAutomaticUpdates)
            {
                Timing.RunCoroutine(Updater.UpdatePluginCoroutine(string.Empty));
            }
            else
                Timing.RunCoroutine(Updater.CheckForUpdatesCoroutine());

            CreditsRequest credit = new();
            credit.SendRequest();

            ImportManager.Init();
            VersionManager.Init();
            BackupSystem.Init();
            LabAPIExtensions.Init();
            ECRIntegration.Init();
            ECIIntegration.Init();
            AudioApi.Init();
#if EXILED
            CommonUtilitiesPatch.Initialize();
#endif

            if (FailedToPatch)
                Timing.RunCoroutine(PatchWarningCoroutine());
        }

        private IEnumerator<float> PatchWarningCoroutine()
        {
            while (FailedToPatch)
            {
                LogManager.Warn($"Harmony patching failed! UCI will not behave correctly PLEASE report this to the developers!");
                yield return Timing.WaitForSeconds(10f);
            }
        }
    }
}