#if EXILED
using Exiled.API.Interfaces;
using Exiled.Loader;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Manager;
using YamlDotNet.Serialization;

namespace UncomplicatedCustomItems.API.CustomModuleAPI
{
    public class CustomModuleManager
    {
        public static List<CustomModuleBase> CustomModules { get; set; } = [];

#if EXILED
        public static List<IPlugin<IConfig>> ActivePlugins { get; set; } = [];
        public static Dictionary<CustomModuleBase, IPlugin<IConfig>> ModuleOwners { get; set; } = [];
#else
        public static List<LabApi.Loader.Features.Plugins.Plugin> ActivePlugins { get; set; } = [];
        public static Dictionary<CustomModuleBase, LabApi.Loader.Features.Plugins.Plugin> ModuleOwners { get; set; } = [];
#endif

        private static readonly Dictionary<Type, ModuleAccessors> _accessorCache = [];
        private static readonly object _accessorCacheLock = new();

        private static readonly Dictionary<Type, Func<CustomModuleBase>> _factoryCache = [];
        private static readonly object _cacheLock = new();

        public static void Init()
        {
#if EXILED
            foreach (IPlugin<IConfig> plugin in Loader.Plugins)
            {
                LogManager.Silent($"Passing plugin {plugin.Name}");
                foreach (Type type in plugin.Assembly.GetTypes())
                {
                    if (!type.IsClass || type.IsAbstract)
                        continue;
                    if (!typeof(CustomModuleBase).IsAssignableFrom(type))
                        continue;

                    LogManager.Silent($"Importing {type.FullName}!");
                    ActivePlugins.TryAdd(plugin);
                    try
                    {
                        CustomModuleBase module = CreateInstanceFast(type);
                        if (!type.FullName.Contains("UncomplicatedCustomItems"))
                            LogManager.Info($"Imported CustomModule {module.Name} from plugin {plugin.Name} - {type.FullName}");

                        CustomModules.TryAdd(module);
                        ModuleOwners[module] = plugin;
                        module.OnRegistered();
                    }
                    catch (Exception regEx)
                    {
                        LogManager.Error($"Error in OnRegistered for {type.FullName}: {regEx}");
                    }
                }
            }
#else
            foreach (KeyValuePair<LabApi.Loader.Features.Plugins.Plugin, Assembly> pluginEntry in LabApi.Loader.PluginLoader.Plugins.ToArray())
            {
                try
                {
                    LogManager.Silent($"Passing plugin {pluginEntry.Key.Name}");
                    Type[] types;
                    try
                    {
                        types = pluginEntry.Value.GetTypes();
                    }
                    catch (ReflectionTypeLoadException rtlEx)
                    {
                        LogManager.Error($"Failed to get types from plugin {pluginEntry.Key.Name}: {rtlEx}");
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

                            LogManager.Silent($"Importing {type.FullName}!");
                            ActivePlugins.TryAdd(pluginEntry.Key);

                            CustomModuleBase module = CreateInstanceFast(type);

                            if (!type.FullName.Contains("UncomplicatedCustomItems"))
                                LogManager.Info($"Imported CustomModule {module.Name} from plugin {pluginEntry.Key.Name} - {type.FullName}");

                            CustomModules.TryAdd(module);
                            ModuleOwners[module] = pluginEntry.Key;
                            module.OnRegistered();
                        }
                        catch (Exception e)
                        {
                            LogManager.Error($"Error while registering CustomModule {type.FullName}: {e}");
                        }
                    }
                }
                catch (Exception e)
                {
                    LogManager.Error($"Unexpected error with plugin {pluginEntry.Key.Name}: {e}");
                }
            }
#endif
        }

#if EXILED
        public static void RegisterCustomModule<T>(IPlugin<IConfig> owner) where T : CustomModuleBase
#else
        public static void RegisterCustomModule<T>(LabApi.Loader.Features.Plugins.Plugin owner) where T : CustomModuleBase
#endif
        {
            Type type = typeof(T);

            if (CustomModules.Any(m => m.GetType() == type))
            {
                LogManager.Warn($"CustomModule {type.FullName} is already registered. Skipping.");
                return;
            }

            try
            {
                CustomModuleBase module = CreateInstanceFast(type);
                CustomModules.TryAdd(module);
                ModuleOwners[module] = owner;
                LogManager.Info($"Registered CustomModule {module.Name} - {type.FullName}");
                module.OnRegistered();
            }
            catch (Exception regEx)
            {
                LogManager.Error($"Error in OnRegistered for {type.FullName}: {regEx}");
            }
        }

        public static CustomModuleBase CreateInstanceFast(Type type)
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
                    Type moduleType = customModule.GetType();
                    instance = CloneModule(customModule, moduleType);
                }
                catch (Exception ex)
                {
                    LogManager.Error($"Failed to instantiate or deserialize module {customModule?.Name}: {ex}");
                    continue;
                }

                instance.CustomItem = item.CustomItem;
                instance.RegisterEvents();
                instance.OnAdded(item);

                item.CustomModules.Add(instance);
            }
        }

        public static Dictionary<CustomModuleBase, List<object>> Decode(Dictionary<object, List<object>> args)
        {
            Dictionary<CustomModuleBase, List<object>> result = [];
            if (args == null || CustomModules == null)
                return result;

            foreach (KeyValuePair<object, List<object>> kvp in args)
            {
                try
                {
                    if (kvp.Key == null)
                        continue;

                    string key = kvp.Key.ToString().Trim();
                    if (key.Length == 0)
                        continue;

                    CustomModuleBase? templateModule = CustomModules.FirstOrDefault(cm => string.Equals(cm?.Name, key, StringComparison.OrdinalIgnoreCase));
                    if (templateModule == null)
                    {
                        LogManager.Warn($"No registered module named '{key}' found. Known modules: {string.Join(", ", CustomModules.Select(m => m?.Name))}");
                        continue;
                    }

                    Type moduleType = templateModule.GetType();

                    if (kvp.Value == null || kvp.Value.Count == 0)
                    {
                        CustomModuleBase defaultInstance = CreateInstanceFast(moduleType);
                        result[defaultInstance] = [];
                        continue;
                    }

                    foreach (object raw in kvp.Value)
                    {
                        if (raw == null)
                            continue;

                        try
                        {
                            CustomModuleBase? deserializedModule = PopulateModule(moduleType, raw);
                            if (deserializedModule != null)
                            {
                                if (!result.TryGetValue(deserializedModule, out List<object> list))
                                {
                                    list = [];
                                    result[deserializedModule] = list;
                                }

                                list.Add(raw);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogManager.Error($"Exception deserializing module {templateModule.Name}: {ex}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error($"Exception while processing entry: {ex}");
                }
            }

            return result;
        }

        private static ModuleAccessors GetAccessors(Type moduleType)
        {
            if (_accessorCache.TryGetValue(moduleType, out ModuleAccessors cached))
                return cached;

            lock (_accessorCacheLock)
            {
                if (_accessorCache.TryGetValue(moduleType, out cached))
                    return cached;

                Dictionary<string, (Action<CustomModuleBase, object?>, Type)> setters = new(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, Func<CustomModuleBase, object?>> getters = new(StringComparer.OrdinalIgnoreCase);

                foreach (PropertyInfo prop in moduleType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (prop.GetIndexParameters().Length > 0)
                        continue;

                    if (prop.GetCustomAttribute<YamlIgnoreAttribute>() != null)
                        continue;

                    if (prop.CanRead && prop.GetGetMethod(nonPublic: false) != null)
                        getters[prop.Name] = BuildGetter(prop);

                    if (prop.CanWrite && prop.GetSetMethod(nonPublic: false) != null)
                        setters[prop.Name] = (BuildSetter(prop), prop.PropertyType);
                }

                cached = new ModuleAccessors(setters, getters);
                _accessorCache[moduleType] = cached;
                return cached;
            }
        }

        private static Action<CustomModuleBase, object?> BuildSetter(PropertyInfo prop)
        {
            ParameterExpression instanceParam = Expression.Parameter(typeof(CustomModuleBase), "instance");
            ParameterExpression valueParam = Expression.Parameter(typeof(object), "value");

            UnaryExpression instanceCast = Expression.Convert(instanceParam, prop.DeclaringType!);
            UnaryExpression valueCast = Expression.Convert(valueParam, prop.PropertyType);
            MethodCallExpression call = Expression.Call(instanceCast, prop.GetSetMethod()!, valueCast);

            return Expression.Lambda<Action<CustomModuleBase, object?>>(call, instanceParam, valueParam).Compile();
        }

        private static Func<CustomModuleBase, object?> BuildGetter(PropertyInfo prop)
        {
            ParameterExpression instanceParam = Expression.Parameter(typeof(CustomModuleBase), "instance");
            UnaryExpression instanceCast = Expression.Convert(instanceParam, prop.DeclaringType!);
            MemberExpression propertyAccess = Expression.Property(instanceCast, prop);
            UnaryExpression convert = Expression.Convert(propertyAccess, typeof(object));

            return Expression.Lambda<Func<CustomModuleBase, object?>>(convert, instanceParam).Compile();
        }

        private static CustomModuleBase? PopulateModule(Type moduleType, object raw)
        {
            CustomModuleBase instance = CreateInstanceFast(moduleType);

            if (raw is not IDictionary dict)
            {
                LogManager.Warn($"Expected a mapping for module {moduleType.Name} but got {raw?.GetType().Name ?? "null"}.");
                return instance;
            }

            ModuleAccessors accessors = GetAccessors(moduleType);

            foreach (DictionaryEntry entry in dict)
            {
                string keyName = entry.Key?.ToString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(keyName))
                    continue;

                if (!accessors.Setters.TryGetValue(keyName, out var setter))
                {
                    LogManager.Warn($"'{keyName}' in the config for module {moduleType.Name} doesn't match any settable property. Known properties: {string.Join(", ", accessors.Setters.Keys)}");
                    continue;
                }

                try
                {
                    setter.Set(instance, ConvertValue(entry.Value, setter.Type));
                }
                catch (Exception ex)
                {
                    LogManager.Error($"Failed to set {keyName} on {moduleType.Name}: {ex}");
                }
            }

            return instance;
        }

        private static CustomModuleBase CloneModule(CustomModuleBase source, Type moduleType)
        {
            CustomModuleBase clone = CreateInstanceFast(moduleType);
            ModuleAccessors accessors = GetAccessors(moduleType);

            foreach (KeyValuePair<string, Func<CustomModuleBase, object?>> getter in accessors.Getters)
            {
                if (!accessors.Setters.TryGetValue(getter.Key, out var setter))
                    continue;

                setter.Set(clone, getter.Value(source));
            }

            return clone;
        }

        private static object? ConvertValue(object? value, Type targetType)
        {
            if (value == null)
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

            Type underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlying.IsInstanceOfType(value))
                return value;

            if (underlying.IsEnum)
                return Enum.Parse(underlying, value.ToString()!, ignoreCase: true);

            return Convert.ChangeType(value, underlying);
        }
    }
}