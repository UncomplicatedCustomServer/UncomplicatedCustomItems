using System.Collections.Generic;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class MERSpawn : CustomModuleBase
    {
        public override string Name =>  "MERSpawn";
        public override List<string> RequiredArguments =>
        [
            "ObjectName",
            "SchematicName",
            "ReplacePrimitive",
            "LockerSpawning",
        ];

        public bool LockerSpawning { get; set; }
        public bool ReplacePrimitive { get; set; }
        public string SchematicName { get; set; } = string.Empty;
        public string ObjectName { get; set; } = string.Empty;

        public override void OnAdded(SummonedCustomItem item)
        {
            if (CustomItem == null)
                return;

            foreach (Dictionary<object, object> args in Arguments)
            {
                if (!args.TryGetValue<bool>("LockerSpawning", out var lockerSpawning))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} LockerSpawning is not a valid Boolean!");
                    return;
                }

                if (!args.TryGetValue<bool>("ReplacePrimitive", out var replacePrimitive))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} ReplacePrimitive is not a valid float!");
                    return;
                }

                if (!args.TryGetValue<string>("SchematicName", out var schematicName))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} SchematicName is not a valid string!");
                    return;
                }

                if (!args.TryGetValue<string>("ObjectName", out var objectName))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} ObjectName is not a valid string!");
                    return;
                }

                LockerSpawning = lockerSpawning;
                ReplacePrimitive = replacePrimitive;
                SchematicName = schematicName;
                ObjectName = objectName;
            }
        }
    }
}
