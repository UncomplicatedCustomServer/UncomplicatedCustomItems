using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules.Enums;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using UncomplicatedCustomItems.API.Features.Manager;

namespace UncomplicatedCustomItems.API.ItemUpdater
{
    public static class CustomModuleUpdater
    {
        private static readonly IDeserializer Deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        private static readonly ISerializer Serializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        private static readonly Dictionary<CustomFlags, string> FlagToModuleMap = new()
        {
            { CustomFlags.DoNotTriggerTeslaGates, "DoNotTriggerTeslaGates" },
            { CustomFlags.LifeSteal, "LifeSteal" },
            { CustomFlags.InfiniteAmmo, "InfiniteAmmo" },
            { CustomFlags.DieOnUse, "DieOn" },
            { CustomFlags.WorkstationBan, "WorkstationBan" },
            { CustomFlags.ItemGlow, "ItemGlow" },
            { CustomFlags.EffectWhenUsed, "Effect" },
            { CustomFlags.EffectShot, "Effect" },
            { CustomFlags.EffectWhenEquiped, "Effect" },
            { CustomFlags.NoCharge, "NoCharge" },
            { CustomFlags.CustomSound, "CustomAudio" },
            { CustomFlags.ExplosiveBullets, "ExplosiveBullets" },
            { CustomFlags.ToolGun, "ToolGun" },
            { CustomFlags.SpawnItemWhenDetonated, "SpawnItemWhenDetonated" },
            { CustomFlags.Cluster, "Cluster" },
            { CustomFlags.SwitchRoleOnUse, "SwitchRoleOn" },
            { CustomFlags.DieOnDrop, "DieOn" },
            { CustomFlags.VaporizeKills, "VaporizeKills" },
            { CustomFlags.CantDrop, "CantDrop" },
            { CustomFlags.Craftable, "Craftable" },
            { CustomFlags.HealOnKill, "HealOnKill" },
            { CustomFlags.Capybara, "Capybara" },
            { CustomFlags.SingleFire, "SingleFire" },
            { CustomFlags.DistruptorTracer, "DistruptorTracer" },
            { CustomFlags.Disguise, "Disguise" },
            { CustomFlags.HumeShield, "HumeShield" },
            { CustomFlags.TantrumOnUse, "TantrumOnUse" },
            { CustomFlags.MERSpawn, "MERSpawn" },
            { CustomFlags.AmmoRegen, "AmmoRegen" },
            { CustomFlags.ItemShot, "ItemShot" },
            { CustomFlags.ChangeDisguiseOnKill, "Disguise" }
        };

        private static readonly Dictionary<CustomFlags, TriggerOn> FlagToTriggerMap = new()
        {
            { CustomFlags.DieOnUse, TriggerOn.OnUse },
            { CustomFlags.DieOnDrop, TriggerOn.OnDropped },
            { CustomFlags.EffectWhenUsed, TriggerOn.OnUse },
            { CustomFlags.EffectShot, TriggerOn.OnShot },
            { CustomFlags.EffectWhenEquiped, TriggerOn.OnChangedItem }
        };

        private static readonly Dictionary<string, string> SettingsToModuleMap = new()
        {
            { "item_glow_settings", "ItemGlow" },
            { "life_steal_settings", "LifeSteal" },
            { "effect_settings", "Effect" },
            { "audio_settings", "CustomAudio" },
            { "explosive_bullets_settings", "ExplosiveBullets" },
            { "spawn_item_when_detonated_settings", "SpawnItemWhenDetonated" },
            { "cluster_settings", "Cluster" },
            { "switch_role_on_use_settings", "SwitchRoleOn" },
            { "die_on_drop_settings", "DieOn" },
            { "cant_drop_settings", "CantDrop" },
            { "disguise_settings", "Disguise" },
            { "craftable_settings", "Craftable" },
            { "die_on_use_settings", "DieOn" },
            { "heal_on_kill_settings", "HealOnKill" },
            { "hume_shield_settings", "HumeShield" },
            { "mer_spawn_settings", "MERSpawn" },
            { "ammo_regen_settings", "AmmoRegen" },
            { "item_shot_settings", "ItemShot" }
        };

        private static readonly Dictionary<string, TriggerOn> SettingsToTriggerMap = new()
        {
            { "die_on_use_settings", TriggerOn.OnUse },
            { "die_on_drop_settings", TriggerOn.OnDropped },
        };

        private static readonly Dictionary<string, (string key, string defaultValue)> NewSettingsMap = new()
        {
            { "LifeSteal", ("percentage_based", "false") },
            { "Effect", ("Trigger", "OnUse") },
            { "Disguise", ("Trigger", "OnUse") }
        };

        public static bool TryUpdateCustomModules(string yamlPath, out Dictionary<string, object> customModules)
        {
            customModules = [];

            try
            {
                LogManager.Debug($"[CustomModuleUpdater] Processing file: {yamlPath}");

                string yamlContent = File.ReadAllText(yamlPath);
                Dictionary<string, object> yamlData = Deserializer.Deserialize<Dictionary<string, object>>(yamlContent);

                if (!ProcessYamlData(yamlData, out customModules))
                    return false;

                File.WriteAllText(yamlPath, Serializer.Serialize(yamlData));

                if (customModules.Count > 0)
                {
                    LogManager.Debug($"Successfully converted custom_flags and flag_settings to custom_modules in {yamlPath}");                    
                }
                else
                    LogManager.Debug($"Removed custom_flags and flag_settings from {yamlPath} (no custom_modules present; wrote empty mapping).");

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error($"Error converting custom flags to modules: {ex.Message}");
                return false;
            }
        }

        public static async Task<(bool Updated, Dictionary<string, object> CustomModules)> TryUpdateCustomModulesAsync(string yamlPath)
        {
            Dictionary<string, object> customModules = [];

            try
            {
                LogManager.Debug($"[CustomModuleUpdater] Processing file: {yamlPath}");

                string yamlContent = await Task.Run(() => File.ReadAllText(yamlPath)).ConfigureAwait(false);
                Dictionary<string, object> yamlData = Deserializer.Deserialize<Dictionary<string, object>>(yamlContent);

                if (!ProcessYamlData(yamlData, out customModules))
                    return (false, customModules);

                await Task.Run(() => File.WriteAllText(yamlPath, Serializer.Serialize(yamlData))).ConfigureAwait(false);

                if (customModules.Count > 0)
                {
                    LogManager.Debug($"Successfully converted custom_flags and flag_settings to custom_modules in {yamlPath}");                    
                }
                else
                    LogManager.Debug($"Removed custom_flags and flag_settings from {yamlPath} (no custom_modules present; wrote empty mapping).");

                return (true, customModules);
            }
            catch (Exception ex)
            {
                LogManager.Error($"Error converting custom flags to modules: {ex.Message}");
                return (false, customModules);
            }
        }

        private static bool ProcessYamlData(Dictionary<string, object> yamlData, out Dictionary<string, object> customModules)
        {
            customModules = [];

            if (yamlData.TryGetValue("custom_modules", out object existingModulesObj))
            {
                if (existingModulesObj is IDictionary existingDict && existingDict.Count > 0)
                {
                    return false;
                }
            }

            HashSet<CustomFlags> parsedFlags = [];
            if (yamlData.TryGetValue("custom_flags", out object customFlagsObj))
            {
                if (customFlagsObj is IEnumerable flagList && customFlagsObj is not string)
                {
                    foreach (object item in flagList)
                    {
                        if (item == null)
                            continue;

                        string s = item.ToString();
                        if (Enum.TryParse(s, true, out CustomFlags f) && f != CustomFlags.None)
                            parsedFlags.Add(f);
                    }
                }
                else
                {
                    string s = customFlagsObj?.ToString() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(s) && !string.Equals(s, "None", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = s.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries);
                        foreach (string part in parts)
                        {
                            string trimmed = part.Trim();
                            if (Enum.TryParse<CustomFlags>(trimmed, true, out CustomFlags f) && f != CustomFlags.None)
                                parsedFlags.Add(f);
                        }
                    }
                }
            }

            HashSet<string> enabledModules = new(StringComparer.OrdinalIgnoreCase);
            TriggerOn dieOnTriggers = 0;
            Dictionary<string, object> dieOnSettings = [];

            foreach (CustomFlags flag in parsedFlags)
            {
                if (!FlagToModuleMap.TryGetValue(flag, out string moduleName))
                    continue;

                enabledModules.Add(moduleName);

                if (FlagToTriggerMap.TryGetValue(flag, out TriggerOn trigger))
                {
                    if (moduleName == "DieOn")
                    {
                        dieOnTriggers |= trigger;
                    }
                    else
                    {
                        if (!customModules.TryGetValue(moduleName, out object value))
                        {
                            value = new List<Dictionary<string, object>>();
                            customModules[moduleName] = value;
                        }

                        List<Dictionary<string, object>> moduleList = (List<Dictionary<string, object>>)value;
                        Dictionary<string, object> existingEntry = moduleList.FirstOrDefault(m => m.ContainsKey("trigger"));

                        if (existingEntry != null)
                        {
                            TriggerOn existingTrigger = (TriggerOn)existingEntry["trigger"];
                            existingEntry["trigger"] = existingTrigger | trigger;
                        }
                        else
                        {
                            moduleList.Add(new Dictionary<string, object> { { "trigger", trigger } });
                        }
                    }
                }
                else
                {
                    if (!customModules.ContainsKey(moduleName))
                        customModules[moduleName] = new List<Dictionary<string, object>>();
                }
            }

            if (yamlData.TryGetValue("flag_settings", out object flagSettingsObj))
            {
                if (flagSettingsObj is IDictionary flagSettings)
                {
                    foreach (DictionaryEntry kvp in flagSettings)
                    {
                        string? settingKey = kvp.Key?.ToString();
                        if (settingKey == null || !SettingsToModuleMap.TryGetValue(settingKey, out string moduleName))
                            continue;

                        if (!enabledModules.Contains(moduleName))
                            continue;

                        if (SettingsToTriggerMap.TryGetValue(settingKey, out TriggerOn trigger) && moduleName == "DieOn")
                        {
                            dieOnTriggers |= trigger;
                            if (kvp.Value is IList settingsList && settingsList.Count > 0 && settingsList[0] is IDictionary settings)
                                dieOnSettings = ConvertDictionary(settings);

                            continue;
                        }

                        if (kvp.Value is IList settingsAsList)
                        {
                            List<Dictionary<string, object>> convertedList = [];

                            foreach (object item in settingsAsList)
                            {
                                Dictionary<string, object> converted = item is IDictionary d ? ConvertDictionary(d) : new Dictionary<string, object> { { "value", item } };
                                if (NewSettingsMap.TryGetValue(moduleName, out (string key, string defaultValue) newSetting))
                                {
                                    Dictionary<string, object> wrapped = new()
                                    {
                                        { newSetting.key, newSetting.defaultValue }
                                    };

                                    foreach (KeyValuePair<string, object> entry in converted)
                                        wrapped[entry.Key] = entry.Value;

                                    convertedList.Add(wrapped);
                                }
                                else
                                    convertedList.Add(converted);
                            }

                            customModules[moduleName] = convertedList;
                        }
                        else if (kvp.Value is IDictionary singleDict)
                        {
                            Dictionary<string, object> converted = ConvertDictionary(singleDict);
                            if (NewSettingsMap.TryGetValue(moduleName, out var newSetting))
                            {
                                Dictionary<string, object> wrapped = new()
                                {
                                    { newSetting.key, newSetting.defaultValue }
                                };

                                foreach (KeyValuePair<string, object> entry in converted)
                                {
                                    wrapped[entry.Key] = entry.Value;                                    
                                }

                                customModules[moduleName] = new List<Dictionary<string, object>> { wrapped };
                            }
                            else
                                customModules[moduleName] = new List<Dictionary<string, object>> { converted };
                        }
                        else
                        {
                            if (NewSettingsMap.TryGetValue(moduleName, out var newSetting))
                            {
                                Dictionary<string, object> wrapped = new()
                                {
                                    { newSetting.key, newSetting.defaultValue },
                                    { "value", kvp.Value }
                                };

                                customModules[moduleName] = new List<Dictionary<string, object>> { wrapped };
                            }
                            else
                                customModules[moduleName] = kvp.Value;
                        }
                    }
                }
            }

            if (dieOnTriggers != 0 && enabledModules.Contains("DieOn"))
            {
                List<Dictionary<string, object>> dieOnList = [];
                Dictionary<string, object> dieOnEntry = new()
                {
                    { "trigger", dieOnTriggers.ToString() }
                };

                if (dieOnSettings != null)
                {
                    foreach (KeyValuePair<string, object> kvp in dieOnSettings)
                        dieOnEntry[kvp.Key] = kvp.Value;
                }

                dieOnList.Add(dieOnEntry);
                customModules["DieOn"] = dieOnList;
            }

            if (yamlData.ContainsKey("flag_settings"))
                yamlData.Remove("flag_settings");

            if (yamlData.ContainsKey("custom_flags"))
                yamlData.Remove("custom_flags");

            if (customModules.Count > 0)
            {
                yamlData["custom_modules"] = customModules;
            }
            else
            {
                if (!yamlData.ContainsKey("custom_modules"))
                {
                    yamlData["custom_modules"] = new Dictionary<string, object>();
                }
                else
                {
                    if (yamlData["custom_modules"] is IDictionary existingDict && existingDict.Count == 0)
                        yamlData["custom_modules"] = new Dictionary<string, object>();
                }
            }

            return true;
        }

        private static Dictionary<string, object> ConvertDictionary(IDictionary source)
        {
            Dictionary<string, object> result = [];
            foreach (DictionaryEntry kvp in source)
            {
                if (kvp.Key != null)
                    result[kvp.Key.ToString()] = kvp.Value;
            }

            return result;
        }
    }
}