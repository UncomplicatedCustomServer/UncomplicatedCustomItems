using System.Collections.Generic;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class EquipHintOverride : CustomModuleBase
    {
        public override string Name => "EquipHintOverride";
        public override List<string> RequiredArguments =>
        [
            "Hint",
            "Duration"
        ];

        public string Hint { get; set; }
        public uint Duration { get; set; }

        public override void OnAdded(SummonedCustomItem item)
        {
            foreach (Dictionary<object, object> args in Arguments)
            {
                if (!args.TryGetValue<string>("Hint", out var hint))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} Hint is not a valid string!");
                    return;
                }

                if (!args.TryGetValue<uint>("Duration", out var dur))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} Duration is not a valid uint!");
                    return;
                }

                Hint = hint;
                Duration = dur;
            }
        }
    }
}
