using CommandSystem;
using LabApi.Features.Wrappers;
using MEC;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
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
        private int ReloadedBaseItems;

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
                int BeforeBaseItems = APICustomItem.List.Count();
                ReloadedItems = 0;
                ReloadedActions = 0;
                ReloadedBaseItems = 0;

                foreach (SummonedCustomItem item in SummonedCustomItem.List.ToList())
                    item.Destroy();

                foreach (SummonedAPICustomItem item in SummonedAPICustomItem.List.ToList())
                    item.Destroy();

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

                foreach (APICustomItem baseitem in APICustomItem.List.ToList())
                {
                    APICustomItem.Unregister(baseitem.Id);
                    LogManager.Debug($"Unregistered API CustomItem: {baseitem.Name}.");
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
                Task.Run(ImportManager.Actor);

                if (Round.IsRoundStarted)
                    Events.Internal.Server.SpawnItemsOnRoundStarted();

                foreach (ICustomItem item in CustomItem.List)
                    ReloadedItems++;
                foreach (ICustomAction action in CustomAction.List)
                    ReloadedActions++;
                foreach (APICustomItem item in APICustomItem.List)
                    ReloadedBaseItems++;

                int NewItems = BeforeItems - ReloadedItems;
                int NewActions = BeforeActions - ReloadedActions;
                int NewBaseItems = BeforeBaseItems - ReloadedBaseItems;

                foreach (var entry in CustomItems)
                    Timing.CallDelayed(1f, () => new SummonedCustomItem(entry.Key, entry.Value));

                List<string> reloadedLines =
                [
                    $"Reloaded {CustomItem.List.Count} CustomItems{(NewItems > 0 ? $" ({NewItems} new)" : "")}.",
                    $"Reloaded {CustomAction.List.Count} CustomActions{(NewActions > 0 ? $" ({NewActions} new)" : "")}.",
                    $"Reloaded {APICustomItem.List.Count} BaseItems{(NewBaseItems > 0 ? $" ({NewBaseItems} new)" : "")}."
                ];

                response = "\n" + string.Join("\n", reloadedLines)
                    + $"\nUnregistered - Items: {BeforeItems}, Actions: {BeforeActions}, BaseItems: {BeforeBaseItems}";
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