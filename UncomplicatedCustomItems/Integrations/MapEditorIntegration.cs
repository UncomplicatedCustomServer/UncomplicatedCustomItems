using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UncomplicatedCustomItems.API.Features.Manager;
using UnityEngine;

#if EXILED
using Exiled.Loader;
#else
using LabApi.Loader;
#endif

namespace UncomplicatedCustomItems.Integrations
{
    public static class MapEditorIntegration
    {
        private static bool _initialized;
        private static readonly object InitLock = new();

        private static Assembly? _merAssembly;
        private static Assembly? _tmeAssembly;

#region MapEditorReborn
        private static Type? _merSchematicType;
        private static MethodInfo? _merSpawnSchematic;
        private static MethodInfo? _merSchematicDestroy;
        private static Func<object, object?>? _merAnimationControllerGetter;
        private static Action<object, string, int>? _merPlayByIndex;
        private static Action<object, string, string>? _merPlayByName;
        private static Action<object, Vector3>? _merPositionSetter;
        private static Action<object, Vector3>? _merEulerAnglesSetter;
        private static Action<object, Vector3>? _merScaleSetter;
#endregion

#region Thaumiel Map Editor
        private static Type? _tmeSchematicType;
        private static MethodInfo? _tmeSpawnSchematic;
        private static MethodInfo? _tmeDestroySchematic;
        private static Func<object, object?>? _tmeAnimationControllerGetter;
        private static Action<object, string, int>? _tmePlayByIndex;
        private static Action<object, string, string>? _tmePlayByName;
        private static Action<object, Vector3>? _tmePositionSetter;
        private static Action<object, Vector3>? _tmeEulerAnglesSetter;
        private static Action<object, Vector3>? _tmeScaleSetter;
#endregion

        /// <summary>
        /// Gets a value indicating whether Map Editor Reborn was found.
        /// </summary>
        public static bool MERAvailable => _merAssembly != null;

        /// <summary>
        /// Gets a value indicating whether Thaumiel Map Editor was found.
        /// </summary>
        public static bool TMEAvailable => _tmeAssembly != null;

        /// <summary>
        /// Gets a value indicating whether at least one supported map editor was found.
        /// </summary>
        public static bool FoundAny => MERAvailable || TMEAvailable;

        public static void Init()
        {
            if (_initialized)
                return;

            lock (InitLock)
            {
                if (_initialized)
                    return;

                _initialized = true;

                try
                {
                    ResolveAssemblies();

                    if (MERAvailable)
                        ResolveMER();

                    if (TMEAvailable)
                        ResolveTME();

                    if (!FoundAny)
                    {
                        LogManager.Info("No supported map editor found! Install either MapEditorReborn or Thaumiel Map Editor to use the CustomModel module.");
                        return;
                    }

                    if (MERAvailable)
                        LogManager.Debug("MapEditorReborn integration loaded.");

                    if (TMEAvailable)
                        LogManager.Debug("Thaumiel Map Editor integration loaded.");
                }
                catch (Exception ex)
                {
                    LogManager.Error($"Initialization failed: {ex}");
                }
            }
        }

        private static void ResolveAssemblies()
        {
#if EXILED
            foreach (var plugin in Loader.Plugins)
            {
                if (plugin == null || plugin.Assembly == null)
                    continue;

                if (_merAssembly == null && plugin.Name is "ProjectMER" or "MapEditorReborn")
                {
                    _merAssembly = plugin.Assembly;
                    continue;
                }

                if (_tmeAssembly == null && plugin.Name == "Thaumiel Map Editor")
                {
                    _tmeAssembly = plugin.Assembly;
                }
            }
#else
            foreach (LabApi.Loader.Features.Plugins.Plugin plugin in PluginLoader.EnabledPlugins)
            {
                if (_merAssembly == null && plugin.Name is "ProjectMER" or "MapEditorReborn")
                {
                    _merAssembly = PluginLoader.Plugins[plugin];
                    continue;
                }

                if (_tmeAssembly == null && plugin.Name == "Thaumiel Map Editor")
                {
                    _tmeAssembly = PluginLoader.Plugins[plugin];
                }
            }
#endif
        }

        private static void ResolveMER()
        {
            Assembly? assembly = _merAssembly;
            if (assembly == null)
                return;

            Type? spawnerType = assembly.GetType("ProjectMER.Features.ObjectSpawner");
            Type? schematicType = assembly.GetType("ProjectMER.Features.Objects.SchematicObject");
            Type? animationControllerType = assembly.GetType("ProjectMER.Features.AnimationController");

            if (spawnerType == null || schematicType == null || animationControllerType == null)
            {
                LogManager.Warn("MapEditorReborn API types not found. The plugin layout may have changed.");
                _merAssembly = null;
                return;
            }

            _merSchematicType = schematicType;

            _merSpawnSchematic = GetMethod(spawnerType, "SpawnSchematic", typeof(string), typeof(Vector3), typeof(Vector3), typeof(Vector3));
            _merSchematicDestroy = GetMethod(schematicType, "Destroy");

            _merAnimationControllerGetter = BuildInstanceGetter(schematicType, schematicType.GetProperty("AnimationController"));
            _merPlayByIndex = BuildInstanceAction<string, int>(animationControllerType, GetMethod(animationControllerType, "Play", typeof(string), typeof(int)));
            _merPlayByName = BuildInstanceAction<string, string>(animationControllerType, GetMethod(animationControllerType, "Play", typeof(string), typeof(string)));

            _merPositionSetter = BuildInstanceSetter<Vector3>(schematicType, schematicType.GetProperty("Position"));
            _merEulerAnglesSetter = BuildInstanceSetter<Vector3>(schematicType, schematicType.GetProperty("EulerAngles"));
            _merScaleSetter = BuildInstanceSetter<Vector3>(schematicType, schematicType.GetProperty("Scale"));

            if (!ResolveRequired("MapEditorReborn", ("SpawnSchematic", _merSpawnSchematic), ("Destroy", _merSchematicDestroy), ("AnimationController", _merAnimationControllerGetter), ("Play(string,int)", _merPlayByIndex), ("Play(string,string)", _merPlayByName), ("Position", _merPositionSetter), ("EulerAngles", _merEulerAnglesSetter), ("Scale", _merScaleSetter)))
            {
                _merAssembly = null;
            }
        }

        private static void ResolveTME()
        {
            Assembly? assembly = _tmeAssembly;
            if (assembly == null)
                return;

            Type? loaderType = assembly.GetType("ThaumielMapEditor.API.Helpers.Loader");
            Type? schematicType = assembly.GetType("ThaumielMapEditor.API.Data.SchematicData");
            Type? animationControllerType = assembly.GetType("ThaumielMapEditor.API.Animation.AnimationController");

            if (loaderType == null || schematicType == null || animationControllerType == null)
            {
                LogManager.Warn("Thaumiel Map Editor API types not found. The plugin layout may have changed.");
                _tmeAssembly = null;
                return;
            }

            _tmeSchematicType = schematicType;

            _tmeSpawnSchematic = GetMethod(loaderType, "SpawnSchematic", typeof(string), typeof(Vector3), typeof(Quaternion), typeof(Vector3));
            _tmeDestroySchematic = GetMethod(loaderType, "DestroySchematic", [_tmeSchematicType]);

            _tmeAnimationControllerGetter = BuildInstanceGetter(schematicType, schematicType.GetProperty("AnimationController"));
            _tmePlayByIndex = BuildInstanceAction<string, int>(animationControllerType, GetMethod(animationControllerType, "Play", typeof(string), typeof(int)));
            _tmePlayByName = BuildInstanceAction<string, string>(animationControllerType, GetMethod(animationControllerType, "Play", typeof(string), typeof(string)));

            _tmePositionSetter = BuildInstanceSetter<Vector3>(schematicType, schematicType.GetProperty("Position"));
            _tmeEulerAnglesSetter = BuildInstanceSetter<Vector3>(schematicType, schematicType.GetProperty("EulerAngles"));
            _tmeScaleSetter = BuildInstanceSetter<Vector3>(schematicType, schematicType.GetProperty("Scale"));

            if (!ResolveRequired("Thaumiel Map Editor", ("SpawnSchematic", _tmeSpawnSchematic), ("DestroySchematic", _tmeDestroySchematic), ("AnimationController", _tmeAnimationControllerGetter), ("Play(string,int)", _tmePlayByIndex), ("Play(string,string)", _tmePlayByName), ("Position", _tmePositionSetter), ("EulerAngles", _tmeEulerAnglesSetter), ("Scale", _tmeScaleSetter)))
            {
                _tmeAssembly = null;
            }
        }

        private static bool ResolveRequired(string plugin, params (string Name, object? Member)[] required)
        {
            string missing = string.Join(", ", required.Where(member => member.Member is null).Select(member => member.Name));
            if (missing.Length == 0)
                return true;

            LogManager.Warn($"{plugin} API is incomplete, missing: {missing}. The CustomModel module will be disabled for {plugin}.");
            return false;
        }

        /// <summary>
        /// Spawns a schematic using the first available map editor (MER takes priority).
        /// </summary>
        /// <returns>A handle to the spawned schematic, or <see langword="null"/> if spawning failed.</returns>
        public static object? SpawnSchematic(string name, Vector3 position, Vector3 eulerAngles, Vector3 scale)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                LogManager.Warn("Schematic name is empty.");
                return null;
            }

            if (MERAvailable)
                return SpawnSchematicMER(name, position, eulerAngles, scale);

            if (TMEAvailable)
                return SpawnSchematicTME(name, position, eulerAngles, scale);

            LogManager.Warn("No supported map editor found! Install either MapEditorReborn or Thaumiel Map Editor to use the CustomModel module.");
            return null;
        }

        /// <summary>
        /// Plays an animation state on the given schematic.
        /// </summary>
        /// <param name="schematic">The schematic handle returned by <see cref="SpawnSchematic"/>.</param>
        /// <param name="stateName">The animation state to play.</param>
        /// <param name="animatorName">An optional animator name to target when the schematic has multiple animators.</param>
        public static void PlayAnimation(object schematic, string stateName, string animatorName = "")
        {
            if (schematic == null || string.IsNullOrWhiteSpace(stateName) || IsSchematicDestroyed(schematic))
                return;

            if (IsMER(schematic))
            {
                if (_merAnimationControllerGetter?.Invoke(schematic) is object controller)
                    InvokePlay(controller, _merPlayByIndex, _merPlayByName, stateName, animatorName, "MapEditorReborn");
            }
            else if (IsTME(schematic))
            {
                if (_tmeAnimationControllerGetter?.Invoke(schematic) is object controller)
                    InvokePlay(controller, _tmePlayByIndex, _tmePlayByName, stateName, animatorName, "Thaumiel Map Editor");
            }
        }

        /// <summary>
        /// Updates the world position of the given schematic.
        /// </summary>
        public static void SetPosition(object schematic, Vector3 position)
        {
            if (schematic == null || IsSchematicDestroyed(schematic))
                return;

            if (IsMER(schematic))
            {
                _merPositionSetter?.Invoke(schematic, position);
            }
            else if (IsTME(schematic))
                _tmePositionSetter?.Invoke(schematic, position);
        }

        /// <summary>
        /// Updates the rotation of the given schematic.
        /// </summary>
        public static void SetRotation(object schematic, Vector3 eulerAngles)
        {
            if (schematic == null || IsSchematicDestroyed(schematic))
                return;

            if (IsMER(schematic))
            {
                _merEulerAnglesSetter?.Invoke(schematic, eulerAngles);
            }
            else if (IsTME(schematic))
                _tmeEulerAnglesSetter?.Invoke(schematic, eulerAngles);
        }

        /// <summary>
        /// Updates the scale of the given schematic.
        /// </summary>
        public static void SetScale(object schematic, Vector3 scale)
        {
            if (schematic == null || IsSchematicDestroyed(schematic))
                return;

            if (IsMER(schematic))
            {
                _merScaleSetter?.Invoke(schematic, scale);
            }
            else if (IsTME(schematic))
                _tmeScaleSetter?.Invoke(schematic, scale);
        }

        /// <summary>
        /// Destroys the given schematic and cleans up its resources.
        /// </summary>
        public static void DestroySchematic(object schematic)
        {
            if (schematic == null || IsSchematicDestroyed(schematic))
                return;

            try
            {
                if (IsMER(schematic))
                {
                    _merSchematicDestroy?.Invoke(schematic, null);
                }
                else if (IsTME(schematic))
                {
                    _tmeDestroySchematic?.Invoke(null, [schematic]);
                }
            }
            catch (Exception ex)
            {
                LogManager.Warn($"Failed to destroy custom model: {ex}");
            }
        }

        private static object? SpawnSchematicMER(string name, Vector3 position, Vector3 eulerAngles, Vector3 scale)
        {
            try
            {
                return _merSpawnSchematic?.Invoke(null, [name, position, eulerAngles, scale]);
            }
            catch (Exception ex)
            {
                LogManager.Warn($"MapEditorReborn spawn failed: {ex}");
                return null;
            }
        }

        private static object? SpawnSchematicTME(string name, Vector3 position, Vector3 eulerAngles, Vector3 scale)
        {
            try
            {
                return _tmeSpawnSchematic?.Invoke(null, [name, position, Quaternion.Euler(eulerAngles), scale]);
            }
            catch (Exception ex)
            {
                LogManager.Warn($"Thaumiel Map Editor spawn failed: {ex}");
                return null;
            }
        }

        private static bool IsMER(object schematic)
            => _merSchematicType != null && _merSchematicType.IsInstanceOfType(schematic);

        private static bool IsTME(object schematic)
            => _tmeSchematicType != null && _tmeSchematicType.IsInstanceOfType(schematic);

        public static bool IsSchematicDestroyed(object schematic)
            => schematic is UnityEngine.Object unity && unity == null;

        /// <summary>
        /// Gets whether a schematic handle is a live Unity component whose transform
        /// can be re-parented (MER schematic objects are MonoBehaviours).
        /// </summary>
        public static bool CanParent(object schematic)
            => schematic is Component component && component != null;

        /// <summary>
        /// Parents a schematic to a transform and applies the given local transform,
        /// so it inherits the parent's position and rotation every frame for free.
        /// </summary>
        public static void ParentTo(object schematic, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            if (schematic is not Component component || component == null || parent == null)
                return;

            component.transform.SetParent(parent, false);
            component.transform.localPosition = localPosition;
            component.transform.localRotation = localRotation;
            component.transform.localScale = localScale;
        }

        /// <summary>
        /// Detaches a schematic from its parent and restores it to root space.
        /// </summary>
        public static void Unparent(object schematic)
        {
            if (schematic is not Component component || component == null)
                return;

            if (component.transform.parent != null)
                component.transform.SetParent(null, false);
        }

        private static void InvokePlay(object controller, Action<object, string, int>? playByIndex, Action<object, string, string>? playByName, string stateName, string animatorName, string plugin)
        {
            try
            {
                if (!string.IsNullOrEmpty(animatorName))
                {
                    playByName?.Invoke(controller, stateName, animatorName);
                }
                else
                    playByIndex?.Invoke(controller, stateName, 0);
            }
            catch (Exception ex)
            {
                LogManager.Warn($"{plugin} failed to play animation '{stateName}': {ex}");
            }
        }

        private static MethodInfo? GetMethod(Type type, string name, params Type[] parameterTypes)
            => type.GetMethod(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, parameterTypes, null);

        private static Action<object, T>? BuildInstanceSetter<T>(Type instanceType, PropertyInfo? property)
        {
            if (property == null || property.GetSetMethod(nonPublic: true) == null)
                return null;

            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression value = Expression.Parameter(typeof(T), "value");
            MethodCallExpression call = Expression.Call(Expression.Convert(instance, instanceType), property.GetSetMethod(nonPublic: true)!, value);
            return Expression.Lambda<Action<object, T>>(call, instance, value).Compile();
        }

        private static Func<object, object?>? BuildInstanceGetter(Type instanceType, PropertyInfo? property)
        {
            if (property == null || property.GetGetMethod(nonPublic: true) == null)
                return null;

            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            MemberExpression member = Expression.Property(Expression.Convert(instance, instanceType), property);
            return Expression.Lambda<Func<object, object?>>(Expression.Convert(member, typeof(object)), instance).Compile();
        }

        private static Action<object, T1, T2>? BuildInstanceAction<T1, T2>(Type instanceType, MethodInfo? method)
        {
            if (method == null)
                return null;

            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression arg1 = Expression.Parameter(typeof(T1), "arg1");
            ParameterExpression arg2 = Expression.Parameter(typeof(T2), "arg2");
            MethodCallExpression call = Expression.Call(Expression.Convert(instance, instanceType), method, arg1, arg2);
            return Expression.Lambda<Action<object, T1, T2>>(call, instance, arg1, arg2).Compile();
        }
    }
}
