using UncomplicatedCustomItems.API.Features;
using System;
using CommandSystem;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;

namespace UncomplicatedCustomItems.Commands.Admin
{
    internal class Summon : Subcommand
    {
        public override string Name { get; } = "summon";

        public override string Description { get; } = "Summon an existing Custom Item";

        public override string VisibleArgs { get; } = "<Item Id>";

        public override int RequiredArgsCount { get; } = 1;

        public override string RequiredPermission { get; } = "uci.summon";

        public override string[] Aliases { get; } = ["spawn", "s"];

        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!Round.IsRoundStarted)
            {
                response = "<color=red>[⚠]</color> This command cannot be ran if the round isnt started!";
                return false;
            }

            object customItemobj = null!;
            if (Utilities.TryGetCustomItem(uint.Parse(arguments.At(0)), out var CustomItem))
            {
                customItemobj = CustomItem;
            }
            else if (APICustomItem.CustomItems.TryGetValue(uint.Parse(arguments.At(0)), out var baseCustomItem))
            {
                customItemobj = baseCustomItem;
            }
            
            if (customItemobj is CustomItem customItem)
            {
                response = $"Successfully summoned 1 '{customItem.Name}' to it's spawn point";
                Utilities.SummonCustomItem(customItem, true);
                return true;
            }
            if (customItemobj is APICustomItem baseCustom)
            {
                response = $"Successfully summoned 1 '{baseCustom.Name}' to it's spawn point";
                APICustomItem.SummonItem(baseCustom, true);
                return true;
            }

            response = "Couldnt spawn item";
            return false;
        }
    }
}