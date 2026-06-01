using CommandSystem;
#if EXILED
using Exiled.API.Interfaces;
#endif
using System.Collections.Generic;
using System.Text;
using UncomplicatedCustomItems.API.CustomModuleAPI;
using UncomplicatedCustomItems.API.Interfaces;
using LabPlugin = LabApi.Loader.Features.Plugins.Plugin;

namespace UncomplicatedCustomItems.Commands.Admin
{
    internal class CustomModuleInfo : ISubcommand
    {
        public string Name { get; } = "custommoduleinfo";
        public string Description { get; } = "Gets info on all registered CustomModules";
        public string VisibleArgs { get; } = "";
        public int RequiredArgsCount { get; } = 0;
        public string RequiredPermission { get; } = "uci.custommoduleinfo";
        public string[] Aliases { get; } = ["cmi"];

        public bool Execute(List<string> args, ICommandSender sender, out string response)
        {
            StringBuilder sb = new();
            if (CustomModuleManager.ModuleOwners.IsEmpty())
            {
                response = $"No registered CustomModules were found!";
                return false;
            }

            sb.AppendLine($"<size=23>CustomModule Info</size>");
#if EXILED
            foreach (KeyValuePair<CustomModuleBase, IPlugin<IConfig>> kvp in CustomModuleManager.ModuleOwners)
                sb.AppendLine($"[{kvp.Key.GetType().Name}] - {kvp.Value.Name} - v{kvp.Value.Version}");
#else
            foreach (KeyValuePair<CustomModuleBase, LabPlugin> kvp in CustomModuleManager.ModuleOwners)
                sb.AppendLine($"[{kvp.Key.GetType().Name}] - {kvp.Value.Name} - v{kvp.Value.Version}");
#endif
            sb.AppendLine($"");
            sb.AppendLine($"Total: {CustomModuleManager.CustomModules.Count}");
            response = sb.ToString();
            return true;
        }
    }
}