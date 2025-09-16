using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AdminToys;
using LabApi.Loader.Features.Misc;
using MapGeneration.Distributors;
using Mirror;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Interfaces;
using UncomplicatedCustomItems.HarmonyElements.Patches;
using UnityEngine;

namespace UncomplicatedCustomItems.Integrations
{
    internal static class MERIntergration
    {
        private static Assembly MERAssembly;
        private static bool Found;
        private static EventToken EventToken;

        public static void Init()
        {
            foreach (LabApi.Loader.Features.Plugins.Plugin plugin in LabApi.Loader.PluginLoader.EnabledPlugins)
            {
                if (plugin.Name is "ProjectMER")
                {
                    Found = true;
                    plugin.TryGetLoadedAssembly(out MERAssembly);
                    break;
                }
            }

            if (Found && MERAssembly != null)
            {
                LogManager.Silent($"MER Found! :D");
                Register();
            }
            else
                LogManager.Silent($"MER Not Found! D:");
        }

        private static void Register()
        {
            LogManager.Silent($"Registering MER SchematicSpawned Event...");
            EventToken = ReflectionEventRegistrar.RegisterEventHandler("ProjectMER.Events.Handlers.Schematic", "SchematicSpawned", obj =>
                {
                    LogManager.Debug($"MER SchematicSpawned Event Triggered");
                    string name = obj?.GetType().GetProperty("Name")?.GetValue(obj)?.ToString() ?? "<unknown>";
                    object schematicObject = obj?.GetType().GetProperty("Schematic")?.GetValue(obj);

                    foreach (CustomItem item in CustomItem.List)
                    {
                        if (item.HasModule(CustomFlags.MERSpawn))
                        {
                            foreach (MERSpawnSettings data in item.FlagSettings.MerSpawnSettings)
                            {
                                LogManager.Debug($"{item.Name} - Yup has MerSpawn - {data.ObjectName} - {data.ReplacePrimitive} - {name}");
                                if (data.ReplacePrimitive && name == data.SchematicName)
                                {
                                    DestroyPrimitiveInSchematic(schematicObject, data.ObjectName, item);
                                }

                                if (data.LockerSpawning && name == data.SchematicName)
                                {
                                    List<Locker> lockers = GetLockers(schematicObject);
                                    if (lockers == null || lockers.Count() <= 0)
                                        continue;

                                    foreach (Locker locker in lockers)
                                    {
                                        LabApi.Features.Wrappers.Locker lablocker = LabApi.Features.Wrappers.Locker.Get(locker);
                                        foreach (SpawnData spawn in item.Spawn.SpawnSettings)
                                        {
                                            LockerSpawningItemPrefix.HandleLockerSpawn(spawn, lablocker, item);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            );
        }

        internal static void Unregister()
        {
            EventToken?.Unregister();
        }

        private static List<Locker> GetLockers(object schematicObj)
        {
            List<Locker> lockers = [];

            PropertyInfo attachedBlocksProperty = schematicObj.GetType().GetProperty("AttachedBlocks");
            if (attachedBlocksProperty == null)
                return null;

            if (attachedBlocksProperty.GetValue(schematicObj) is not IEnumerable<GameObject> gameObjects)
                return null;

            foreach (GameObject obj in gameObjects)
            {
                SpawnableStructure structure = obj.GetComponent<SpawnableStructure>();
                if (structure != null && structure is Locker locker)
                    lockers.Add(locker);
            }

            return lockers;
        }

        private static void DestroyPrimitiveInSchematic(object schematicObj, string primitiveName, ICustomItem item)
        {
            if (schematicObj == null)
                return;

            try
            {
                PropertyInfo adminToyBasesProperty = schematicObj.GetType().GetProperty("AdminToyBases");
                if (adminToyBasesProperty == null)
                    return;

                if (adminToyBasesProperty.GetValue(schematicObj) is not IEnumerable<AdminToyBase> adminToyBases)
                    return;

                foreach (AdminToyBase adminToy in adminToyBases)
                {
                    if (adminToy == null || adminToy.gameObject == null)
                        continue;

                    if (adminToy.gameObject.name.Contains(primitiveName) || adminToy.gameObject.name == primitiveName)
                    {
                        LogManager.Debug($"Destroying primitive: {adminToy.gameObject.name} - Replacing with {item.Name}");
                        new SummonedCustomItem(item, adminToy.Position);
                        NetworkServer.Destroy(adminToy.gameObject);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"Error destroying primitive in schematic: {ex.Message}");
            }
        }
    }
}