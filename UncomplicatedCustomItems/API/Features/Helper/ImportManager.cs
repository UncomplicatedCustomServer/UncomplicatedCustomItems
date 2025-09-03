using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UncomplicatedCustomItems.API.Attributes;
using UncomplicatedCustomItems.API.Interfaces;
using UncomplicatedCustomItems.API.Extensions;

namespace UncomplicatedCustomItems.API.Features.Helper
{
    internal class ImportManager
    {
        public static List<LabApi.Loader.Features.Plugins.Plugin> ActivePlugins => [];

        private static bool _alreadyLoaded = false;

        public static void Init()
        {
            if (_alreadyLoaded)
                return;

            ActivePlugins.Clear();
            // Call a delayed task
            Task.Run(Actor);
        }

        internal static void Actor()
        {
            LogManager.Info($"{nameof(ImportManager)}: Checking for CustomItems registered in other plugins to import...");

            _alreadyLoaded = true;

            foreach (var dic in LabApi.Loader.PluginLoader.Plugins)
            {
                LogManager.Silent($"{nameof(ImportManager)}: Passing plugin {dic.Key.Name}");
                foreach (Type type in dic.Value.GetTypes())
                    try
                    {
                        object[] attribs = type.GetCustomAttributes(typeof(PluginCustomItem), false);
                        if (attribs != null && attribs.Length > 0 && (type.IsSubclassOf(typeof(ICustomItem)) || type.IsSubclassOf(typeof(CustomItem))))
                        {
                            LogManager.Silent($"{nameof(ImportManager)}: Importing It!");
                            ActivePlugins.TryAdd<LabApi.Loader.Features.Plugins.Plugin>(dic.Key);

                            ICustomItem Item = Activator.CreateInstance(type) as ICustomItem;
                            LogManager.Info($"{nameof(ImportManager)}: Imported CustomItem {Item.Name} ({Item.Id}) through Attribute from plugin {dic.Key.Name} (v{dic.Key.Version})");
                            if (Item.Name is "ToolGun" && Item.Id is 20 && !Plugin.Instance.Config.EnableToolGun)
                                return;

                            CustomItem.Register(Item);
                        }
                    }
                    catch (Exception e)
                    {
                        LogManager.Error($"{nameof(ImportManager)}: Error while registering CustomItem from class by Attribute: {e.GetType().FullName} - {e.Message}\nType: {type.FullName} [{dic.Key.Name}] - Source: {e.Source}");
                    }
            }
        }
    }
}