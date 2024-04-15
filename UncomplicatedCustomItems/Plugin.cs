using Exiled.API.Features;
using System;
using System.Collections.Generic;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.Interfaces;
using EventSource = Exiled.Events.Handlers.Player;
using EventHandler = UncomplicatedCustomItems.Events.Internal.Player;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems
{
    public class Plugin : Plugin<Config>
    {
        // Basic EXILED shit
        public override string Author => "SpGerg, FoxWorn3365";
        public override string Name => "UncomplicatedCustomItems";
        public override string Prefix => "UncomplicatedCustomItems";
        public override Version Version => new(0, 8, 0);
        public override Version RequiredExiledVersion => new(8, 8, 1);

        // Custom var for plugin management
        public static Plugin Instance { get; private set; }
        internal EventHandler Handler;
        // Item Id => Item
        internal static Dictionary<uint, ICustomItem> Items { get; private set; } = new();
        // Player ID => List<>() of custom items
        internal static Dictionary<int, List<CustomItemHandler>> ItemDictionary { get; private set; } = new();

        public override void OnEnabled()
        {
            Instance = this;
            Handler = new();

            EventSource.DroppingItem += Handler.OnItemDropping;
            EventSource.ItemAdded += Handler.OnItemPickup;
            EventSource.UsingItem += Handler.OnItemUsing;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            EventSource.DroppingItem -= Handler.OnItemDropping;
            EventSource.ItemAdded -= Handler.OnItemPickup;
            EventSource.UsingItem -= Handler.OnItemUsing;

            Handler = null;
            Instance = null;
            base.OnDisabled();
        }
    }
}
