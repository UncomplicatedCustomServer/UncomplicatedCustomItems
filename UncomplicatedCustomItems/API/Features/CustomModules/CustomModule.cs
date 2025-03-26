using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UncomplicatedCustomItems.Interfaces;
using UncomplicatedCustomItems.Enums;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.API.Features.CustomModules
{
#pragma warning disable IDE1006

    public abstract class CustomModule : ICustomModule
    {
        /// <summary>
        /// Gets the <see cref="CustomFlags"/> that represent this module
        /// </summary>
        public static CustomFlags Flag => CustomFlags.NotExecutable;

        /// <summary>
        /// Gets the <see cref="SummonedCustomItem"/> instance that refers to this specific Module.<br></br>
        /// This can be null if the module does not implement anything related to it!
        /// </summary>
        public SummonedCustomItem Instance { get; } = null;

        public string Name { get; } = string.Empty;

        /// <summary>
        /// Gets whether the given <see cref="CustomModule"/> has it's <see cref="Instance"/> or not
        /// </summary>
        public bool HasInstance => Instance is not null;

        private static IEnumerable<Type> _loadedCache { get; set; } = null;

        private static IEnumerable<Type> _loaded { get
            {
                _loadedCache ??= LoadAll();
                return _loadedCache;
            } 
        }

        /// <summary>
        /// Create a new instance of <see cref="CustomModule"/> without a <see cref="SummonedCustomItem"/>
        /// </summary>
        public CustomModule() => Name = GetType().Name;

        /// <summary>
        /// Create a new instance of <see cref="CustomModule"/>
        /// </summary>
        /// <param name="instance"></param>
        public CustomModule(SummonedCustomItem instance)
        {
            Instance = instance;
            Name = GetType().Name;
        }

        /// <summary>
        /// Executen the Custom Module action
        /// </summary>
        public virtual void Execute()
        { }

        public static IEnumerable<Type> LoadAll()
        {
            return Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(CustomModule)) && !t.IsAbstract);
        }

        /// <summary>
        /// Load a <see cref="List{T}"/> of <see cref="ICustomModule"/> from the given <see cref="CustomFlags"/>
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static List<ICustomModule> Load(CustomFlags Flags, SummonedCustomItem Id)
        {
            LogManager.Debug($"Loading custom modules for {Id.Item} - {Id.Item.Serial} -- {Flags} -- {_loaded.Count()}");
            List<ICustomModule> result = new();

            try
            {
                foreach (Type type in _loaded)
                {
                    PropertyInfo property = type.GetProperty("Flag", BindingFlags.Public | BindingFlags.Static);
                    if (property is not null)
                    {
                        CustomFlags flag = (CustomFlags)property.GetValue(null, null);
                        if ((Flags & flag) == flag)
                            if (type.GetConstructors()[0].GetParameters().Length > 0 && type.GetConstructors()[0].GetParameters()[0].ParameterType == typeof(SummonedCustomItem))
                                result.Add((CustomModule)Activator.CreateInstance(type, new object[] { Id }));
                            else
                                result.Add((CustomModule)Activator.CreateInstance(type));

                    }
                }
            }
            catch (Exception e)
            {
                LogManager.Error($"Failed to act CustomModule::Load(CustomFlags, SummonedCustomItem) - {e.GetType().FullName}: {e.Message}\n{e.StackTrace}");
            }

            LogManager.Silent($"rff for {Flags}: {result.Count}");
            return result;
        }

        public static ICustomModule Load(Type type, SummonedCustomItem instance)
        {
            if (type.GetConstructors()[0].GetParameters().Length > 0 && type.GetConstructors()[0].GetParameters()[0].ParameterType == typeof(SummonedCustomItem))
                return (CustomModule)Activator.CreateInstance(type, new object[] { instance });
            return (CustomModule)Activator.CreateInstance(type);
        }
    }
}