using System.Collections.Generic;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class WorkstationBanHintOverride : CustomModuleBase
    {
        public override string Name => "WorkstationBanHintOverride";
        public override List<string> RequiredArguments =>
        [
            "Hint",
            "Duration"
        ];

        public string HintOverride { get; set; } = string.Empty;
        public float DurationOverride { get; set; }

        public override void OnAdded(SummonedCustomItem item)
        {
            if (CustomItem == null)
                return;

            base.OnAdded(item);
            foreach (Dictionary<object, object> args in Arguments)
            {
                if (!args.TryGetValue<string>("Hint", out var hint))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} Hint is not a valid string!");
                    return;
                }

                if (!args.TryGetValue<float>("Duration", out var duration))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} Duration is not a valid float!");
                    return;
                }

                HintOverride = hint;
                DurationOverride = duration;
            }
        }
    }
}