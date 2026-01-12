using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.API.CustomModuleAPI
{
    public class CustomModuleManager
    {
        public static List<CustomModuleBase> CustomModules { get; set; } = [];
        public static List<LabApi.Loader.Features.Plugins.Plugin> ActivePlugins { get; set; } = [];
        public static Dictionary<CustomModuleBase, LabApi.Loader.Features.Plugins.Plugin> ModuleOwners { get; set; } = [];
        private static readonly Dictionary<Type, Func<CustomModuleBase>> _factoryCache = [];
        private static readonly object _cacheLock = new();

        public static void Init()
        {
            foreach (var pluginEntry in LabApi.Loader.PluginLoader.Plugins.ToArray())
            {
                try
                {
                    LogManager.Silent($"{nameof(CustomModuleManager)}: Passing plugin {pluginEntry.Key.Name}");
                    Type[] types;
                    try
                    {
                        types = pluginEntry.Value.GetTypes();
                    }
                    catch (ReflectionTypeLoadException rtlEx)
                    {
                        LogManager.Error($"{nameof(CustomModuleManager)}: Failed to get types from plugin {pluginEntry.Key.Name}: {rtlEx}");
                        foreach (Exception loaderEx in rtlEx.LoaderExceptions ?? [])
                            LogManager.Error($" - LoaderException: {loaderEx}");

                        continue;
                    }

                    foreach (Type type in types)
                    {
                        try
                        {
                            if (!type.IsClass || type.IsAbstract)
                                continue;
                            if (!typeof(CustomModuleBase).IsAssignableFrom(type))
                                continue;

                            LogManager.Silent($"{nameof(CustomModuleManager)}: Importing {type.FullName}!");
                            ActivePlugins.TryAdd(pluginEntry.Key);
                            object instance = null;
                            try
                            {
                                instance = Activator.CreateInstance(type);
                            }
                            catch (MissingMethodException)
                            {
                                LogManager.Error($"{nameof(CustomModuleManager)}: No parameterless constructor for {type.FullName} (plugin {pluginEntry.Key.Name}).");
                                continue;
                            }

                            if (instance is not CustomModuleBase module)
                            {
                                LogManager.Error($"{nameof(CustomModuleManager)}: Instance of {type.FullName} could not be cast to CustomModuleBase.");
                                continue;
                            }

                            LogManager.Info($"{nameof(CustomModuleManager)}: Imported CustomModule {module.Name} from plugin {pluginEntry.Key.Name} - {type.FullName}");
                            CustomModules.TryAdd(module);
                            ModuleOwners[module] = pluginEntry.Key;

                            try
                            {
                                module.OnRegistered();
                            }
                            catch (Exception regEx)
                            {
                                LogManager.Error($"{nameof(CustomModuleManager)}: Error in OnRegistered for {module.Name}: {regEx}");
                            }
                        }
                        catch (Exception e)
                        {
                            LogManager.Error($"{nameof(CustomModuleManager)}: Error while registering CustomModule: {e}");
                        }
                    }
                }
                catch (Exception e)
                {
                    LogManager.Error($"{nameof(CustomModuleManager)}: Unexpected error with plugin {pluginEntry.Key.Name}: {e}");
                }
            }
        }

        private static CustomModuleBase CreateInstanceFast(Type type)
        {
            if (!_factoryCache.TryGetValue(type, out Func<CustomModuleBase> factory))
            {
                lock (_cacheLock)
                {
                    if (!_factoryCache.TryGetValue(type, out factory))
                    {
                        ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes) ?? throw new InvalidOperationException($"Type {type.Name} must have a parameterless constructor");
                        NewExpression newExpr = Expression.New(ctor);
                        Expression<Func<CustomModuleBase>> lambda = Expression.Lambda<Func<CustomModuleBase>>(newExpr);
                        factory = lambda.Compile();
                        _factoryCache[type] = factory;
                    }
                }
            }
            
            return factory();
        }

        public static void Destroy(SummonedCustomItem item)
        {
            foreach (CustomModuleBase customModule in item.CustomModules.ToArray())
            {
                customModule.UnregisterEvents();
                customModule.OnDestroyed();
                item.CustomModules.Remove(customModule);
            }
        }

        public static void Load(SummonedCustomItem item)
        {
            LogManager.Debug($"Loaded CustomModules for {item.CustomItem.Name}");
            foreach (CustomModuleBase customModule in item.CustomItem.CustomModules.Keys.ToArray())
            {
                LogManager.Debug($"{customModule.Name}");
                CustomModuleBase instance;
                try
                {
                    instance = CreateInstanceFast(customModule.GetType());
                }
                catch (Exception ex)
                {
                    LogManager.Error($"Failed to instantiate module {customModule?.Name}: {ex}");
                    continue;
                }

                instance.CustomItem = item.CustomItem;
                if (item.CustomItem.CustomModules.TryGetValue(customModule, out List<object> rawList) && rawList is { Count: > 0 })
                {
                    List<Dictionary<object, object>> args = [];
                    foreach (object raw in rawList)
                    {
                        if (raw is Dictionary<object, object> dict)
                        {
                            args.Add(dict);
                            continue;
                        }

                        List<string> required = instance.RequiredArguments ?? [];
                        args.Add(required.Count > 0 ? new Dictionary<object, object> { { required[0], raw } } : []);
                    }

                    instance.Arguments = args;
                }
                else
                {
                    instance.Arguments =
                    [
                        []
                    ];
                }

                instance.RegisterEvents();
                instance.OnAdded(item);

                item.CustomModules.Add(instance);
            }
        }

        public static Dictionary<CustomModuleBase, List<object>> Decode(Dictionary<object, List<object>> args)
        {
            Dictionary<CustomModuleBase, List<object>> result = [];
            if (args == null)
            {
                LogManager.Debug("CustomModuleManager.Decode: args is null");
                return result;
            }

            if (CustomModules == null)
            {
                LogManager.Debug("CustomModuleManager.Decode: CustomModules is null");
                return result;
            }

            LogManager.Debug($"CustomModuleManager.Decode: starting. arg count = {args.Count}. available modules = {CustomModules.Count}");
            foreach (KeyValuePair<object, List<object>> top in args)
            {
                string k = top.Key?.ToString() ?? "<null>";
                string vt = top.Value == null ? "<null>" : $"List<{(top.Value.Count>0 ? top.Value[0]?.GetType().Name : "unknown")}>";
            }

            foreach (KeyValuePair<object, List<object>> kvp in args)
            {
                try
                {
                    if (kvp.Key == null)
                        continue;

                    string key = kvp.Key.ToString().Trim();
                    if (key.Length == 0)
                        continue;

                    CustomModuleBase module = CustomModules.FirstOrDefault(cm => string.Equals(cm?.Name, key, StringComparison.OrdinalIgnoreCase));
                    if (module == null)
                        continue;

                    LogManager.Debug($"  Found module: {module.Name}");

                    HashSet<string> required = new(module.RequiredArguments ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);                    
                    if (kvp.Value == null || kvp.Value.Count == 0)
                    {
                        if (!result.TryGetValue(module, out List<object> list))
                        {
                            list = [];
                            result[module] = list;
                        }
                        list.Add(new Dictionary<object, object>());
                        continue;
                    }

                    foreach (object raw in kvp.Value)
                    {
                        if (raw == null)
                            continue;

                        if (raw is Dictionary<object, object> rawDict)
                        {
                            Dictionary<object, object> filtered = [];
                            
                            if (required.IsEmpty())
                            {
                                foreach (KeyValuePair<object, object> inner in rawDict)
                                    filtered[inner.Key] = inner.Value;
                            }
                            else
                            {
                                foreach (KeyValuePair<object, object> inner in rawDict)
                                {
                                    string innerKey = inner.Key?.ToString().Trim() ?? "<null>";
                                    if (required.Contains(innerKey))
                                        filtered[innerKey] = inner.Value;
                                }
                            }

                            if (filtered.Count > 0)
                            {
                                if (!result.TryGetValue(module, out List<object> list))
                                {
                                    list = [];
                                    result[module] = list;
                                }

                                list.Add(filtered);
                            }
                        }
                        else
                        {
                            string rawText = raw.ToString().Trim();

                            if (required.Count == 0)
                            {
                                if (!result.TryGetValue(module, out List<object> list))
                                {
                                    list = [];
                                    result[module] = list;
                                }
                                
                                list.Add(raw);
                            }
                            else if (required.Contains(rawText))
                            {
                                if (!result.TryGetValue(module, out List<object> list))
                                {
                                    list = [];
                                    result[module] = list;
                                }

                                list.Add(raw);
                            }
                            else
                                LogManager.Debug($"  Value '{rawText}' does not match any required arguments: {string.Join(", ", required)}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error($"CustomModuleManager.Decode: exception while processing entry: {ex}");
                }
            }

            LogManager.Debug($"CustomModuleManager.Decode: completed. Result contains {result.Count} modules");
            return result;
        }
    }
}