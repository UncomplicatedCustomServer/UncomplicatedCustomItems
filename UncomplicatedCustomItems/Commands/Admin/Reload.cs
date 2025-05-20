using CommandSystem;
using LabApi.Features.Wrappers;
using MEC;
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

        public string VisibleArgs { get; } = string.Empty;

        public int RequiredArgsCount { get; } = 0;

        public PlayerPermissions RequiredPermission { get; } = PlayerPermissions.ServerConfigs;

        public string[] Aliases { get; } = ["reload"];

        public Dictionary<uint, Player> CustomItems = [];

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
                foreach (Player player in Player.ReadyList)
                {
                    List<ushort> ItemsToRemove = new List<ushort>();

                    foreach (Item Item in player.Items)
                    {
                        int id = player.PlayerId;
                        ushort Serial = Item.Serial;
                        if (SummonedCustomItem.TryGet(Serial, out SummonedCustomItem CustomItem))
                        {
                            CustomItems[CustomItem.CustomItem.Id] = player;
                            LogManager.Debug($"Marked {Item.Type} from {player.Nickname} for removal");
                            ItemsToRemove.Add(Serial);
                        }
                    }

                    foreach (ushort Serial in ItemsToRemove)
                    {
                        player.RemoveItem(Item.Get(Serial));
                        LogManager.Debug($"Removed CustomItem with serial {Serial} from {player.Nickname}");
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
                CustomItem.UnregisteredList.Clear();

                FileConfig.Welcome(loadExamples: true);
                FileConfig.Welcome(Server.Port.ToString());
                FileConfig.LoadAll();
                FileConfig.LoadAll(Server.Port.ToString());
                Events.Internal.Server.SpawnItemsOnRoundStarted();

                foreach (var entry in CustomItems)
                {
                    Timing.CallDelayed(1f, () =>
                    {
                        Player player = entry.Value;
                        uint Id = entry.Key;
                        Utilities.TryGetCustomItem(Id, out ICustomItem item);
                        new SummonedCustomItem(item, player);
                    });
                }
                if (NewItems > 0)
                {
                    response = $"\nReloaded {CustomItem.List.Count} Added {NewItems} New Custom items. \n Amount of unregistered Custom items: {CustomItem.UnregisteredList.Count}";
                    return true;
                }
                else
                {
                    response = $"\nReloaded {CustomItem.List.Count} Custom items. \n Amount of unregistered Custom items: {CustomItem.UnregisteredList.Count}";
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