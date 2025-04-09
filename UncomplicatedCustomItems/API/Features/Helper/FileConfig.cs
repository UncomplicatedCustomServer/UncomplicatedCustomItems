using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Extensions;
using Exiled.Loader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UncomplicatedCustomItems.Enums;
using UnityEngine;
using UncomplicatedCustomItems.Interfaces;
using YamlDotNet.Core;

namespace UncomplicatedCustomItems.API.Features.Helper
{
    internal class FileConfig
    {
        public static readonly List<YAMLCustomItem> _examples =
        [
            new()
            {
                CustomFlags = CustomFlags.None,
                CustomData = YAMLCaster.Encode(new ItemData()
                {
                    Event = ItemEvents.Command,
                    Command = "/SERVER_EVENT DETONATION_INSTANT",
                    ConsoleMessage = "UHUHUHUH!"
                })
            },
            new()
            {
                Id = 2,
                Name = "FunnyGun",
                Description = "A magic weapon that has a shotgun-like bullet spread",
                BadgeName = "FunnyGun",
                BadgeColor = "pumpkin",
                Item = ItemType.GunFRMG0,
                CustomItemType = CustomItemType.Weapon,
                Scale = Vector3.one,
                Spawn = new(),
                CustomFlags = CustomFlags.InfiniteAmmo,
                FlagSettings = new(),
                CustomData = YAMLCaster.Encode(new WeaponData())
            },
            new()
            {
                Id = 3,
                Name = "Titanium Armor",
                Description = "A super heavy armor",
                BadgeName = "Armor",
                BadgeColor = "pumpkin",
                Item = ItemType.ArmorHeavy,
                CustomItemType = CustomItemType.Armor,
                Scale = Vector3.one,
                Spawn = new(),
                CustomFlags = CustomFlags.None,
                FlagSettings = new(),
                CustomData = YAMLCaster.Encode(new ArmorData()
                {
                    HeadProtection = 150,
                    BodyProtection = 200,
                    RemoveExcessOnDrop = false,
                    StaminaUseMultiplier = 2
                })
            },
            new()
            {
                Id = 4,
                Name = "Incredible beautiful keycard",
                Description = "UWU owo keycard",
                BadgeName = "Keycard",
                BadgeColor = "pumpkin",
                Item = ItemType.KeycardJanitor,
                CustomItemType = CustomItemType.Keycard,
                Scale = Vector3.one,
                Spawn = new(),
                CustomFlags = CustomFlags.None,
                FlagSettings = new(),
                CustomData = YAMLCaster.Encode(new KeycardData()
                {
                    Permissions = KeycardPermissions.AlphaWarhead | KeycardPermissions.Checkpoints
                })
            },
            new()
            {
                Id = 5,
                Name = "My favourite grenade",
                Description = "Throw it my friend :)",
                BadgeName = "Grenade",
                BadgeColor = "pumpkin",
                Item = ItemType.GrenadeHE,
                CustomItemType = CustomItemType.ExplosiveGrenade,
                Scale = Vector3.one,
                Spawn = new(),
                CustomFlags = CustomFlags.None,
                FlagSettings = new(),
                CustomData = YAMLCaster.Encode(new ExplosiveGrenadeData()
                {
                    MaxRadius = 250f
                })
            },
            new()
            {
                Id = 6,
                Name = "Blinder",
                Description = "Make every people in the facility blind",
                BadgeName = "Blinder",
                BadgeColor = "pumpkin",
                Item = ItemType.GrenadeFlash,
                CustomItemType = CustomItemType.FlashGrenade,
                Scale = Vector3.one,
                Spawn = new(),
                CustomFlags = CustomFlags.None,
                FlagSettings = new(),
                CustomData = YAMLCaster.Encode(new FlashGrenadeData()
                {
                    AdditionalBlindedEffect = 250f
                })
            },
            new()
            {
                Id = 7,
                Name = "Overpowered medikit",
                Description = "This medikit will heal you 100%",
                BadgeName = "Medikit",
                BadgeColor = "pumpkin",
                Item = ItemType.Medkit,
                CustomItemType = CustomItemType.Medikit,
                Scale = new(2, 2, 2),
                Spawn = new(),
                CustomFlags = CustomFlags.None,
                FlagSettings = new(),
                CustomData = YAMLCaster.Encode(new MedikitData()
                {
                    Health = 250f
                })
            },
            new()
            {
                Id = 8,
                Name = "Really fast painkillers",
                Description = "This painkillers regenerate lots of health within seconds but you'll have to wait...",
                BadgeName = "Painkillers",
                BadgeColor = "pumpkin",
                Item = ItemType.Painkillers,
                CustomItemType = CustomItemType.Painkillers,
                Scale = new(5, 5, 5),
                Spawn = new(),
                CustomFlags = CustomFlags.None,
                FlagSettings = new(),
                CustomData = YAMLCaster.Encode(new PainkillersData()
                {
                    TickHeal = 1f,
                    TickTime = 0.1f,
                    TimeBeforeStartHealing = 10f,
                    TotalHealing = 50f
                })
            },
            new()
            {
                Id = 9,
                Name = "Just a Jailbird",
                Description = "Just a Jailbird",
                BadgeName = "Jailbird",
                BadgeColor = "pumpkin",
                Item = ItemType.Jailbird,
                CustomItemType = CustomItemType.Jailbird,
                Scale = new(1, 1, 1),
                Spawn = new(),
                CustomFlags = CustomFlags.None,
                FlagSettings = new(),
                CustomData = YAMLCaster.Encode(new JailbirdData())
            },
            new()
            {
                Id = 10,
                Name = "Shitty adrenaline",
                Description = "This adrenaline just give you 10AHP",
                Item = ItemType.Adrenaline,
                CustomItemType = CustomItemType.Adrenaline,
                Scale = new(1, 1, 1),
                CustomFlags = CustomFlags.None,
                FlagSettings = new(),
                CustomData = YAMLCaster.Encode(new AdrenalineData()
                {
                    Amount = 10,
                    Decay = 0.01f,
                    Persistant = true,
                    Sustain = 1000
                })
            },
            new()
            {
                Id = 11,
                Name = "SCP-500",
                Description = "SCP-500",
                BadgeName = "SCP-500",
                BadgeColor = "pumpkin",
                Item = ItemType.SCP500,
                CustomItemType = CustomItemType.SCPItem,
                Scale = Vector3.one,
                Spawn = new(),
                CustomFlags = CustomFlags.None,
                FlagSettings = new(),
                CustomData = YAMLCaster.Encode(new SCP500Data())
            },
            new()
            {
                Id = 12,
                Name = "SCP-207",
                Description = "SCP-207",
                BadgeName = "SCP-207",
                BadgeColor = "pumpkin",
                Item = ItemType.SCP207,
                CustomItemType = CustomItemType.SCPItem,
                Scale = Vector3.one,
                Spawn = new(),
                CustomFlags = CustomFlags.None,
                FlagSettings = new(),
                CustomData = YAMLCaster.Encode(new SCP207Data())
            },
            new()
            {
                Id = 13,
                Name = "SCP-018",
                Description = "SCP-018",
                BadgeName = "SCP-018",
                BadgeColor = "pumpkin",
                Item = ItemType.SCP018,
                CustomItemType = CustomItemType.SCPItem,
                Scale = Vector3.one,
                Spawn = new(),
                CustomFlags = CustomFlags.None,
                FlagSettings = new(),
                CustomData = YAMLCaster.Encode(new SCP018Data())
            },
            new()
            {
                Id = 14,
                Name = "SCP2176",
                Description = "SCP2176",
                BadgeName = "SCP2176",
                BadgeColor = "pumpkin",
                Item = ItemType.SCP2176,
                CustomItemType = CustomItemType.SCPItem,
                Scale = new(2, 2, 2),
                Spawn = new(),
                CustomFlags = CustomFlags.None,
                FlagSettings = new(),
                CustomData = YAMLCaster.Encode(new SCP2176Data())
            },
            new()
            {
                Id = 15,
                Name = "SCP244",
                Description = "SCP244",
                BadgeName = "SCP244",
                BadgeColor = "pumpkin",
                Item = ItemType.SCP244a,
                CustomItemType = CustomItemType.SCPItem,
                Scale = new(5, 5, 5),
                Spawn = new(),
                CustomFlags = CustomFlags.None,
                FlagSettings = new(),
                CustomData = YAMLCaster.Encode(new SCP244Data())
            },
            new()
            {
                Id = 16,
                Name = "SCP244",
                Description = "SCP244",
                BadgeName = "SCP244",
                BadgeColor = "pumpkin",
                Item = ItemType.SCP244b,
                CustomItemType = CustomItemType.SCPItem,
                Scale = new(5, 5, 5),
                Spawn = new(),
                CustomFlags = CustomFlags.None,
                FlagSettings = new(),
                CustomData = YAMLCaster.Encode(new SCP244Data())
            },
            new()
            {
                Id = 17,
                Name = "SCP1853",
                Description = "SCP1853",
                BadgeName = "SCP1853",
                BadgeColor = "pumpkin",
                Item = ItemType.SCP1853,
                CustomItemType = CustomItemType.SCPItem,
                Scale = new(1, 1, 1),
                Spawn = new(),
                CustomFlags = CustomFlags.None,
                FlagSettings = new(),
                CustomData = YAMLCaster.Encode(new SCP1853Data())
            },
            new()
            {
                Id = 18,
                Name = "SCP1576",
                Description = "SCP1576",
                Item = ItemType.SCP1576,
                CustomItemType = CustomItemType.SCPItem,
                Scale = Vector3.one,
                CustomData = YAMLCaster.Encode(new SCP1576Data())
            }
        ];

        public uint NewId = new();

        public void GenerateCustomItem(uint id, string name, ItemType itemType, CustomItemType customType, string description)
        {
            Dictionary<string, string> customData = [];
            
            if (itemType == ItemType.SCP244a && customType == CustomItemType.SCPItem)
            {
                SCP244Data Data = new SCP244Data();
                customData = YAMLCaster.Encode(Data);
            }
            else if (itemType == ItemType.SCP244b && customType == CustomItemType.SCPItem)
            {
                SCP244Data Data = new SCP244Data();
                customData = YAMLCaster.Encode(Data);
            }
            else if (itemType == ItemType.SCP2176 && customType == CustomItemType.SCPItem)
            {
                SCP2176Data Data = new SCP2176Data();
                customData = YAMLCaster.Encode(Data);
            }
            else if (itemType == ItemType.SCP018 && customType == CustomItemType.SCPItem)
            {
                SCP018Data Data = new SCP018Data();
                customData = YAMLCaster.Encode(Data);
            }
            else if (itemType == ItemType.SCP500 && customType == CustomItemType.SCPItem)
            {
                SCP500Data Data = new SCP500Data();
                customData = YAMLCaster.Encode(Data);
            }
            else if (itemType == ItemType.SCP207 && customType == CustomItemType.SCPItem)
            {
                SCP207Data Data = new SCP207Data();
                customData = YAMLCaster.Encode(Data);
            }
            else if (itemType == ItemType.AntiSCP207 &&  customType == CustomItemType.SCPItem)
            {
                SCP207Data Data = new SCP207Data();
                customData = YAMLCaster.Encode(Data);
            }
            else if (itemType == ItemType.SCP1853 && customType == CustomItemType.SCPItem)
            {
                SCP1853Data Data = new SCP1853Data();
                customData = YAMLCaster.Encode(Data);
            }
            else if (itemType == ItemType.SCP1576 && customType == CustomItemType.SCPItem)
            {
                SCP1576Data Data = new SCP1576Data();
                customData = YAMLCaster.Encode(Data);
            }
            else if (ItemExtensions.GetCategory(itemType) == ItemCategory.Firearm && customType == CustomItemType.Weapon)
            {
                WeaponData Data = new WeaponData();
                customData = YAMLCaster.Encode(Data);
            }
            else if (ItemExtensions.GetCategory(itemType) == ItemCategory.Keycard && customType == CustomItemType.Keycard)
            {
                KeycardData Data = new KeycardData();
                customData = YAMLCaster.Encode(Data);
            }
            else if (ItemExtensions.GetCategory(itemType) == ItemCategory.Armor && customType == CustomItemType.Armor)
            {
                ArmorData Data = new ArmorData();
                customData = YAMLCaster.Encode(Data);
            }
            else if (itemType == ItemType.GrenadeHE && customType == CustomItemType.ExplosiveGrenade)
            {
                ExplosiveGrenadeData Data = new ExplosiveGrenadeData();
                customData = YAMLCaster.Encode(Data);
            }
            else if (itemType == ItemType.GrenadeFlash && customType == CustomItemType.FlashGrenade)
            {
                FlashGrenadeData Data = new FlashGrenadeData();
                customData = YAMLCaster.Encode(Data);
            }
            else if (itemType == ItemType.Jailbird && customType == CustomItemType.Jailbird)
            {
                JailbirdData Data = new JailbirdData();
                customData = YAMLCaster.Encode(Data);
            }
            else if (itemType == ItemType.Medkit && customType == CustomItemType.Medikit)
            {
                MedikitData Data = new MedikitData();
                customData = YAMLCaster.Encode(Data);
            }
            else if (itemType == ItemType.Painkillers && customType == CustomItemType.Painkillers)
            {
                PainkillersData Data = new PainkillersData();
                customData = YAMLCaster.Encode(Data);
            }
            else if (itemType == ItemType.Adrenaline && customType == CustomItemType.Adrenaline)
            {
                AdrenalineData Data = new AdrenalineData();
                customData = YAMLCaster.Encode(Data);
            }
            else
            {
                ItemData Data = new ItemData();
                customData = YAMLCaster.Encode(Data);
            }

            foreach (ICustomItem customItem in CustomItem.List)
            {
                if (customItem.Id == id)
                {
                    NewId = CustomItem.GetFirstFreeId(1);
                    break;
                }
                else
                {
                    NewId = id;
                }
            }

            YAMLCustomItem NewItem = new()
            {
                Id = NewId,
                Name = name,
                Description = description,
                BadgeName = name,
                BadgeColor = "pumpkin",
                Item = itemType,
                CustomItemType = customType,
                Scale = Vector3.one,
                Spawn = new(),
                CustomFlags = CustomFlags.None,
                FlagSettings = new(),
                CustomData = customData,
            };

            string filePath = Path.Combine(Paths.Configs, "UncomplicatedCustomItems", $"{name.ToLower().Replace(" ", "-")}.yml");
            File.WriteAllText(filePath, Loader.Serializer.Serialize(NewItem));

            CustomItem.Register(YAMLCaster.Converter(NewItem));

            LogManager.Info($"Generated and registered custom item: {NewItem.Name} with ID {NewItem.Id}");
        }

        internal string Dir = Path.Combine(Paths.Configs, "UncomplicatedCustomItems");

        public bool Is(string localDir = "")
        {
            return Directory.Exists(Path.Combine(Dir, localDir));
        }

        public string[] List(string localDir = "")
        {
            return Directory.GetFiles(Path.Combine(Dir, localDir));
        }

        public void LoadAll(string localDir = "")
        {
            LoadAction((YAMLCustomItem Item) =>
            {
                CustomItem.Register(YAMLCaster.Converter(Item));
            }, localDir);
        }

        public void LoadAction(Action<YAMLCustomItem> action, string localDir = "")
        {
            foreach (string FileName in List(localDir))
            {
                try
                {
                    if (Directory.Exists(FileName))
                        continue;

                    if (FileName.Split().First() == ".")
                        return;

                    string fileContent = File.ReadAllText(FileName);
                    
                    try 
                    {
                        YAMLCustomItem Item = Loader.Deserializer.Deserialize<YAMLCustomItem>(fileContent);
                        LogManager.Debug($"Proposed to the registerer the external item {Item.Id} [{Item.Name}] from file:\n{FileName}");
                        action(Item);
                    }
                    catch (YamlException yamlEx)
                    {
                        string errorMessage = $"Failed to parse {FileName}. YAML syntax error: {yamlEx.Message}";

                        if (yamlEx.Start.Line > 0)
                        {
                            errorMessage += $" at line {yamlEx.Start.Line}, column {yamlEx.Start.Column}";
                            
                            string[] lines = fileContent.Split('\n');
                            if (yamlEx.Start.Line <= lines.Length)
                            {
                                string problematicLine = lines[yamlEx.Start.Line - 1];
                                errorMessage += $"\nProblematic line: \"{problematicLine.Trim()}\"";
                            }
                        }
                        
                        if (Plugin.Instance.Config.Debug)
                        {
                            LogManager.Error($"{errorMessage}\nStack trace: {yamlEx.StackTrace}\nIf this was caused by a plugin update you can update your customitem here: https://uci.thaumielscpsl.site/uciupdater");
                        }
                        else
                        {
                            LogManager.Error($"{errorMessage}\nIf this was caused by a plugin update you can update your customitem here: https://uci.thaumielscpsl.site/uciupdater");
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = $"Failed to process {FileName}. Error: {ex.Message}";
                        
                        if (ex.Message.Contains("type") || ex.Message.Contains("convert"))
                        {
                            errorMessage += "\nThis appears to be a type mismatch error. Check that your values match the expected types for each field.";
                        }
                        else if (ex.Message.Contains("property") || ex.Message.Contains("member"))
                        {
                            errorMessage += "\nThis appears to be related to an unknown property. Check for typos in your YAML field names.";
                        }
                        
                        if (Plugin.Instance.Config.Debug)
                        {
                            LogManager.Error($"{errorMessage}\nStack trace: {ex.StackTrace}\nIf this was caused by a plugin update you can update your customitem here: https://uci.thaumielscpsl.site/uciupdater");
                        }
                        else
                        {
                            LogManager.Error($"{errorMessage}\nIf this was caused by a plugin update you can update your customitem here: https://uci.thaumielscpsl.site/uciupdater");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error($"Failed to access file {FileName}. Error: {ex.Message}");
                    if (Plugin.Instance.Config.Debug)
                    {
                        LogManager.Error($"Stack trace: {ex.StackTrace}");
                    }
                }
            }
        }

        public void Welcome(string localDir = "", bool loadExamples = false)
        {
            if (!Is(localDir))
            {
                Directory.CreateDirectory(Path.Combine(Dir, localDir));
                if (!loadExamples)
                    File.WriteAllText(Path.Combine(Dir, localDir, "example-item.yml"), Loader.Serializer.Serialize(new YAMLCustomItem()
                    {
                        Id = CustomItem.GetFirstFreeId(1)
                    }));
                else
                    foreach (YAMLCustomItem customItem in _examples)
                        File.WriteAllText(Path.Combine(Dir, localDir, $"{customItem.Name.ToLower().Replace(" ", "-")}.yml"), Loader.Serializer.Serialize(customItem));

                LogManager.Info($"Plugin does not have a item folder, generated one in {Path.Combine(Dir, localDir)}");
            }
        }
    }
}
