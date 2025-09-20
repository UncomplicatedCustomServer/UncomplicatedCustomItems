using CommandSystem;
using LabApi.Features.Wrappers;
using System.Collections.Generic;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.Commands.Admin
{
    internal class Summon : ISubcommand
    {
        public string Name { get; } = "summon";

        public string Description { get; } = "Summon an existing Custom Item";

        public string VisibleArgs { get; } = "<Item Id>";

        public int RequiredArgsCount { get; } = 1;

        public string RequiredPermission { get; } = "uci.summon";

        public string[] Aliases { get; } = ["spawn", "s"];

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            if (!Round.IsRoundStarted)
            {
                response = "<color=red>[⚠]</color> This command cannot be ran if the round isnt started!";
                return false;
            }

            object customItemobj = null;
            if (Utilities.TryGetCustomItem(uint.Parse(arguments[0]), out var iCustomItem))
            {
                customItemobj = iCustomItem;
            }
            else if (BaseCustomItem.CustomItems.TryGetValue(uint.Parse(arguments[0]), out var baseCustomItem))
            {
                customItemobj = baseCustomItem;
            }
            if (customItemobj is ICustomItem customItem)
            {
                response = $"Successfully summoned 1 '{customItem.Name}' to it's spawn point";
                Utilities.SummonCustomItem(customItem);
                return true;
            }
            if (customItemobj is BaseCustomItem baseCustom)
            {
                response = $"Successfully summoned 1 '{baseCustom.Name}' to it's spawn point";
                BaseCustomItem.SummonItem(baseCustom);
                return true;
            }

            response = "Couldnt spawn item";
            return false;
        }
    }
}