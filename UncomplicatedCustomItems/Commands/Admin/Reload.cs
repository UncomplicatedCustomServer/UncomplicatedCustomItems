using CommandSystem;
using LabApi.Features.Wrappers;
using MEC;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.Commands.Admin
{
    internal class Reload : ISubcommand
    {
        public string Name { get; } = "reload";

        public string Description { get; } = "Reloads all custom items";

        public string VisibleArgs { get; } = string.Empty;

        public int RequiredArgsCount { get; } = 0;

        public string RequiredPermission { get; } = "uci.reload";

        public string[] Aliases { get; } = ["reload"];

        public Dictionary<ICustomItem, Player> CustomItems = [];

        private int ReloadedItems;
        private int ReloadedActions;

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
                ReloadedItems = 0;
                ReloadedActions = 0;

                foreach (SummonedCustomItem item in SummonedCustomItem.List.ToList())
                {
                    if (item.IsPickup)
                        item.Pickup.Destroy();
                    else
                    {
                        CustomItems.Add(item.CustomItem, item.Owner);
                        item.Destroy();
                    }
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

                ImportManager.ActivePlugins.Clear();
                SummonedCustomItem.List.Clear();
                CustomItem.List.Clear();
                CustomItem.UnregisteredList.Clear();
                CustomAction.List.Clear();
                CustomAction.UnregisteredList.Clear();

                Plugin.Instance.FileConfig.Welcome(loadExamples: true);
                Plugin.Instance.FileConfig.Welcome(Server.Port.ToString());
                Plugin.Instance.FileConfig.Welcome("Actions");
                Plugin.Instance.FileConfig.LoadAll();
                Plugin.Instance.FileConfig.LoadAll(Server.Port.ToString());
                Plugin.Instance.FileConfig.LoadAll("Actions");
                Task.Run(ImportManager.Actor);

                if (Round.IsRoundStarted)
                    Events.Internal.Server.SpawnItemsOnRoundStarted();

                foreach (ICustomItem item in CustomItem.List)
                    ReloadedItems++;
                foreach (ICustomAction action in CustomAction.List)
                    ReloadedActions++;

                int NewItems = BeforeItems - ReloadedItems;
                int NewActions = BeforeActions - ReloadedActions;

                foreach (var entry in CustomItems)
                    Timing.CallDelayed(1f, () => new SummonedCustomItem(entry.Key, entry.Value));

                if (NewItems > 0 && NewActions > 0)
                {
                    response = $"\nReloaded {CustomItem.List.Count} CustomItems ({NewItems} new). \nReloaded {CustomAction.List.Count} CustomActions ({NewActions} new).\nUnregistered - Items: {CustomItem.UnregisteredList.Count}, Actions: {CustomAction.UnregisteredList.Count}";
                    return true;
                }
                else if (NewItems > 0)
                {
                    response = $"\nReloaded {CustomItem.List.Count} CustomItems ({NewItems} new).\nReloaded {CustomAction.List.Count} CustomActions.\nUnregistered - Items: {CustomItem.UnregisteredList.Count}, Actions: {CustomAction.UnregisteredList.Count}";
                    return true;
                }
                else if (NewActions > 0)
                {
                    response = $"\nReloaded {CustomAction.List.Count} CustomActions ({NewActions} new).\nUnregistered - Actions: {CustomAction.UnregisteredList.Count}";
                    return true;
                }
                else
                {
                    response = $"\nReloaded {CustomItem.List.Count} CustomItems.\nReloaded {CustomAction.List.Count} CustomActions.\nUnregistered - Items: {CustomItem.UnregisteredList.Count}, Actions: {CustomAction.UnregisteredList.Count}";
                    return true;
                }
            }
            else
            {
                response = $"Couldnt reload Custom items. Unknown error";
                return false;
            }
        }
    }
}