#if EXILED
using Exiled.API.Interfaces;
using Exiled.Loader;
#endif

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UncomplicatedCustomItems.API.Attributes;
using UncomplicatedCustomItems.API.Extensions;
using System.Reflection;
using UncomplicatedCustomItems.API.Features.Helper;
using System.Linq;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    internal class ImportManager
    {
        private static bool _alreadyLoaded = false;

        public static void Init()
        {
            if (_alreadyLoaded)
                return;

            ActivePlugins.Clear();
            // Call a delayed task
            Task.Run(Actor);
        }

        internal static Type[] BannedTypes =
        [
            typeof(CustomItem),
            typeof(ICustomItem)
        ];

        internal static Type[] CustomItemTypes =
        [
            typeof(CustomArmor),
            typeof(CustomExplosiveGrenade),
            typeof(CustomFlashGrenade),
            typeof(CustomKeycard),
            typeof(CustomWeapon),
            typeof(CustomSCP127),
            typeof(CustomSCP207),
            typeof(CustomSCP244),
            typeof(CustomSCP268),
            typeof(CustomSCP1853),
            typeof(ToolGun),
            typeof(CustomJailbird),
            typeof(CustomSCP018),
            typeof(CustomCandy),
            typeof(CustomPainkillers),
            typeof(CustomMedkit),
            typeof(CustomAdrenaline)
        ];

#if EXILED
        public static List<IPlugin<IConfig>> ActivePlugins => new();

        internal static void Actor()
        {
            LogManager.Info($"{nameof(ImportManager.Actor)}: Checking for CustomItems registered in other plugins to import...");
            _alreadyLoaded = true;

            foreach (IPlugin<IConfig> plugin in Loader.Plugins)
            {
                LogManager.Silent($"{nameof(ImportManager.Actor)}: Passing plugin {plugin.Name}");
                foreach (Type type in plugin.Assembly.GetTypes())
                    try
                    {
                        object[] attribs = type.GetCustomAttributes(typeof(PluginCustomItem), false);
                        if (attribs != null && attribs.Length > 0 && CustomItemTypes.Any(baseType => baseType.IsAssignableFrom(type)))
                        {
                            if (!Plugin.Instance.Config.LoadExampleapiItems)
                            {
                                switch (type.FullName)
                                {
                                    case "UncomplicatedCustomItems.Examples.ExampleCustomWeapon":
                                        continue;
                                    case "UncomplicatedCustomItems.Examples.ExampleCustomKeycard":
                                        continue;
                                    case "UncomplicatedCustomItems.Examples.ExampleCustomGrenade":
                                        continue;
                                    case "UncomplicatedCustomItems.Examples.ExampleCustomArmor":
                                        continue;
                                    case "UncomplicatedCustomItems.Examples.ExampleCustomCandy":
                                        continue;
                                };
                            }

                            if (type.FullName == "UncomplicatedCustomItems.API.ToolGun.ToolGun" && !Plugin.Instance.Config.EnableToolGun)
                                continue;

                            LogManager.Silent($"{nameof(ImportManager.Actor)}: Importing It!");
                            ActivePlugins.TryAdd(plugin);

                            APICustomItem Item = Activator.CreateInstance(type) as APICustomItem;
                            LogManager.Info($"{nameof(ImportManager.Actor)}: Imported CustomItem {Item.Name} ({Item.Id}) through Attribute from plugin {plugin.Name} (v{plugin.Version})");

                            APICustomItem.Register(Item);
                        }

                        if (attribs != null && attribs.Length > 0 && BannedTypes.Any(baseType => baseType.IsAssignableFrom(type)))
                            LogManager.Warn($"{type.FullName} is using a old version of the CustomItem API. For the CustomItem to be loaded and registered it MUST be updated to the new version");
                    }
                    catch (Exception e)
                    {
                        LogManager.Error($"{nameof(ImportManager.Actor)}: Error while registering CustomItem from class by Attribute: {e.GetType().FullName} - {e.Message}\nType: {type.FullName} [{plugin.Name}] - Source: {e.Source}");
                    }
            }
        }
#else
        public static List<LabApi.Loader.Features.Plugins.Plugin> ActivePlugins => [];
        
        internal static void Actor()
        {
            LogManager.Info($"{nameof(ImportManager)}: Checking for CustomItems registered in other plugins to import...");
            _alreadyLoaded = true;

            foreach (var dic in LabApi.Loader.PluginLoader.Plugins)
            {
                LogManager.Silent($"{nameof(ImportManager)}: Passing plugin {dic.Key.Name}");
                foreach (Type type in dic.Value.GetTypes())
                {
                    try
                    {
                        object[] attribs = type.GetCustomAttributes(typeof(PluginCustomItem), false);
                        if (attribs != null && attribs.Length > 0 && CustomItemTypes.Any(baseType => baseType.IsAssignableFrom(type)))
                        {
                            if (!Plugin.Instance.Config.LoadExampleapiItems)
                            {
                                switch (type.FullName)
                                {
                                    case "UncomplicatedCustomItems.Examples.ExampleCustomWeapon":
                                        continue;
                                    case "UncomplicatedCustomItems.Examples.ExampleCustomKeycard":
                                        continue;
                                    case "UncomplicatedCustomItems.Examples.ExampleCustomGrenade":
                                        continue;
                                    case "UncomplicatedCustomItems.Examples.ExampleCustomArmor":
                                        continue;
                                    case "UncomplicatedCustomItems.Examples.ExampleCustomCandy":
                                        continue;
                                };
                            }

                            if (type.FullName == "UncomplicatedCustomItems.API.ToolGun.ToolGun" && !Plugin.Instance.Config.EnableToolGun)
                                continue;

                            LogManager.Silent($"{nameof(ImportManager)}: Importing It!");
                            ActivePlugins.TryAdd(dic.Key);

                            APICustomItem Item = Activator.CreateInstance(type) as APICustomItem;
                            LogManager.Info($"{nameof(ImportManager)}: Imported CustomItem {Item.Name} ({Item.Id}) through Attribute from plugin {dic.Key.Name} - {type.FullName}");

                            APICustomItem.Register(Item);
                        }

                        if (attribs != null && attribs.Length > 0 && BannedTypes.Any(baseType => baseType.IsAssignableFrom(type)))
                            LogManager.Warn($"{type.FullName} is using a old version of the CustomItem API. For the CustomItem to be loaded and registered it MUST be updated to the new version");
                    }
                    catch (Exception e)
                    {
                        LogManager.Error($"{nameof(ImportManager)}: Error while registering CustomItem from class by Attribute: {e.GetType().FullName} - {e.Message}\nType: {type.FullName} [{dic.Key.Name}] - Source: {e.Source}");
                    }
                }
            }
        }
#endif
    }
}