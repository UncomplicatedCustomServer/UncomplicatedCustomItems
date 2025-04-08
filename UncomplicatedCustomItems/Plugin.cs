using Exiled.API.Enums;
using Exiled.API.Features;
using System;
using HarmonyLib;
using System.IO;
using UncomplicatedCustomItems.API.Features.Helper;
using System.Threading.Tasks;
using Handler = UncomplicatedCustomItems.Events.EventHandler;

using PlayerEvent = Exiled.Events.Handlers.Player;
using ItemEvent = Exiled.Events.Handlers.Item;
using ServerEvent = Exiled.Events.Handlers.Server;
using MapEvent = Exiled.Events.Handlers.Map;

namespace UncomplicatedCustomItems
{
    public class Plugin : Plugin<Config>
    {
        public const bool IsPrerelease = true;
        public override string Name => "UncomplicatedCustomItems";

        public override string Prefix => "UncomplicatedCustomItems";

        public override string Author => "SpGerg, FoxWorn & Mr. Baguetter";

        public override Version RequiredExiledVersion { get; } = new(9, 5, 1);

        public override Version Version { get; } = new(3, 2, 0);

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

            if (IsPrerelease)
            {
                ServerEvent.WaitingForPlayers += Handler.OnWaitingForPlayers;
            }

            PlayerEvent.Hurt += Handler.OnHurt;
            PlayerEvent.TriggeringTesla += Handler.OnTriggeringTesla;
            PlayerEvent.Shooting += Handler.OnShooting;
            PlayerEvent.UsingItemCompleted += Handler.OnItemUse;
            ItemEvent.ChangingAttachments += Handler.OnChangingAttachments;
            PlayerEvent.ActivatingWorkstation += Handler.OnWorkstationActivation;
            PlayerEvent.DroppedItem += Handler.OnDrop;
            MapEvent.PickupDestroyed += Handler.OnPickup;
            ServerEvent.RoundEnded += Handler.Onroundend;
            PlayerEvent.Shot += Handler.OnShot;
            PlayerEvent.Shot += Handler.OnShot2;
            ItemEvent.ChargingJailbird += Handler.OnCharge;
            PlayerEvent.Shooting += Handler.OnDieOnUseFlag;
            PlayerEvent.ReceivingEffect += Handler.Receivingeffect;
            PlayerEvent.ThrownProjectile += Handler.ThrownProjectile;
            MapEvent.ExplodingGrenade += Handler.GrenadeExploding;

            //Debugging Events
            PlayerEvent.DroppingItem += Handler.Ondrop;
            PlayerEvent.ItemAdded += Handler.Onpickup;
            PlayerEvent.UsingItem += Handler.Onuse;
            PlayerEvent.ReloadingWeapon += Handler.Onreloading;
            PlayerEvent.Shooting += Handler.Onshooting;

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

            PlayerEvent.Hurt -= Handler.OnHurt;
            PlayerEvent.TriggeringTesla -= Handler.OnTriggeringTesla;
            PlayerEvent.Shooting -= Handler.OnShooting;
            ServerEvent.WaitingForPlayers -= Handler.OnWaitingForPlayers;
            PlayerEvent.UsingItemCompleted -= Handler.OnItemUse;
            ItemEvent.ChangingAttachments -= Handler.OnChangingAttachments;
            PlayerEvent.ActivatingWorkstation -= Handler.OnWorkstationActivation;
            PlayerEvent.DroppedItem -= Handler.OnDrop;
            ServerEvent.RoundEnded -= Handler.Onroundend;
            PlayerEvent.Shot -= Handler.OnShot;
            PlayerEvent.Shot -= Handler.OnShot2;
            ItemEvent.ChargingJailbird -= Handler.OnCharge;
            PlayerEvent.Shooting -= Handler.OnDieOnUseFlag;
            PlayerEvent.ReceivingEffect -= Handler.Receivingeffect;
            PlayerEvent.ThrownProjectile -= Handler.ThrownProjectile;
            MapEvent.ExplodingGrenade -= Handler.GrenadeExploding;

            //Debugging Events
            PlayerEvent.DroppingItem -= Handler.Ondrop;
            PlayerEvent.ItemAdded -= Handler.Onpickup;
            PlayerEvent.UsingItem -= Handler.Onuse;
            PlayerEvent.ReloadingWeapon -= Handler.Onreloading;
            PlayerEvent.Shooting -= Handler.Onshooting;

            Instance = null;
            Handler = null;
            base.OnDisabled();

        }
    }
}