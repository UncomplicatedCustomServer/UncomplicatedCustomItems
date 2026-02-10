using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules.Enums;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.API.ItemUpdater
{
    public static class CustomModuleUpdater
    {
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
            { CustomFlags.CustomSound, "CustomSound" },
            { CustomFlags.ExplosiveBullets, "ExplosiveBullets" },
            { CustomFlags.ToolGun, "ToolGun" },
            { CustomFlags.SpawnItemWhenDetonated, "SpawnItemWhenDetonated" },
            { CustomFlags.Cluster, "Cluster" },
            { CustomFlags.SwitchRoleOnUse, "SwitchRoleOnUse" },
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
            { "audio_settings", "CustomSound" },
            { "explosive_bullets_settings", "ExplosiveBullets" },
            { "spawn_item_when_detonated_settings", "SpawnItemWhenDetonated" },
            { "cluster_settings", "Cluster" },
            { "switch_role_on_use_settings", "SwitchRoleOnUse" },
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
                IDeserializer deserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
                Dictionary<string, object> yamlData = deserializer.Deserialize<Dictionary<string, object>>(yamlContent);

                if (yamlData.TryGetValue("custom_modules", out object existingModulesObj))
                {
                    if (existingModulesObj is Dictionary<object, object> moduleDict && moduleDict.Count > 0)
                    {
                        LogManager.Info($"Custom modules already exist in {yamlPath}, skipping conversion.");
                        return false;
                    }
                }

                HashSet<CustomFlags> parsedFlags = [];
                if (yamlData.TryGetValue("custom_flags", out object customFlagsObj))
                {
                    if (customFlagsObj is List<object> flagList)
                    {
                        foreach (object item in flagList)
                        {
                            if (item == null)
                                continue;

                            string s = item.ToString();
                            if (Enum.TryParse<CustomFlags>(s, true, out CustomFlags f) && f != CustomFlags.None)
                                parsedFlags.Add(f);
                        }
                    }
                    else
                    {
                        string s = customFlagsObj?.ToString();
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
                foreach (CustomFlags flag in parsedFlags)
                {
                    if (FlagToModuleMap.TryGetValue(flag, out string moduleName))
                        enabledModules.Add(moduleName);
                }

                TriggerOn dieOnTriggers = 0;
                Dictionary<string, object> dieOnSettings = null;

                foreach (CustomFlags flag in parsedFlags)
                {
                    if (!FlagToModuleMap.TryGetValue(flag, out string moduleName))
                        continue;

                    if (FlagToTriggerMap.TryGetValue(flag, out TriggerOn trigger))
                    {
                        if (moduleName == "DieOn")
                        {
                            dieOnTriggers |= trigger;
                        }
                        else
                        {
                            if (!customModules.ContainsKey(moduleName))
                                customModules[moduleName] = new List<Dictionary<string, object>>();

                            List<Dictionary<string, object>> moduleList = (List<Dictionary<string, object>>)customModules[moduleName];
                            Dictionary<string, object> existingEntry = moduleList.FirstOrDefault(m => m.ContainsKey("trigger"));

                            if (existingEntry != null)
                            {
                                TriggerOn existingTrigger = (TriggerOn)existingEntry["trigger"];
                                existingEntry["trigger"] = existingTrigger | trigger;
                            }
                            else
                                moduleList.Add(new Dictionary<string, object> { { "trigger", trigger } });
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
                    if (flagSettingsObj is Dictionary<object, object> flagSettings)
                    {
                        foreach (KeyValuePair<object, object> kvp in flagSettings)
                        {
                            string settingKey = kvp.Key.ToString();
                            if (!SettingsToModuleMap.TryGetValue(settingKey, out string moduleName))
                                continue;

                            if (!enabledModules.Contains(moduleName))
                                continue;

                            if (SettingsToTriggerMap.TryGetValue(settingKey, out TriggerOn trigger) && moduleName == "DieOn")
                            {
                                dieOnTriggers |= trigger;
                                if (kvp.Value is List<object> settingsList && settingsList.Count > 0 && settingsList[0] is Dictionary<object, object> settings)
                                    dieOnSettings = ConvertDictionary(settings);

                                continue;
                            }

                            if (kvp.Value is List<object> settingsAsList)
                            {
                                List<Dictionary<string, object>> convertedList = [];

                                foreach (object item in settingsAsList)
                                {
                                    Dictionary<string, object> converted = item is Dictionary<object, object> d ? ConvertDictionary(d) : new Dictionary<string, object> { { "value", item } };
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
                            else if (kvp.Value is Dictionary<object, object> singleDict)
                            {
                                Dictionary<string, object> converted = ConvertDictionary(singleDict);
                                if (NewSettingsMap.TryGetValue(moduleName, out var newSetting))
                                {
                                    Dictionary<string, object> wrapped = new()
                                    {
                                        { newSetting.key, newSetting.defaultValue }
                                    };

                                    foreach (KeyValuePair<string, object> entry in converted)
                                        wrapped[entry.Key] = entry.Value;

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
                        if (yamlData["custom_modules"] is Dictionary<object, object> existingDict && existingDict.Count == 0)
                            yamlData["custom_modules"] = new Dictionary<string, object>();
                    }
                }

                ISerializer serializer = new SerializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
                string updatedYaml = serializer.Serialize(yamlData);
                File.WriteAllText(yamlPath, updatedYaml);

                if (customModules.Count > 0)
                    LogManager.Debug($"Successfully converted custom_flags and flag_settings to custom_modules in {yamlPath}");
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

        private static Dictionary<string, object> ConvertDictionary(Dictionary<object, object> source)
        {
            Dictionary<string, object> result = [];
            foreach (KeyValuePair<object, object> kvp in source)
                result[kvp.Key.ToString()] = kvp.Value;

            return result;
        }
    }
}