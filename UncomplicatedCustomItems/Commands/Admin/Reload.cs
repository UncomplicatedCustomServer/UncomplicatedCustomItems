using System;
using CommandSystem;
using LabApi.Features.Wrappers;
using MEC;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.Manager;
using UnityEngine;

namespace UncomplicatedCustomItems.Commands.Admin
{
    internal class Reload : Subcommand
    {
        public override string Name { get; } = "reload";

        public override string Description { get; } = "Reloads all custom items";

        public override string VisibleArgs { get; } = string.Empty;

        public override int RequiredArgsCount { get; } = 0;

        public override string RequiredPermission { get; } = "uci.reload";

        public override string[] Aliases { get; } = [""];

        public Dictionary<APICustomItem, List<Vector3>> APICustomItemsPickups = [];
        public Dictionary<CustomItem, List<Vector3>> CustomItemsPickups = [];
        public Dictionary<Player, List<CustomItem>> CustomItems = [];
        public Dictionary<Player, List<APICustomItem>> APICustomItems = [];

        private int ReloadedItems;
        private int ReloadedActions;
        private int ReloadedAPIItems;

        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count > 0)
            {
                response = "This command doesnt have any arguments";
                return false;
            }

            if (CustomItem.List.Count == 0)
            {
                response = $"No loaded custom items!";
                return false;
            }

            if (CustomItem.List.Count > 0)
            {
                CustomItems.Clear();
                CustomItemsPickups.Clear();
                APICustomItems.Clear();
                APICustomItemsPickups.Clear();
                int BeforeItems = CustomItem.List.Count();
                int BeforeActions = CustomAction.List.Count();
                int BeforeAPIItems = APICustomItem.List.Count();
                ReloadedItems = 0;
                ReloadedActions = 0;
                ReloadedAPIItems = 0;

                foreach (SummonedCustomItem item in SummonedCustomItem.List.ToArray())
                {
                    if (item == null || item.CustomItem == null)
                        continue;

                    if (item.Owner != null && !CustomItems.ContainsKey(item.Owner))
                        CustomItems[item.Owner] = [];

                    if (!CustomItemsPickups.ContainsKey(item.CustomItem))
                        CustomItemsPickups[item.CustomItem] = [];

                    if (!item.IsPickup)
                    {
                        if (item.Owner != null)
                            CustomItems[item.Owner].Add(item.CustomItem);
                    }
                    else if (item.IsPickup)
                    {
                        try
                        {
                            Pickup? pickup = item.Pickup;
                            if (pickup != null && pickup.Base != null && !pickup.IsDestroyed)
                            {
                                Vector3 pos = pickup.Position;
                                if (pos != Vector3.zero)
                                    CustomItemsPickups[item.CustomItem].Add(pos);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogManager.Debug($"Skipped pickup position for {item.CustomItem.Name}: {ex.Message}");
                        }
                    }

                    item.Destroy();
                }

                foreach (SummonedAPICustomItem item in SummonedAPICustomItem.List.ToList())
                {
                    if (item == null || item.CustomItem == null)
                        continue;

                    if (item.Owner != null && !APICustomItems.ContainsKey(item.Owner))
                        APICustomItems[item.Owner] = [];

                    if (!APICustomItemsPickups.ContainsKey(item.CustomItem!))
                        APICustomItemsPickups[item.CustomItem!] = [];

                    if (!item.IsPickup)
                    {
                        if (item.Owner != null)
                            APICustomItems[item.Owner].Add(item.CustomItem!);
                    }
                    else if (item.IsPickup)
                    {
                        try
                        {
                            Pickup? pickup = item.Pickup;
                            if (pickup != null && pickup.Base != null && !pickup.IsDestroyed)
                            {
                                Vector3 pos = pickup.Position;
                                if (pos != Vector3.zero)
                                    APICustomItemsPickups[item.CustomItem!].Add(pos);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogManager.Debug($"Skipped API pickup position for {item.CustomItem!.Name}: {ex.Message}");
                        }
                    }

                    item.Destroy();
                }

                foreach (CustomItem customItem in CustomItem.List.ToList())
                {
                    CustomItem.Unregister(customItem.Id);
                    LogManager.Debug($"Unregistered {customItem.Name}.");
                }

                foreach (CustomAction action in CustomAction.List.ToList())
                {
                    CustomAction.Unregister(action.Id);
                    LogManager.Debug($"Unregistered action {action.Name}.");
                }

                foreach (APICustomItem apiitem in APICustomItem.List.ToList())
                {
                    APICustomItem.Unregister(apiitem.Id);
                    LogManager.Debug($"Unregistered API CustomItem: {apiitem.Name}.");
                }

                APICustomItem.CustomItems.Clear();
                CustomItem.CustomItems.Clear();
                CustomItem.UnregisteredCustomItems.Clear();
                CustomAction.CustomActions.Clear();
                CustomAction.UnregisteredCustomActions.Clear();
                SummonedAPICustomItem.SummonedCustomItems.Clear();
                CustomItem.List.Clear();
                CustomItem.UnregisteredList.Clear();
                CustomAction.List.Clear();
                CustomAction.UnregisteredList.Clear();
                ImportManager.ActivePlugins.Clear();
                SummonedCustomItem.List.Clear();
                SummonedAPICustomItem.List.Clear();

                Plugin.Instance.FileConfig.Welcome(loadExamples: true);
                Plugin.Instance.FileConfig.Welcome(Server.Port.ToString());
                Plugin.Instance.FileConfig.Welcome("Actions");
                Plugin.Instance.FileConfig.LoadAllAsync().GetAwaiter().GetResult();
                Plugin.Instance.FileConfig.LoadAllAsync(Server.Port.ToString()).GetAwaiter().GetResult();
                Plugin.Instance.FileConfig.LoadAllAsync("Actions").GetAwaiter().GetResult();

                ImportManager.Actor();

                foreach (CustomItem item in CustomItem.List)
                {
                    ReloadedItems++;                    
                }

                foreach (CustomAction action in CustomAction.List)
                {
                    ReloadedActions++;                    
                }

                foreach (APICustomItem item in APICustomItem.List)
                {
                    ReloadedAPIItems++;                    
                }

                int NewItems = BeforeItems - ReloadedItems;
                int NewActions = BeforeActions - ReloadedActions;
                int NewApiItems = BeforeAPIItems - ReloadedAPIItems;

                foreach (KeyValuePair<CustomItem, List<Vector3>> entry in CustomItemsPickups)
                {
                    foreach (Vector3 pos in entry.Value)
                        Timing.CallDelayed(1f, () => new SummonedCustomItem(entry.Key, pos));
                }

                foreach (KeyValuePair<APICustomItem, List<Vector3>> entry in APICustomItemsPickups)
                {
                    foreach (Vector3 pos in entry.Value)
                        Timing.CallDelayed(1f, () => new SummonedAPICustomItem(entry.Key, pos));
                }

                foreach (KeyValuePair<Player, List<CustomItem>> entry in CustomItems)
                {
                    foreach (CustomItem customItem in entry.Value)
                        Timing.CallDelayed(1f, () => new SummonedCustomItem(customItem, entry.Key));
                }

                foreach (KeyValuePair<Player, List<APICustomItem>> entry in APICustomItems)
                {
                    foreach (APICustomItem customItem in entry.Value)
                        Timing.CallDelayed(1f, () => new SummonedAPICustomItem(customItem, entry.Key));
                }

                List<string> reloadedLines =
                [
                    $"Reloaded {CustomItem.List.Count} CustomItems{(NewItems > 0 ? $" ({NewItems} new)" : "")}.",
                    $"Reloaded {CustomAction.List.Count} CustomActions{(NewActions > 0 ? $" ({NewActions} new)" : "")}.",
                    $"Reloaded {APICustomItem.List.Count} APIItems{(NewApiItems > 0 ? $" ({NewApiItems} new)" : "")}."
                ];

                response = $"\n {string.Join("\n", reloadedLines)}\nUnregistered - Items: {BeforeItems}, Actions: {BeforeActions}, APIItems: {BeforeAPIItems}";
                return true;
            }
            else
            {
                response = $"Couldnt reload Custom items. Unknown error";
                return false;
            }
        }
    }
}