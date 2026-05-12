using CommandSystem;
using LabApi.Features.Wrappers;
using MEC;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Interfaces;
using UnityEngine;

namespace UncomplicatedCustomItems.Commands.Admin
{
    internal class Reload : ISubcommand
    {
        public string Name { get; } = "reload";

        public string Description { get; } = "Reloads all custom items";

        public string VisibleArgs { get; } = string.Empty;

        public int RequiredArgsCount { get; } = 0;

        public string RequiredPermission { get; } = "uci.reload";

        public string[] Aliases { get; } = [""];

        public Dictionary<APICustomItem, List<Vector3>> APICustomItemsPickups = [];
        public Dictionary<ICustomItem, List<Vector3>> CustomItemsPickups = [];
        public Dictionary<Player, List<ICustomItem>> CustomItems = [];
        public Dictionary<Player, List<APICustomItem>> APICustomItems = [];

        private int ReloadedItems;
        private int ReloadedActions;
        private int ReloadedAPIItems;

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
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
                int BeforeItems = CustomItem.List.Count();
                int BeforeActions = CustomAction.List.Count();
                int BeforeAPIItems = APICustomItem.List.Count();
                ReloadedItems = 0;
                ReloadedActions = 0;
                ReloadedAPIItems = 0;

                foreach (SummonedCustomItem item in SummonedCustomItem.List.ToArray())
                {
                    if (item == null)
                        continue;

                    if (item.Owner != null && !CustomItems.ContainsKey(item.Owner))
                        CustomItems[item.Owner] = [];

                    if (!CustomItemsPickups.ContainsKey(item.CustomItem))
                        CustomItemsPickups[item.CustomItem] = [];

                    if (!item.IsPickup)
                    {
                        CustomItems[item.Owner!].Add(item.CustomItem);
                    }
                    else if (item.IsPickup && item.Pickup?.Position != null && item.Pickup.Position != Vector3.zero)
                        CustomItemsPickups[item.CustomItem].Add(item.Pickup.Position);

                    item.Destroy();
                }

                foreach (SummonedAPICustomItem item in SummonedAPICustomItem.List.ToList())
                {
                    if (item == null)
                        continue;

                    if (item.Owner != null && !APICustomItems.ContainsKey(item.Owner))
                        APICustomItems[item.Owner] = [];

                    if (!APICustomItemsPickups.ContainsKey(item.CustomItem!))
                        APICustomItemsPickups[item.CustomItem!] = [];

                    if (!item.IsPickup)
                    {
                        APICustomItems[item.Owner!].Add(item.CustomItem!);                        
                    }
                    else if (item.IsPickup && item.Pickup?.Position != null && item.Pickup.Position != Vector3.zero)
                        APICustomItemsPickups[item.CustomItem!].Add(item.Pickup.Position);

                    item.Destroy();
                }

                foreach (ICustomItem customItem in CustomItem.List.ToList())
                {
                    CustomItem.Unregister(customItem.Id);
                    LogManager.Debug($"Unregistered {customItem.Name}.");
                }

                foreach (ICustomAction action in CustomAction.List.ToList())
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
                Plugin.Instance.FileConfig.LoadAll();
                Plugin.Instance.FileConfig.LoadAll(Server.Port.ToString());
                Plugin.Instance.FileConfig.LoadAll("Actions");
                ImportManager.Actor();

                foreach (ICustomItem item in CustomItem.List)
                    ReloadedItems++;
                foreach (ICustomAction action in CustomAction.List)
                    ReloadedActions++;
                foreach (APICustomItem item in APICustomItem.List)
                    ReloadedAPIItems++;

                int NewItems = BeforeItems - ReloadedItems;
                int NewActions = BeforeActions - ReloadedActions;
                int NewApiItems = BeforeAPIItems - ReloadedAPIItems;

                foreach (var entry in CustomItemsPickups)
                {
                    foreach (Vector3 pos in entry.Value)
                        Timing.CallDelayed(1f, () => new SummonedCustomItem(entry.Key, pos));
                }

                foreach (var entry in APICustomItemsPickups)
                {
                    foreach (Vector3 pos in entry.Value)
                        Timing.CallDelayed(1f, () => new SummonedAPICustomItem(entry.Key, pos));
                }

                foreach (var entry in CustomItems)
                {
                    foreach (ICustomItem customItem in entry.Value)
                        Timing.CallDelayed(1f, () => new SummonedCustomItem(customItem, entry.Key));
                }

                foreach (var entry in APICustomItems)
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