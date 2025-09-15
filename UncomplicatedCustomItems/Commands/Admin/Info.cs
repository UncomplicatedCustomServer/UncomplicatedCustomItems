using CommandSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Interfaces;
using UnityEngine;

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

        private string Color = null;
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

            if (customItem.Spawn is not null)
            {
                AddInfoLine(sb, "<color=#632300>󾠬</color> Does It Spawn:", string.Join(", ", customItem.Spawn.DoSpawn));
                
                foreach (SummonedCustomItem SummonedCustomItem in SummonedCustomItem.List)
                {
                    if (SummonedCustomItem.CustomItem.Id == customItem.Id)
                    {
                        Count += 1;
                    }
                }
                AddInfoLine(sb, "<color=#632300>📏</color> Amount Spawned:", Count.ToString());
                foreach (SpawnData spawn in customItem.Spawn.SpawnSettings)
                {
                    if (spawn.Coords != Vector3.zero)
                        AddInfoLine(sb, "<color=#632300>󾠬</color> Spawn Coords:", string.Join(", ", spawn?.Coords));
                    else if (spawn.DynamicSpawn.Count >= 1)
                    {
                        AddInfoLine(sb, "<color=#632300>📂</color> Dynamic Spawn:", "");
                        foreach (DynamicSpawn DynamicSpawn in spawn.DynamicSpawn)
                        {
                            AddInfoLine(sb, "    <color=#632300>🎦</color> Spawn Rooms:", string.Join(", ", DynamicSpawn.Room));
                            AddInfoLine(sb, "    <color=#632300>󾠬</color> Spawn Coords:", string.Join(", ", DynamicSpawn.Coords));
                        }
                    }
                    else if (spawn.Zones.Count >= 1)
                        AddInfoLine(sb, "<color=#632300>🇿</color> Spawn Zones:", string.Join(", ", spawn?.Zones));
                }
            }

            ProcessCustomFlags(customItem, sb);

            if (customItem.CustomFlags.HasValue)
                AddInfoLine(sb, "<color=#bf4eb6>📄</color> Custom flags:", customItem.CustomFlags.ToString());

            response = sb.ToString();
            return true;
        }

        private void ProcessCustomFlags(ICustomItem customItem, StringBuilder sb)
        {
            if (customItem.FlagSettings == null || !customItem.CustomFlags.HasValue)
                return;

            Type flagSettingsType = customItem.FlagSettings.GetType();
            PropertyInfo[] settingsProperties = flagSettingsType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            List<CustomFlags> activeFlags = GetActiveCustomFlags(customItem.CustomFlags.Value);

            foreach (PropertyInfo settingsProperty in settingsProperties)
            {
                object settingsValue = settingsProperty.GetValue(customItem.FlagSettings);
                if (settingsValue == null) continue;

                string flagName = GetFlagNameFromSettingsProperty(settingsProperty.Name);
                if (!IsRelevantForActiveFlags(flagName, activeFlags)) continue;

                if (settingsValue is IEnumerable enumerable && settingsValue is not string)
                {
                    ProcessSettingsCollection(settingsProperty.Name, enumerable, sb);
                }
                else
                {
                    ProcessSingleSettings(settingsProperty.Name, settingsValue, sb);
                }
            }
        }

        private List<CustomFlags> GetActiveCustomFlags(CustomFlags flags)
        {
            List<CustomFlags> activeFlags = [];
            foreach (CustomFlags flag in Enum.GetValues(typeof(CustomFlags)))
            {
                if (flag != CustomFlags.None && flags.HasFlag(flag))
                {
                    activeFlags.Add(flag);
                }
            }
            return activeFlags;
        }

        private string GetFlagNameFromSettingsProperty(string propertyName)
        {
            if (propertyName.EndsWith("Settings"))
                return propertyName.Substring(0, propertyName.Length - 8);
            return propertyName;
        }

        private bool IsRelevantForActiveFlags(string flagName, List<CustomFlags> activeFlags)
        {
            Dictionary<string, CustomFlags[]> specialMappings = new()
            {
                { "Effect", new[] { CustomFlags.EffectShot, CustomFlags.EffectWhenEquiped, CustomFlags.EffectWhenUsed } },
                { "Audio", new[] { CustomFlags.CustomSound } },
                { "ItemGlow", new[] { CustomFlags.ItemGlow } },
                { "CantDrop", new[] { CustomFlags.CantDrop } },
                { "Cluster", new[] { CustomFlags.Cluster } },
                { "DieOnDrop", new[] { CustomFlags.DieOnDrop } },
                { "ExplosiveBullets", new[] { CustomFlags.ExplosiveBullets } },
                { "LifeSteal", new[] { CustomFlags.LifeSteal } },
                { "SpawnItemWhenDetonated", new[] { CustomFlags.SpawnItemWhenDetonated } },
                { "SwitchRoleOnUse", new[] { CustomFlags.SwitchRoleOnUse } },
                { "Craftable", new[] { CustomFlags.Craftable } }
            };

            if (specialMappings.ContainsKey(flagName))
            {
                return specialMappings[flagName].Any(activeFlags.Contains);
            }

            try
            {
                CustomFlags flag = (CustomFlags)Enum.Parse(typeof(CustomFlags), flagName, true);
                return activeFlags.Contains(flag);
            }
            catch
            {
                return false;
            }
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
                ProcessSettingsObject(item, sb, color, "    ");
            }
        }

        private void ProcessSingleSettings(string settingsName, object settings, StringBuilder sb)
        {
            string displayName = GetDisplayName(settingsName);
            string color = GetColorForSettings(settingsName);
            
            AddInfoLine(sb, $"<color={color}>📂</color> {displayName}:", "");
            ProcessSettingsObject(settings, sb, color, "    ");
        }

        private void ProcessSettingsObject(object settings, StringBuilder sb, string color, string indent)
        {
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
                    string icon = GetIconForProperty(prop.Name);
                    string formattedValue = FormatPropertyValue(value);

                    if (prop.Name.Equals("GlowColor", StringComparison.OrdinalIgnoreCase) && value is string glowColor)
                    {
                        Color = glowColor;
                        color = glowColor;
                    }

                    AddInfoLine(sb, $"{indent}<color={color}>{icon}</color> {displayName}:", formattedValue);
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        private void AddInfoLine(StringBuilder sb, string label, string value)
        {
            sb.AppendLine($"{label.GenerateWithBuffer(40)} {value}");
        }

        private string GetDisplayName(string settingsName)
        {
            return settingsName.Replace("Settings", "")
                              .Replace("_", " ");
        }

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

        private string GetIconForProperty(string propertyName)
        {
            var iconMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "AudibleDistance", "📏" },
                { "AudioPath", "📃" },
                { "SoundVolume", "🔉" },
                { "Volume", "🔉" },
                { "HintOrBroadcast", "💬" },
                { "Message", "💬" },
                { "Duration", "🕛" },
                { "DeathMessage", "💬" },
                { "Vaporize", "💦" },
                { "AmountToSpawn", "#" },
                { "Amount", "#" },
                { "FuseTime", "🕛" },
                { "ItemToSpawn", "🔫" },
                { "ItemId", "🔫" },
                { "ScpDamageMultiplier", "💥" },
                { "DamageRadius", "💥" },
                { "EffectEvent", "💻" },
                { "Effect", "💉" },
                { "EffectIntensity", "📶" },
                { "EffectDuration", "🕛" },
                { "GlowColor", "🌟" },
                { "LifeStealAmount", "💊" },
                { "LifeStealPercentage", "💊" },
                { "Chance", "🎲" },
                { "Pickupable", "🛠️" },
                { "TimeTillDespawn", "🕛" },
                { "Delay", "🔂" },
                { "KeepLocation", "🔒" },
                { "RoleId", "🆔" },
                { "RoleType", "🚶" },
                { "SpawnFlags", "󾓦" },
                { "KnobSetting", "🔒" },
                { "OriginalItem", "🆔" }
            };

            return iconMap.TryGetValue(propertyName, out string icon) ? icon : "📋";
        }

        private string FormatPropertyValue(object value)
        {
            if (value == null) return "null";
            
            if (value is IEnumerable enumerable && !(value is string))
            {
                var items = enumerable.Cast<object>().Select(x => x?.ToString() ?? "null");
                return string.Join(", ", items);
            }
            
            return value.ToString();
        }
    }
}