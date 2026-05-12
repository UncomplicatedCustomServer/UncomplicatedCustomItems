using CommandSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Interfaces;
using UnityEngine;
using UncomplicatedCustomItems.API.CustomModuleAPI;

namespace UncomplicatedCustomItems.Commands.Admin
{
    internal class Info : ISubcommand
    {
        public string Name { get; } = "info";
        public string Description { get; } = "Get info on a summoned custom item";
        public string VisibleArgs { get; } = "<Item Id>";
        public int RequiredArgsCount { get; } = 1;
        public string RequiredPermission { get; } = "uci.info";
        public string[] Aliases { get; } = ["info"];

        private string Color = string.Empty;
        private int Count = 0;

        public bool Execute(List<string> args, ICommandSender sender, out string response)
        {
            if (args.Count == 0)
            {
                response = $"usage: <Item Serial>";
                return false;
            }

            if (!ushort.TryParse(args[0], out ushort id) || !Utilities.TryGetCustomItem(id, out ICustomItem customItem))
            {
                response = $"CustomItem {args[0]} not found!";
                return false;
            }

            StringBuilder sb = new();
            sb.AppendLine($"<size=23><b>{customItem.Name} Info:</b></size>");

            AddInfoLine(sb, "<color=#00ffff>🔢</color> Id:", $"<b>{customItem.Id}</b>");
            AddInfoLine(sb, "<color=#00ff00>🔪</color> Item:", $"<b>{customItem.Item}</b>");
            AddInfoLine(sb, "<color=#00ff00>⚖</color> Scale:", $"<b>{customItem.Scale}</b>");
            AddInfoLine(sb, "<color=#00ff00>⚖</color> Weight:", $"<b>{customItem.Weight}</b>");

            if (customItem.Spawn != null)
            {
                ISpawn spawnRoot = customItem.Spawn;
                AddInfoLine(sb, "<color=#632300>󾠬</color> Does It Spawn:", spawnRoot.DoSpawn ? "Yes" : "No");
                AddInfoLine(sb, "<color=#632300>🔢</color> Spawn Count:", spawnRoot.Count.ToString());

                Count = 0;
                foreach (SummonedCustomItem summoned in SummonedCustomItem.List.Where(sci => sci.CustomItem.Id == customItem.Id))
                    Count += 1;

                AddInfoLine(sb, "<color=#632300>📏</color> Amount Spawned:", Count.ToString());

                if (spawnRoot.SpawnSettings != null && spawnRoot.SpawnSettings.Count > 0)
                {
                    int idx = 0;
                    foreach (SpawnData spawn in spawnRoot.SpawnSettings)
                    {
                        idx++;
                        AddInfoLine(sb, $"<color=#632300>📦</color> Spawn Setting #{idx}:", "");
                        AddInfoLine(sb, "    Chance:", spawn.Chance.ToString());

                        if (spawn.Rotation != Vector3.zero)
                            AddInfoLine(sb, "    Rotation:", FormatVector(spawn.Rotation));

                        if (spawn.Coords != Vector3.zero)
                            AddInfoLine(sb, "    Coords:", FormatVector(spawn.Coords));

                        if (spawn.LockerSettings != null && spawn.LockerSettings.Enable)
                        {
                            AddInfoLine(sb, "    Locker Spawn:", "");
                            AddInfoLine(sb, "        Enable:", spawn.LockerSettings.Enable.ToString());
                            AddInfoLine(sb, "        Locker Type:", spawn.LockerSettings.LockerType.ToString());
                            AddInfoLine(sb, "        Room:", spawn.LockerSettings.Room ?? "<null>");
                            AddInfoLine(sb, "        Zone:", spawn.LockerSettings.Zone.ToString());
                            AddInfoLine(sb, "        Chamber:", string.IsNullOrEmpty(spawn.LockerSettings.Chamber) ? "<none>" : spawn.LockerSettings.Chamber);
                            if (spawn.LockerSettings.Offset != Vector3.zero)
                                AddInfoLine(sb, "        Offset:", FormatVector(spawn.LockerSettings.Offset));
                        }

                        if (spawn.DynamicSpawn != null && spawn.DynamicSpawn.Count >= 1)
                        {
                            AddInfoLine(sb, "    Dynamic Spawn:", "");
                            int dI = 0;
                            foreach (DynamicSpawn dynamic in spawn.DynamicSpawn)
                            {
                                dI++;
                                AddInfoLine(sb, $"        Spawn #{dI} Room:", dynamic.Room ?? "<null>");
                                if (dynamic.Coords != Vector3.zero)
                                    AddInfoLine(sb, $"        Spawn #{dI} Coords:", FormatVector(dynamic.Coords));
                            }
                        }

                        if (spawn.Zones != null && spawn.Zones.Count >= 1)
                            AddInfoLine(sb, "    Spawn Zones:", string.Join(", ", spawn.Zones.Select(z => z.ToString())));

                        AddInfoLine(sb, "    Replace Existing Pickup:", spawn.ReplaceExistingPickup.ToString());
                        AddInfoLine(sb, "    Force Item Replace:", spawn.ForceItem.ToString());
                        AddInfoLine(sb, "    Replace Items In Pedestals:", spawn.ReplaceItemsInPedestals?.ToString() ?? "null");
                    }
                }
                else
                    AddInfoLine(sb, "<color=#632300>📦</color> Spawn Settings:", "<i>None</i>");
            }

            ProcessCustomModules(customItem, sb);

            response = sb.ToString();
            return true;
        }

        private void ProcessCustomModules(ICustomItem customItem, StringBuilder sb)
        {
            if (customItem.CustomModules == null || customItem.CustomModules.Count == 0)
                return;

            AddInfoLine(sb, "<color=#ffd700>🧩</color> Custom Modules:", "");

            foreach (KeyValuePair<CustomModuleBase, List<object>> kvp in customItem.CustomModules)
            {
                try
                {
                    CustomModuleBase module = kvp.Key;
                    List<object> argsList = kvp.Value;

                    if (module == null)
                        continue;

                    string moduleName = module.Name ?? module.GetType().Name;
                    AddInfoLine(sb, $"    <color=#ffd700>🔧</color> {moduleName}:", "");

                    if (argsList == null || argsList.Count == 0)
                    {
                        AddInfoLine(sb, "        ", "<i>No arguments</i>");
                        continue;
                    }

                    int argIndex = 0;
                    foreach (object arg in argsList)
                    {
                        ProcessModuleArgument(arg, sb, argIndex);
                        argIndex++;
                    }
                }
                catch (Exception ex)
                {
                    AddInfoLine(sb, "    <color=#ff0000>!</color> Error reading module:", ex.Message);
                }
            }
        }

        private void ProcessModuleArgument(object arg, StringBuilder sb, int argIndex)
        {
            string indent = "        ";
            string color = "#ffd700";

            if (arg is IDictionary dict)
            {
                if (dict.Count == 0)
                    return;

                AddInfoLine(sb, $"{indent}<color={color}>▪</color> Argument {argIndex}:", "");
                foreach (DictionaryEntry entry in dict)
                {
                    string rawKey = entry.Key?.ToString() ?? "<null>";
                    string propName = GetPropertyDisplayName(rawKey);
                    string formatted = FormatPropertyValue(entry.Value);

                    AddInfoLine(sb, $"{indent}    {propName}:", formatted);
                }
            }
            else if (arg is IEnumerable enumerable and not string)
            {
                IEnumerable<string> items = enumerable.Cast<object>().Select(x => x?.ToString() ?? "null");
                AddInfoLine(sb, $"{indent}<color={color}>▪</color> Argument {argIndex}:", string.Join(", ", items));
            }
            else
                AddInfoLine(sb, $"{indent}<color={color}>▪</color> Argument {argIndex}:", arg?.ToString() ?? "null");
        }

        private void ProcessSettingsCollection(string settingsName, IEnumerable collection, StringBuilder sb)
        {
            string displayName = GetDisplayName(settingsName);
            string color = GetColorForSettings(settingsName);

            AddInfoLine(sb, $"<color={color}>📂</color> {displayName}:", "");

            foreach (object item in collection)
            {
                if (item == null)
                    continue;

                ProcessSettingsObject(item, sb, "    ");
            }
        }

        private void ProcessSingleSettings(string settingsName, object settings, StringBuilder sb)
        {
            string displayName = GetDisplayName(settingsName);
            string color = GetColorForSettings(settingsName);

            AddInfoLine(sb, $"<color={color}>📂</color> {displayName}:", "");
            ProcessSettingsObject(settings, sb, "    ");
        }

        private void ProcessSettingsObject(object settings, StringBuilder sb, string indent)
        {
            if (settings == null)
                return;

            Type settingsType = settings.GetType();
            PropertyInfo[] properties = settingsType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in properties)
            {
                try
                {
                    object value = prop.GetValue(settings);
                    if (value == null)
                        continue;

                    string displayName = GetPropertyDisplayName(prop.Name);
                    string formattedValue = FormatPropertyValue(value);

                    if (prop.Name.Equals("GlowColor", StringComparison.OrdinalIgnoreCase) && value is string glowColor)
                        Color = glowColor;

                    AddInfoLine(sb, $"{indent} {displayName}:", formattedValue);
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        private void AddInfoLine(StringBuilder sb, string label, string value) =>
            sb.AppendLine($"{label.GenerateWithBuffer(40)} {value}");

        private string GetDisplayName(string settingsName) =>
            settingsName.Replace("Settings", "").Replace("_", " ");

        private string GetColorForSettings(string settingsName)
        {
            if (settingsName.Contains("ItemGlow") && !string.IsNullOrEmpty(Color))
                return Color;

            return "#bf4eb6";
        }

        private string GetPropertyDisplayName(string propertyName)
        {
            string result = System.Text.RegularExpressions.Regex.Replace(propertyName, "(\\B[A-Z])", " $1");
            return result;
        }

        private string FormatPropertyValue(object value)
        {
            if (value == null)
                return "null";

            if (value is Vector3 v)
                return FormatVector(v);

            if (value is IEnumerable enumerable and not string)
            {
                IEnumerable<string> items = enumerable.Cast<object>().Select(x => x?.ToString() ?? "null");
                return string.Join(", ", items);
            }

            return value.ToString();
        }

        private string FormatVector(Vector3 v) =>
            $"{v.x:F2}, {v.y:F2}, {v.z:F2}";
    }
}