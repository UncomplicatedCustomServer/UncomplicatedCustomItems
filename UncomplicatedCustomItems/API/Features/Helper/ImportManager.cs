using Exiled.API.Interfaces;
using Exiled.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UncomplicatedCustomItems.API.Attributes;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Interfaces;
using UncomplicatedCustomItems.Extensions;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.Manager
{
    internal class ImportManager
    {
        public static List<IPlugin<IConfig>> ActivePlugins => new();

        public const float WaitingTime = 5f;

        private static bool _alreadyLoaded = false;

        public static void Init()
        {
            if (_alreadyLoaded)
                return;

            ActivePlugins.Clear();
            // Call a delayed task
            Task.Run(Actor);
        }

        private static void Actor()
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
                        if (attribs != null && attribs.Length > 0 && (type.IsSubclassOf(typeof(ICustomItem)) || type.IsSubclassOf(typeof(CustomItem))))
                        {
                            LogManager.Silent($"{nameof(ImportManager.Actor)}: Importing It!");
                            ActivePlugins.TryAdd(plugin);

                            ICustomItem Item = Activator.CreateInstance(type) as ICustomItem;
                            LogManager.Info($"{nameof(ImportManager.Actor)}: Imported CustomItem {Item.Name} ({Item.Id}) through Attribute from plugin {plugin.Name} (v{plugin.Version})");
                            CustomItem.Register(Item);
                        }
                    }
                    catch (Exception e)
                    {
                        LogManager.Error($"{nameof(ImportManager.Actor)}: Error while registering CustomItem from class by Attribute: {e.GetType().FullName} - {e.Message}\nType: {type.FullName} [{plugin.Name}] - Source: {e.Source}");
                    }
            }
        }
    }
}
