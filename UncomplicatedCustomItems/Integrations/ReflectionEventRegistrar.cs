using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LabApi.Loader.Features.Misc;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.Integrations
{
#nullable enable
    internal sealed class EventToken
    {
        public string Id { get; }
        internal EventInfo EventInfo { get; }
        internal object? Target { get; }
        internal Delegate Delegate { get; }

        internal EventToken(string id, EventInfo evt, object? target, Delegate del)
        {
            Id = id;
            EventInfo = evt;
            Target = target;
            Delegate = del;
        }

        /// <summary>
        /// Unregisters this handler from the event.
        /// </summary>
        public void Unregister()
        {
            try
            {
                EventInfo.RemoveEventHandler(Target, Delegate);
            }
            catch (Exception ex)
            {
                LogManager.Debug($"{nameof(EventToken)}.{nameof(Unregister)} failed: {ex}");
            }
        }
    }

    internal static class ReflectionEventRegistrar
    {
        private static readonly ConcurrentDictionary<string, EventToken> _tokens = new();

        private static Type? FindType(string name)
        {
            foreach (LabApi.Loader.Features.Plugins.Plugin plugin in LabApi.Loader.PluginLoader.EnabledPlugins)
            {
                plugin.TryGetLoadedAssembly(out Assembly asm);
                Type? t = asm.GetType(name, false);
                if (t != null)
                    return t;
            }

            return null;
        }

        /// <summary>
        /// Register a typed event handler
        /// </summary>
        public static EventToken? RegisterEventHandler<TEventArgs>(string typeFullName, string eventName, Action<TEventArgs> handler)
        {
            if (string.IsNullOrEmpty(typeFullName) || string.IsNullOrEmpty(eventName) || handler == null)
                return null;

            Type? targetType = FindType(typeFullName);
            if (targetType == null)
            {
                LogManager.Debug($"{nameof(ReflectionEventRegistrar)}: Type '{typeFullName}' not found.");
                return null;
            }

            EventInfo? evt = targetType.GetEvent(eventName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            if (evt == null)
            {
                LogManager.Debug($"{nameof(ReflectionEventRegistrar)}: Event '{eventName}' not found on '{typeFullName}'.");
                return null;
            }

            Delegate? del = CreateDelegateForHandler(evt.EventHandlerType!, handler);
            if (del == null)
            {
                LogManager.Debug($"{nameof(ReflectionEventRegistrar)}: Couldn't create delegate for event '{eventName}'.");
                return null;
            }

            try
            {
                LogManager.Debug($"Registered Event {evt.EventHandlerType}");
                object? addTarget = evt.AddMethod?.IsStatic == true ? null : GetInstanceForType(targetType);
                evt.AddEventHandler(addTarget, del);

                string id = $"{typeFullName}.{eventName}.{Guid.NewGuid():N}";
                EventToken? token = new(id, evt, addTarget, del);
                _tokens[id] = token;
                return token;
            }
            catch (Exception ex)
            {
                LogManager.Debug($"{nameof(ReflectionEventRegistrar)}: Failed to add event handler: {ex}");
                return null;
            }
        }

        public static EventToken? RegisterEventHandler(string typeFullName, string eventName, Action<object> handler)
        {
            if (handler == null)
                return null;

            return RegisterEventHandler<object>(typeFullName, eventName, obj =>
            {
                handler(obj!);
            });
        }

        /// <summary>
        /// Register using an existing MethodInfo. `ishandlerTarget`  null for static methods.
        /// The method signature must be compatible with the event delegate.
        /// </summary>
        public static EventToken? RegisterEventHandlerByMethod(string typeFullName, string eventName, object? handlerTarget, MethodInfo handlerMethod)
        {
            if (string.IsNullOrEmpty(typeFullName) || string.IsNullOrEmpty(eventName) || handlerMethod == null)
                return null;

            Type? targetType = FindType(typeFullName);
            if (targetType == null)
                return null;

            EventInfo? evt = targetType.GetEvent(eventName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            if (evt == null)
                return null;

            Type handlerType = evt.EventHandlerType!;
            try
            {
                Delegate? del = Delegate.CreateDelegate(handlerType, handlerTarget, handlerMethod, throwOnBindFailure: false);
                if (del == null)
                {
                    LogManager.Debug($"{nameof(ReflectionEventRegistrar)}: Could not create delegate from MethodInfo (signature mismatch).");
                    return null;
                }
                
                LogManager.Debug($"Registered Event {evt.EventHandlerType}");
                object? addTarget = evt.AddMethod!.IsStatic ? null : GetInstanceForType(targetType);
                if (addTarget == null && !evt.AddMethod.IsStatic && handlerTarget != null)
                    addTarget = handlerTarget;

                evt.AddEventHandler(addTarget, del);

                string id = $"{typeFullName}.{eventName}.{Guid.NewGuid():N}";
                EventToken? token = new(id, evt, addTarget, del);
                _tokens[id] = token;
                return token;
            }
            catch (Exception ex)
            {
                LogManager.Debug($"{nameof(ReflectionEventRegistrar)}: RegisterEventHandlerByMethod failed: {ex}");
                return null;
            }
        }

        private static Delegate? CreateDelegateForHandler<TEventArgs>(Type handlerType, Action<TEventArgs> action)
        {
            if (handlerType == null)
                return null;

            MethodInfo? invoke = handlerType.GetMethod("Invoke");
            if (invoke == null)
                return null;

            ParameterInfo[] invokeParams = invoke.GetParameters();
            ParameterExpression[] paramExprs = invokeParams.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();

            if (paramExprs.Length == 0)
                return null;

            ParameterExpression eventArgExpr = paramExprs[paramExprs.Length - 1];
            UnaryExpression converted = Expression.Convert(eventArgExpr, typeof(TEventArgs));
            ConstantExpression actionConst = Expression.Constant(action);
            InvocationExpression call = Expression.Invoke(actionConst, converted);

            Expression body = call;
            if (invoke.ReturnType != typeof(void))
            {
                DefaultExpression defaultReturn = Expression.Default(invoke.ReturnType);
                body = Expression.Block(call, defaultReturn);
            }

            try
            {
                LambdaExpression lambda = Expression.Lambda(handlerType, body, paramExprs);
                return lambda.Compile();
            }
            catch (Exception ex)
            {
                LogManager.Debug($"{nameof(ReflectionEventRegistrar)}: Exception building delegate: {ex}");
                return null;
            }
        }

        private static object? GetInstanceForType(Type t)
        {
            string[] props = ["Instance", "Default", "Singleton"];
            foreach (string name in props)
            {
                PropertyInfo? pi = t.GetProperty(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
                if (pi != null)
                    return pi.GetValue(null);
            }

            string[] fields = ["Instance", "Default", "Singleton"];
            foreach (string name in fields)
            {
                FieldInfo? fi = t.GetField(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
                if (fi != null)
                    return fi.GetValue(null);
            }

            return null;
        }
    }
}
