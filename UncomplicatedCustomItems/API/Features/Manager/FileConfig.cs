#if EXILED
using Exiled.API.Features;
using Exiled.API.Extensions;
using Exiled.Loader;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Interfaces;
using UnityEngine;
using YamlDotNet.Core;
using InventorySystem.Items.Usables.Scp330;
using UncomplicatedCustomItems.API.ItemUpdater;

namespace UncomplicatedCustomItems.API.Features.Manager
{
    public class FileConfig
    {
        public static readonly List<YAMLCustomItem> _examples =
        [
            new()
            {
                CustomData = YAMLCaster.Encode(new ItemData()
                {
                    Data =
                    [
                        new()
                        {
                            Event = ItemEvents.Command,
                            Command = "/SERVER_EVENT DETONATION_INSTANT",
                            ConsoleMessage = "UHUHUHUH!"
                        }
                    ]
                })
            },
            new()
            {
                Id = 2,
                Name = "FunnyGun",
                Description = "A FRMG0 that has a shotgun-like bullet spread",
                BadgeName = "FunnyGun",
                BadgeColor = "pumpkin",
                Item = ItemType.GunFRMG0,
                CustomItemType = CustomItemType.Weapon,
                Scale = Vector3.one,
                Spawn = new(),
                Arguments = new Dictionary<ArgumentType, string>
                {
                    [ArgumentType.OnShotWeapon] = "action Example",
                    [ArgumentType.OnAimedWeapon] = "Player::Damage(10, \"Test\", 'AIMING')",
                },
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
                CustomData = YAMLCaster.Encode(new ArmorData()
                {
                    HeadProtection = 150,
                    BodyProtection = 200,
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
                Item = ItemType.KeycardCustomSite02,
                CustomItemType = CustomItemType.Keycard,
                Scale = Vector3.one,
                Spawn = new(),
                CustomData = YAMLCaster.Encode(new KeycardData())
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
                Arguments = new Dictionary<ArgumentType, string>
                {
                    [ArgumentType.OnUsedItem] = "if {Player.Health} < 100 then Player::EnableEffect[CustomPlayerEffects.Flashed](2, 10, true)",
                },
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
            },
            new()
            {
                Id = 19,
                Name = "SCP127",
                Description = "SCP127",
                Item = ItemType.GunSCP127,
                CustomItemType = CustomItemType.SCPItem,
                Scale = Vector3.one,
                CustomData = YAMLCaster.Encode(new SCP127Data())
            },
            new()
            {
                Id = 20,
                Name = "SCP268",
                Description = "SCP268",
                Item = ItemType.SCP268,
                CustomItemType = CustomItemType.SCPItem,
                Scale = Vector3.one,
                CustomData = YAMLCaster.Encode(new SCP268Data())
            },
            new()
            {
                Id = 24,
                Name = "MicroHID",
                Description = "MicroHID",
                Item = ItemType.MicroHID,
                CustomItemType = CustomItemType.MicroHID,
                Scale = Vector3.one,
                CustomData = YAMLCaster.Encode(new MicroHIDData())
            },
            new()
            {
                Id = 25,
                Name = "Flashlight",
                Description = "Flashlight",
                Item = ItemType.Flashlight,
                CustomItemType = CustomItemType.Light,
                Scale = Vector3.one,
                CustomData = YAMLCaster.Encode(new FlashlightData())
            },
            new()
            {
                Id = 26,
                Name = "Lantern",
                Description = "Lantern",
                Item = ItemType.Lantern,
                CustomItemType = CustomItemType.Light,
                Scale = Vector3.one,
                CustomData = YAMLCaster.Encode(new FlashlightData())
            },
            new()
            {
                Id = 27,
                Name = "ParticleDisruptor",
                Description = "ParticleDisruptor",
                Item = ItemType.ParticleDisruptor,
                CustomItemType = CustomItemType.ParticleDisruptor,
                Scale = Vector3.one,
                CustomData = YAMLCaster.Encode(new ParticleDisruptorData())
            },
            new()
            {
                Id = 28,
                Name = "SCP330",
                Description = "SCP330",
                Item = ItemType.SCP330,
                CustomItemType = CustomItemType.Candy,
                Scale = Vector3.one,
                CustomData = YAMLCaster.Encode(new CandyData()
                {
                    CandyType = CandyKindID.Rainbow,
                    Chance = 100,
                    DestroyOnUse = false,
                    EatingMessage = ""
                })
            },
            new()
            {
                Id = 29,
                Name = "SCP1509",
                Description = "SCP1509",
                Item = ItemType.SCP1509,
                CustomItemType = CustomItemType.SCPItem,
                Scale = Vector3.one,
                CustomData = YAMLCaster.Encode(new SCP1509Data())
            },
        ];

        public uint NewId = new();

        public void GenerateCustomItem(uint id, string name, ItemType itemType, CustomItemType customType, string description)
        {
            Dictionary<string, object> customData = [];
            switch (customType, itemType)
            {
                case (CustomItemType.SCPItem, ItemType.SCP1509):
                    customData = YAMLCaster.Encode(new SCP1509Data());
                    break;
                    
                case (CustomItemType.SCPItem, ItemType.SCP244a):
                case (CustomItemType.SCPItem, ItemType.SCP244b):
                    customData = YAMLCaster.Encode(new SCP244Data());
                    break;

                case (CustomItemType.SCPItem, ItemType.SCP2176):
                    customData = YAMLCaster.Encode(new SCP2176Data());
                    break;

                case (CustomItemType.SCPItem, ItemType.SCP018):
                    customData = YAMLCaster.Encode(new SCP018Data());
                    break;

                case (CustomItemType.SCPItem, ItemType.SCP500):
                    customData = YAMLCaster.Encode(new SCP500Data());
                    break;

                case (CustomItemType.SCPItem, ItemType.SCP207):
                case (CustomItemType.SCPItem, ItemType.AntiSCP207):
                    customData = YAMLCaster.Encode(new SCP207Data());
                    break;

                case (CustomItemType.SCPItem, ItemType.SCP1853):
                    customData = YAMLCaster.Encode(new SCP1853Data());
                    break;

                case (CustomItemType.SCPItem, ItemType.SCP1576):
                    customData = YAMLCaster.Encode(new SCP1576Data());
                    break;

                case (CustomItemType.SCPItem, ItemType.GunSCP127):
                    customData = YAMLCaster.Encode(new SCP127Data());
                    break;

                case (CustomItemType.SCPItem, ItemType.SCP268):
                    customData = YAMLCaster.Encode(new SCP268Data());
                    break;

                case (CustomItemType.Candy, ItemType.SCP330):
                    customData = YAMLCaster.Encode(new CandyData());
                    break;

                case (CustomItemType.Weapon, _):
                    customData = YAMLCaster.Encode(new WeaponData());
                    break;

                case (CustomItemType.Keycard, _):
                    customData = YAMLCaster.Encode(new KeycardData());
                    break;

                case (CustomItemType.Armor, _):
                    customData = YAMLCaster.Encode(new ArmorData());
                    break;

                case (CustomItemType.ExplosiveGrenade, ItemType.GrenadeHE):
                    customData = YAMLCaster.Encode(new ExplosiveGrenadeData());
                    break;

                case (CustomItemType.FlashGrenade, ItemType.GrenadeFlash):
                    customData = YAMLCaster.Encode(new FlashGrenadeData());
                    break;

                case (CustomItemType.Jailbird, ItemType.Jailbird):
                    customData = YAMLCaster.Encode(new JailbirdData());                
                    break;

                case (CustomItemType.Medikit, ItemType.Medkit):
                    customData = YAMLCaster.Encode(new MedikitData());
                    break;

                case (CustomItemType.Painkillers, ItemType.Painkillers):
                    customData = YAMLCaster.Encode(new PainkillersData());
                    break;

                case (CustomItemType.Adrenaline, ItemType.Adrenaline):
                    customData = YAMLCaster.Encode(new AdrenalineData());
                    break;

                case (CustomItemType.MicroHID, ItemType.MicroHID):
                    customData = YAMLCaster.Encode(new MicroHIDData());
                    break;

                case (CustomItemType.ParticleDisruptor, ItemType.ParticleDisruptor):
                    customData = YAMLCaster.Encode(new ParticleDisruptorData());
                    break;

                case (CustomItemType.Light, ItemType.Lantern):
                case (CustomItemType.Light, ItemType.Flashlight):
                    customData = YAMLCaster.Encode(new FlashlightData());
                    break;

                case (CustomItemType.Item, _):
                    customData = YAMLCaster.Encode(new ItemData());
                    break;
            }

            foreach (ICustomItem customItem in CustomItem.List)
            {
                if (customItem.Id == id)
                {
                    NewId = CustomItem.GetFirstFreeId(1);
                    break;
                }
                else
                    NewId = id;
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
                CustomData = customData,
            };

#if EXILED
            string filePath = Path.Combine(Paths.Configs, "UncomplicatedCustomItems", $"{name.ToLower().Replace(" ", "-")}.yml");
            File.WriteAllText(filePath, Loader.Serializer.Serialize(NewItem));            
#else
            string filePath = Path.Combine(LabApi.Loader.Features.Paths.PathManager.Configs.ToString(), "UncomplicatedCustomItems", $"{name.ToLower().Replace(" ", "-")}.yml");
            File.WriteAllText(filePath, LabApi.Loader.Features.Yaml.YamlConfigParser.Serializer.Serialize(NewItem));
#endif

            CustomItem.Register(YAMLCaster.Converter(NewItem));
            LogManager.Info($"Generated and registered custom item: {NewItem.Name} with ID {NewItem.Id}");
        }
        
#if EXILED
        internal string Dir = Path.Combine(Paths.Configs, "UncomplicatedCustomItems");
#else
        internal string Dir = Path.Combine(LabApi.Loader.Features.Paths.PathManager.Configs.ToString(), "UncomplicatedCustomItems");
#endif

        public bool Is(string localDir = "")
        {
            return Directory.Exists(Path.Combine(Dir, localDir));
        }

        public string[] List(string localDir = "")
        {
            return Directory.GetFiles(Path.Combine(Dir, localDir));
        }

        internal bool IsActionFile(string fileContent) => fileContent.Contains("actions:") || fileContent.Contains("parameters:");

        public void LoadAll(string localDir = "")
        {
            foreach (string fileName in List(localDir))
            {
                try
                {
                    if (Directory.Exists(fileName))
                        continue;

                    string fileContent = File.ReadAllText(fileName);
                    if (IsActionFile(fileContent))
                    {
                        YAMLCustomAction action = LabApi.Loader.Features.Yaml.YamlConfigParser.Deserializer.Deserialize<YAMLCustomAction>(fileContent);
                        CustomAction.Register(YAMLCaster.Converter(action));
                        LogManager.Debug($"Registering action {action.Id} [{action.Name}] from {fileName}");
                    }
                    else
                    {
                        try
                        {
                            if (ItemUpdateManager.TryUpdate(Path.Combine(Dir, localDir, fileName)))
                                LogManager.Silent($"Updated Item {fileName}");

                            YAMLCustomItem item = LabApi.Loader.Features.Yaml.YamlConfigParser.Deserializer.Deserialize<YAMLCustomItem>(fileContent);
                            CustomItem.Register(YAMLCaster.Converter(item));
                            LogManager.Debug($"Registering item {item.Id} [{item.Name}] from {fileName}");
                        }
                        catch (YamlException yamlEx)
                        {
                            CustomItem.ErrorCustomItems.Add(new ErrorCustomItem(fileName, File.ReadAllLines(fileName), yamlEx));
                            string errorMessage = $"Failed to parse {fileName}. YAML syntax error: {yamlEx.Message}";

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
                                LogManager.Error($"{errorMessage}\nStack trace: {yamlEx.StackTrace}\nIf this was caused by a plugin update you can update your customitem here: https://uci.ucserver.it/uciupdater");
                            }
                            else
                            {
                                LogManager.Error($"{errorMessage}\nIf this was caused by a plugin update you can update your customitem here: https://uci.ucserver.it/uciupdater");
                            }
                        }
                        catch (Exception ex)
                        {
                            string errorMessage = $"Failed to process {fileName}. Error: {ex.Message}";

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
                                LogManager.Error($"{errorMessage}\nStack trace: {ex.StackTrace}\nIf this was caused by a plugin update you can update your customitem here: https://uci.ucserver.it/uciupdater");
                            }
                            else
                                LogManager.Error($"{errorMessage}\nIf this was caused by a plugin update you can update your customitem here: https://uci.ucserver.it/uciupdater");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error($"Failed to process {fileName}: {ex.Message}");
                }
            }
        }

        public void Welcome(string localDir = "", bool loadExamples = false)
        {
            if (!Is(localDir))
            {
                Directory.CreateDirectory(Path.Combine(Dir, localDir));
                if (!loadExamples)
                    if (localDir != "Actions")
                    {
                        File.WriteAllText(Path.Combine(Dir, localDir, "example-item.yml"), LabApi.Loader.Features.Yaml.YamlConfigParser.Serializer.Serialize(new YAMLCustomItem()
                        {
                            Id = CustomItem.GetFirstFreeId(1)
                        }));
                        LogManager.Debug($"Creating CustomItem at {Path.Combine(Dir, localDir)}");
                    }
                    else
                    {
                        File.WriteAllText(Path.Combine(Dir, localDir, "example-action.yml"), LabApi.Loader.Features.Yaml.YamlConfigParser.Serializer.Serialize(new YAMLCustomAction()
                        {
                            Id = CustomAction.GetFirstFreeId(1)
                        }));
                        LogManager.Debug($"Creating CustomAction at {Path.Combine(Dir, localDir)}");
                    }
                else
                {
                    foreach (YAMLCustomItem customItem in _examples)
                    {
                        File.WriteAllText(Path.Combine(Dir, localDir, $"{customItem.Name.ToLower().Replace(" ", "-")}.yml"), LabApi.Loader.Features.Yaml.YamlConfigParser.Serializer.Serialize(customItem));
                    }
                }

                LogManager.Info($"Plugin does not have a item folder, generated one in {Path.Combine(Dir, localDir)}");
            }
        }
    }
}