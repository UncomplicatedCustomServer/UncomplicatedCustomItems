using Exiled.API.Enums;
using Exiled.API.Features;
using System;
using HarmonyLib;
using System.IO;
using UncomplicatedCustomItems.API.Features.Helper;
using System.Threading.Tasks;
using Handler = UncomplicatedCustomItems.Events.EventHandler;
using UncomplicatedCustomItems.Integration;

using PlayerEvent = Exiled.Events.Handlers.Player;
using ItemEvent = Exiled.Events.Handlers.Item;
using ServerEvent = Exiled.Events.Handlers.Server;
using MapEvent = Exiled.Events.Handlers.Map;

namespace UncomplicatedCustomItems
{
    public class Plugin : Plugin<Config>
    {
        public bool IsPrerelease = true;
        public override string Name => "UncomplicatedCustomItems";

        public override string Prefix => "UncomplicatedCustomItems";

        public override string Author => "SpGerg, FoxWorn & Mr. Baguetter";

        public override Version RequiredExiledVersion { get; } = new(9, 5, 1);

        public override Version Version { get; } = new(3, 5, 0);

        internal Handler Handler;

        public override PluginPriority Priority => PluginPriority.First;

        public static Plugin Instance { get; private set; }

        internal Harmony _harmony;

        internal static HttpManager HttpManager;

        internal FileConfig FileConfig;

        public override void OnEnabled()
        {
            Instance = this;

            FileConfig = new();
            HttpManager = new("uci");
            Handler = new();

            if (!File.Exists(Path.Combine(ConfigPath, "UncomplicatedCustomItems", ".nohttp")))

            PlayerEvent.Hurt += Handler.OnHurt;
            PlayerEvent.TriggeringTesla += Handler.OnTriggeringTesla;
            PlayerEvent.Shooting += Handler.OnShooting;
            PlayerEvent.UsingItemCompleted += Handler.OnItemUse;
            ItemEvent.ChangingAttachments += Handler.OnChangingAttachments;
            PlayerEvent.ActivatingWorkstation += Handler.OnWorkstationActivation;
            PlayerEvent.DroppedItem += Handler.OnDrop;
            MapEvent.PickupDestroyed += Handler.OnPickup;
            PlayerEvent.Shot += Handler.OnShot;
            PlayerEvent.Shot += Handler.OnShot2;
            ItemEvent.ChargingJailbird += Handler.OnCharge;
            PlayerEvent.Shooting += Handler.OnDieOnUseFlag;
            PlayerEvent.ReceivingEffect += Handler.Receivingeffect;
            PlayerEvent.ThrownProjectile += Handler.ThrownProjectile;
            MapEvent.ExplodingGrenade += Handler.GrenadeExploding;
            ServerEvent.WaitingForPlayers += OnFinishedLoadingPlugins;
            PlayerEvent.Dying += Handler.OnDying;
            PlayerEvent.ChangedItem += Handler.OnChangedItem;
            PlayerEvent.DroppingItem += Handler.OnDropping;

            // Debugging Events
            PlayerEvent.DroppingItem += Handler.Ondrop;
            PlayerEvent.ItemAdded += Handler.Onpickup;
            PlayerEvent.UsingItem += Handler.Onuse;
            PlayerEvent.ReloadingWeapon += Handler.Onreloading;
            PlayerEvent.Shooting += Handler.Onshooting;
            PlayerEvent.ThrownProjectile += Handler.Onthrown;

            LogManager.History.Clear();

            LogManager.Info("===========================================");
            LogManager.Info(" Thanks for using UncomplicatedCustomItems");
            LogManager.Info($"    by {Author}");
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

            if (IsPrerelease)
            {
                Harmony.DEBUG = true;
            }

            _harmony = new($"com.ucs.uci_exiled-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
            _harmony.PatchAll();

            ServerConsole.ReloadServerName();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Events.Internal.Player.Unregister();
            Events.Internal.Server.Unregister();

            HttpManager.UnregisterEvents();
            _harmony.UnpatchAll();
            _harmony = null;

            PlayerEvent.Hurt -= Handler.OnHurt;
            PlayerEvent.TriggeringTesla -= Handler.OnTriggeringTesla;
            PlayerEvent.Shooting -= Handler.OnShooting;
            PlayerEvent.UsingItemCompleted -= Handler.OnItemUse;
            ItemEvent.ChangingAttachments -= Handler.OnChangingAttachments;
            PlayerEvent.ActivatingWorkstation -= Handler.OnWorkstationActivation;
            PlayerEvent.DroppedItem -= Handler.OnDrop;
            PlayerEvent.Shot -= Handler.OnShot;
            PlayerEvent.Shot -= Handler.OnShot2;
            ItemEvent.ChargingJailbird -= Handler.OnCharge;
            PlayerEvent.Shooting -= Handler.OnDieOnUseFlag;
            PlayerEvent.ReceivingEffect -= Handler.Receivingeffect;
            PlayerEvent.ThrownProjectile -= Handler.ThrownProjectile;
            MapEvent.ExplodingGrenade -= Handler.GrenadeExploding;
            ServerEvent.WaitingForPlayers -= OnFinishedLoadingPlugins;
            MapEvent.PickupDestroyed -= Handler.OnPickup;
            PlayerEvent.Dying -= Handler.OnDying;
            PlayerEvent.ChangedItem -= Handler.OnChangedItem;
            PlayerEvent.DroppingItem -= Handler.OnDropping;

            // Debugging Events
            PlayerEvent.DroppingItem -= Handler.Ondrop;
            PlayerEvent.ItemAdded -= Handler.Onpickup;
            PlayerEvent.UsingItem -= Handler.Onuse;
            PlayerEvent.ReloadingWeapon -= Handler.Onreloading;
            PlayerEvent.Shooting -= Handler.Onshooting;
            PlayerEvent.ThrownProjectile -= Handler.Onthrown;

            Instance = null;
            Handler = null;
            base.OnDisabled();

        }
        public void OnFinishedLoadingPlugins()
        {
            CommonUtilitiesPatch.Initialize();
        }
    }
}