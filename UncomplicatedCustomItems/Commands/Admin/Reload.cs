using CommandSystem;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using System.Collections.Generic;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.Commands.Admin
{
    internal class Reload : ISubcommand
    {
        public string Name { get; } = "reload";

        public string Description { get; } = "Reloads all custom items";

        public string VisibleArgs { get; } = "";

        public int RequiredArgsCount { get; } = 0;

        public string RequiredPermission { get; } = "uci.reload";

        public string[] Aliases { get; } = ["reload"];
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
                List<Pickup> PickupsToDestroy = new List<Pickup>();

                foreach (Pickup Pickup in Pickup.List)
                {
                    ushort Serial = Pickup.Serial;
                    if (Utilities.IsSummonedCustomItem(Serial))
                    {
                        PickupsToDestroy.Add(Pickup);
                    }
                }

                foreach (Pickup Pickup in PickupsToDestroy)
                {
                    LogManager.Debug($"Destroyed {Pickup.Type}");
                    Pickup.Destroy();
                }
                foreach (Player player in Player.List)
                {
                    List<ushort> ItemsToRemove = new List<ushort>();

                    foreach (Item Item in player.Items)
                    {
                        int id = player.Id;
                        ushort Serial = Item.Serial;
                        if (Utilities.IsSummonedCustomItem(Serial))
                        {
                            LogManager.Debug($"Marked {Item.Type} from {player.DisplayNickname} for removal");
                            ItemsToRemove.Add(Serial);
                        }
                    }

                    foreach (ushort Serial in ItemsToRemove)
                    {
                        player.RemoveItem(Serial);
                        LogManager.Debug($"Removed CustomItem with serial {Serial} from {player.DisplayNickname}");
                    }
                }

                foreach (ICustomItem customItem in CustomItem.List)
                {
                    CustomItem.Unregister(customItem.Id);
                    LogManager.Debug($"Unregistered {customItem.Name}.");
                }
                FileConfig FileConfig = Plugin.Instance.FileConfig;
                int Before = CustomItem.List.Count;
                int NewItems = CustomItem.List.Count - Before;
                SummonedCustomItem.List.Clear();
                CustomItem.List.Clear();

                FileConfig.Welcome(loadExamples: true);
                FileConfig.Welcome(Server.Port.ToString());
                FileConfig.LoadAll();
                FileConfig.LoadAll(Server.Port.ToString());
                Events.Internal.Server.SpawnItemsOnRoundStarted();
                if (NewItems > 0)
                {
                    response = $"Reloaded {CustomItem.List.Count} Added {NewItems} New Custom items.\nReplace the customitem in your inventory with a new one to get the new settings";
                    return true;
                }
                else
                {
                    response = $"Reloaded {CustomItem.List.Count} Custom items.\nReplace the customitem in your inventory with a new one to get the new settings";
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