using CommandSystem;
using Exiled.API.Features;
using System.Collections.Generic;
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

        public string[] Aliases { get; } = ["gen"];
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
                foreach (ICustomItem Id in CustomItem.List)
                {
                    CustomItem.Unregister(Id);
                }
                FileConfig FileConfig = Plugin.Instance.FileConfig;
                
                CustomItem.List.Clear();
                FileConfig.Welcome(loadExamples: true);
                FileConfig.Welcome(Server.Port.ToString());
                FileConfig.LoadAll();
                FileConfig.LoadAll(Server.Port.ToString());
                response = $"Reloaded {CustomItem.List.Count} Custom items.";
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